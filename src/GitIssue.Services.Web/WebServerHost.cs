using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Exceptionable;
using GitIssue.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration.Json;

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

            WebApplication application = builder.Build();
            IssueWorkspace workspace;
            try
            {
                workspace = new IssueWorkspace(IssueManager.Open(repositoryPath, request.IssueFolderName));
            }
            catch (Exception exception) when (exception is not IssueOperationException)
            {
                ((IDisposable)application).Dispose();
                throw new IssueOperationException(
                    "Unable to open the GitIssue repository. Run gitissue init and start the server from the repository root. " + exception.GetBaseException().Message,
                    exception);
            }

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

            application.MapGet("/health", () => Results.Json(new { status = "ok" }));
            application.MapGet("/", () => Results.Content(WebServerHost.homePage(request), "text/html", Encoding.UTF8));
            return application;
        }

        private static string homePage(WebServeRequest request)
        {
            return $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                  <meta charset="utf-8" />
                  <title>GitIssue</title>
                </head>
                <body>
                  <h1>GitIssue</h1>
                  <p>The web server is running for <code>{WebUtility.HtmlEncode(request.IssueFolderName)}</code> in <code>{WebUtility.HtmlEncode(request.RepositoryPath)}</code>.</p>
                </body>
                </html>
                """;
        }
    }
}
