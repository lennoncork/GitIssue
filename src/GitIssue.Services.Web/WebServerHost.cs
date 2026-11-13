using System;
using System.IO;
using System.Linq;
using Exceptionable;
using GitIssue.Services;
using GitIssue.Services.Web.Components;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;

namespace GitIssue.Services.Web
{
    /// <summary>
    ///     Builds the local GitIssue website
    /// </summary>
    public static class WebServerHost
    {
        /// <summary>
        ///     Creates the web application for a parsed request
        /// </summary>
        /// <param name="request">the serve request</param>
        /// <returns>the web application</returns>
        [Throws<IssueOperationException>]
        public static WebApplication Create(WebServeRequest request)
        {
            string repositoryPath = Path.GetFullPath(request.RepositoryPath);
            WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = Array.Empty<string>(),
                ApplicationName = "gitissue",
                ContentRootPath = repositoryPath,
            });
            foreach (JsonConfigurationSource source in builder.Configuration.Sources.OfType<JsonConfigurationSource>().ToArray())
            {
                builder.Configuration.Sources.Remove(source);
            }

            builder.WebHost.UseUrls(request.Url);
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddRazorComponents();

            IssueWorkspace workspace;
            try
            {
                workspace = new IssueWorkspace(IssueManager.Open(repositoryPath, request.IssueFolderName));
            }
            catch (Exception exception) when (exception is not IssueOperationException)
            {
                throw new IssueOperationException(
                    "Unable to open the GitIssue repository. Run gitissue init and start the server from the repository root. " + exception.GetBaseException().Message,
                    exception);
            }

            builder.Services.AddSingleton(workspace);
            WebApplication application = builder.Build();
            application.Lifetime.ApplicationStopping.Register(() => workspace.Dispose());
            if (!string.IsNullOrEmpty(request.Token))
            {
                string expected = $"Bearer {request.Token}";
                application.Use(async (context, next) =>
                {
                    if (!string.Equals(context.Request.Headers.Authorization.ToString(), expected, StringComparison.Ordinal))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }

                    await next();
                });
            }

            application.UseAntiforgery();
            application.MapGet("/health", () => Results.Json(new { status = "ok" }));
            application.MapRazorComponents<App>();
            return application;
        }
    }
}
