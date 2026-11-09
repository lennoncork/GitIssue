using System;
using System.IO;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Exceptionable;
using GitIssue.Services;
using GitIssue.Services.Mcp.Tools;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Serilog;
using Serilog.Events;

namespace GitIssue.Services.Mcp
{
    /// <summary>
    ///     Hosts the GitIssue MCP server on standard input and output
    /// </summary>
    public static class McpServerHost
    {
        /// <summary>
        ///     The server name advertised to MCP clients
        /// </summary>
        public const string ServerName = "gitissue";

        /// <summary>
        ///     Builds the MCP host without starting it
        /// </summary>
        /// <param name="options">the serve options</param>
        /// <returns>the web application hosting the stdio server</returns>
        [Throws<IssueOperationException>]
        public static WebApplication CreateHost(McpServeOptions options)
        {
            string repositoryPath = Path.GetFullPath(options.RepositoryPath);
            if (!Directory.Exists(repositoryPath))
            {
                throw new IssueOperationException($"Repository path \"{repositoryPath}\" does not exist.");
            }

            WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = Array.Empty<string>(),
                ApplicationName = McpServerHost.ServerName,
                ContentRootPath = repositoryPath,
            });
            builder.WebHost.ConfigureServices(services =>
            {
                foreach (ServiceDescriptor descriptor in services.Where(service => service.ServiceType == typeof(IServer)).ToArray())
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton<IServer>(new StdioTransportServer());
            });
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole(console =>
            {
                console.LogToStandardErrorThreshold = LogLevel.Trace;
            });

            Serilog.ILogger logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
                .CreateLogger();

            ((IHostApplicationBuilder)builder).ConfigureContainer(new AutofacServiceProviderFactory(), container =>
            {
                container.RegisterInstance(logger).As<Serilog.ILogger>().SingleInstance();
                container.Register(_ => RepositoryRoot.Open(repositoryPath, options.IssueFolderName))
                    .As<RepositoryRoot>()
                    .SingleInstance();
                container.RegisterType<IssueWorkspace>().AsSelf().SingleInstance();
                container.Register(context =>
                {
                    IComponentContext components = context.Resolve<IComponentContext>();
                    return new IssueSession(() => components.Resolve<IssueWorkspace>());
                }).AsSelf().SingleInstance();
                container.RegisterModule<GitIssueModule>();
            });

            Version version = typeof(McpServerHost).Assembly.GetName().Version ?? new Version(1, 0, 0);
            builder.Services
                .AddMcpServer(server =>
                {
                    server.ServerInfo = new Implementation
                    {
                        Name = McpServerHost.ServerName,
                        Version = version.ToString(),
                    };
                    server.ServerInstructions =
                        "GitIssue stores issues in the working tree. Call list_fields before writing so every field name matches the configured schema. Issue keys identify documents. A feature is an issue whose configured fields, such as Type or Labels, say so. Write tools change the working tree and do not create a git commit.";
                })
                .WithStdioServerTransport()
                .WithToolsFromAssembly(typeof(McpServerHost).Assembly)
                .WithResourcesFromAssembly(typeof(McpServerHost).Assembly)
                .WithPromptsFromAssembly(typeof(McpServerHost).Assembly);

            return builder.Build();
        }

        /// <summary>
        ///     Runs the MCP server until the client closes the input stream
        /// </summary>
        /// <param name="options">the serve options</param>
        [Throws<IssueOperationException>]
        public static void Run(McpServeOptions options)
        {
            McpServerHost.CreateHost(options).Run();
        }
    }

    /// <summary>
    ///     Options for the stdio MCP server
    /// </summary>
    public sealed class McpServeOptions
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="McpServeOptions" /> class
        /// </summary>
        /// <param name="repositoryPath">the working directory</param>
        /// <param name="issueFolderName">the issue directory name</param>
        public McpServeOptions(string repositoryPath, string issueFolderName)
        {
            this.RepositoryPath = repositoryPath;
            this.IssueFolderName = issueFolderName;
        }

        /// <summary>
        ///     Gets the working directory
        /// </summary>
        public string RepositoryPath { get; }

        /// <summary>
        ///     Gets the issue directory name
        /// </summary>
        public string IssueFolderName { get; }
    }
}
