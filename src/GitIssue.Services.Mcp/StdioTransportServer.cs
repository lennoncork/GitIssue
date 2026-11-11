using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;

namespace GitIssue.Services.Mcp
{
    /// <summary>
    ///     Keeps the ASP.NET host alive without opening an HTTP port.
    ///     The MCP server speaks on standard input and output.
    /// </summary>
    internal sealed class StdioTransportServer : IServer
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StdioTransportServer" /> class
        /// </summary>
        public StdioTransportServer()
        {
            this.Features = new FeatureCollection();
        }

        /// <inheritdoc />
        public IFeatureCollection Features { get; }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        /// <inheritdoc />
        public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
            where TContext : notnull
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
