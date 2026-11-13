using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Issues.Json;
using GitIssue.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Playwright;
using NUnit.Framework;

namespace GitIssue.Services.Web.Tests
{
    [TestFixture]
    public class WebBrowserTests
    {
        [Test]
        public async Task LandingPage_ShowsTrackedIssueAndSupportsSearchingAndPaging()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            IssueDocument tracked;
            using (IssueWorkspace workspace = fixture.Open())
            {
                tracked = await workspace.CreateIssueAsync("Tracked task", "Tracked description", CancellationToken.None);
                await workspace.TrackIssueAsync(tracked.Key, CancellationToken.None);
                for (int index = 0; index < 11; index++)
                {
                    await workspace.CreateIssueAsync($"Task {index:00}", $"Description {index:00}", CancellationToken.None);
                }
            }

            WebServeRequest request = WebServerHostTests.request(fixture.Directory, WebServerHostTests.freePort(), null);
            await using WebApplication application = WebServerHost.Create(request);
            await application.StartAsync();
            try
            {
                using IPlaywright playwright = await Playwright.CreateAsync();
                await using IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
                IPage page = await browser.NewPageAsync();
                await page.GotoAsync(request.Url);

                Assert.That(await page.TitleAsync(), Is.EqualTo("GitIssue"));
                Assert.That(await page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "GitIssue", Level = 1 }).CountAsync(), Is.EqualTo(1));
                Assert.That(await page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Tracked issue" }).CountAsync(), Is.EqualTo(1));
                Assert.That(await page.GetByText("Tracked task", new PageGetByTextOptions { Exact = false }).CountAsync(), Is.GreaterThanOrEqualTo(1));
                Assert.That(await page.Locator("tbody tr").CountAsync(), Is.EqualTo(10));
                await page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Next" }).ClickAsync();
                Assert.That(await page.Locator("tbody tr").CountAsync(), Is.EqualTo(2));
                Assert.That(await page.GetByText("Page 2 of 2").CountAsync(), Is.EqualTo(1));
                await page.GetByLabel("Search issues").FillAsync("description 05");
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Search" }).ClickAsync();
                Assert.That(await page.Locator("tbody tr").CountAsync(), Is.EqualTo(1));
                Assert.That(await page.Locator("tbody").InnerTextAsync(), Does.Contain("Task 05"));
                await page.GetByLabel("Search issues").FillAsync("not present");
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Search" }).ClickAsync();
                Assert.That(await page.GetByText("No issues found.").CountAsync(), Is.EqualTo(1));
            }
            finally
            {
                await application.StopAsync();
            }
        }

        [Test]
        public async Task IssuePage_UpdatesFieldsAndReportsValidationErrors()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            IssueDocument created;
            using (IssueWorkspace workspace = fixture.Open())
            {
                created = await workspace.CreateIssueAsync("Original title", "Original description", CancellationToken.None);
            }

            WebServeRequest request = WebServerHostTests.request(fixture.Directory, WebServerHostTests.freePort(), null);
            await using WebApplication application = WebServerHost.Create(request);
            await application.StartAsync();
            try
            {
                using IPlaywright playwright = await Playwright.CreateAsync();
                await using IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
                IPage page = await browser.NewPageAsync();
                await page.GotoAsync(request.Url);
                await page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = created.Key }).ClickAsync();
                Assert.That(await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save Title" }).CountAsync(), Is.EqualTo(0));
                Assert.That(await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).CountAsync(), Is.EqualTo(1));
                LocatorBoundingBoxResult? save = await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).BoundingBoxAsync();
                LocatorBoundingBoxResult? revert = await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Revert", Exact = true }).BoundingBoxAsync();
                LocatorBoundingBoxResult? cancel = await page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Cancel", Exact = true }).BoundingBoxAsync();
                Assert.That(save, Is.Not.Null);
                Assert.That(revert, Is.Not.Null);
                Assert.That(cancel, Is.Not.Null);
                Assert.That(revert!.Y, Is.EqualTo(save!.Y).Within(1));
                Assert.That(cancel!.Y, Is.EqualTo(save.Y).Within(1));
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
                Assert.That(await page.GetByRole(AriaRole.Status).InnerTextAsync(), Does.Contain("No changes to save"));
                Assert.That(await page.Locator("label.modified").CountAsync(), Is.EqualTo(0));

                await page.GetByLabel("Title", new PageGetByLabelOptions { Exact = true }).FillAsync("Updated title");
                await page.GetByLabel("Description", new PageGetByLabelOptions { Exact = true }).FillAsync("Updated description");
                await page.Locator("label.modified[for='Title']").WaitForAsync();
                await page.Locator("label.modified[for='Description']").WaitForAsync();
                Assert.That(await page.Locator("label[for='Title']").EvaluateAsync<string>("node => getComputedStyle(node).color"), Is.EqualTo("rgb(255, 106, 0)"));
                Assert.That(await page.Locator("label.modified[for='Author']").CountAsync(), Is.EqualTo(0));
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Revert", Exact = true }).ClickAsync();
                Assert.That(await page.GetByLabel("Title", new PageGetByLabelOptions { Exact = true }).InputValueAsync(), Is.EqualTo("Original title"));
                Assert.That(await page.GetByLabel("Description", new PageGetByLabelOptions { Exact = true }).InputValueAsync(), Is.EqualTo("Original description"));
                Assert.That(await page.Locator("label.modified").CountAsync(), Is.EqualTo(0));

                await page.GetByLabel("Title", new PageGetByLabelOptions { Exact = true }).FillAsync("Updated title");
                await page.GetByLabel("Description", new PageGetByLabelOptions { Exact = true }).FillAsync("Updated description");
                await page.GetByLabel("Labels", new PageGetByLabelOptions { Exact = true }).FillAsync("+feature");
                await page.Locator("label.modified[for='Labels']").WaitForAsync();
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
                Assert.That(await page.GetByRole(AriaRole.Status).InnerTextAsync(), Does.Contain("Changes saved"));
                await page.ReloadAsync();
                Assert.That(await page.GetByLabel("Title", new PageGetByLabelOptions { Exact = true }).InputValueAsync(), Is.EqualTo("Updated title"));
                Assert.That(await page.GetByLabel("Description", new PageGetByLabelOptions { Exact = true }).InputValueAsync(), Is.EqualTo("Updated description"));
                Assert.That(await page.GetByText("Current value:", new PageGetByTextOptions { Exact = false }).First.InnerTextAsync(), Does.Contain("feature"));
                Assert.That(await page.Locator("label.modified").CountAsync(), Is.EqualTo(0));

                await page.GetByLabel("Title", new PageGetByLabelOptions { Exact = true }).FillAsync("Should not persist");
                await page.GetByLabel("Labels", new PageGetByLabelOptions { Exact = true }).FillAsync("[valid, invalid label]");
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
                Assert.That(await page.GetByRole(AriaRole.Alert).CountAsync(), Is.EqualTo(1));
                Assert.That(await page.GetByLabel("Labels", new PageGetByLabelOptions { Exact = true }).InputValueAsync(), Is.EqualTo("[valid, invalid label]"));
                Assert.That(await page.GetByLabel("Title", new PageGetByLabelOptions { Exact = true }).InputValueAsync(), Is.EqualTo("Should not persist"));
                Assert.That(await page.Locator("label.modified[for='Title']").CountAsync(), Is.EqualTo(1));
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Revert", Exact = true }).ClickAsync();
                Assert.That(await page.GetByLabel("Title", new PageGetByLabelOptions { Exact = true }).InputValueAsync(), Is.EqualTo("Updated title"));
                Assert.That(await page.GetByLabel("Labels", new PageGetByLabelOptions { Exact = true }).InputValueAsync(), Is.EqualTo(string.Empty));

                await page.GetByLabel("Title", new PageGetByLabelOptions { Exact = true }).FillAsync(string.Empty);
                await page.GetByLabel("Description", new PageGetByLabelOptions { Exact = true }).FillAsync("Should not persist description");
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
                Assert.That(await page.GetByRole(AriaRole.Alert).CountAsync(), Is.EqualTo(1));
                await page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Cancel", Exact = true }).ClickAsync();
                Assert.That(new Uri(page.Url).AbsolutePath, Is.EqualTo("/"));

                using IssueWorkspace verify = fixture.Open();
                IssueDocument loaded = await verify.GetIssueAsync(created.Key, CancellationToken.None);
                Assert.That(loaded.Title, Is.EqualTo("Updated title"));
                Assert.That(loaded.Description, Is.EqualTo("Updated description"));
                Assert.That(loaded.Fields["Labels"].ToString(), Does.Contain("feature").And.Not.Contain("valid"));
            }
            finally
            {
                await application.StopAsync();
            }
        }

        [Test]
        public async Task Pages_UseTheConfiguredProjectName()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            string path = Path.Combine(fixture.Directory, Paths.ConfigFileName);
            IssueConfiguration configuration = IssueConfiguration.Read(path);
            configuration.ProjectName = "Rocket Lab";
            configuration.Save(path);
            IssueDocument created;
            using (IssueWorkspace workspace = fixture.Open())
            {
                created = await workspace.CreateIssueAsync("Named project", "Description", CancellationToken.None);
            }

            WebServeRequest request = WebServerHostTests.request(fixture.Directory, WebServerHostTests.freePort(), null);
            await using WebApplication application = WebServerHost.Create(request);
            await application.StartAsync();
            try
            {
                using IPlaywright playwright = await Playwright.CreateAsync();
                await using IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
                IPage page = await browser.NewPageAsync();
                await page.GotoAsync(request.Url);
                Assert.That(await page.TitleAsync(), Is.EqualTo("Rocket Lab"));
                Assert.That(await page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Rocket Lab", Level = 1 }).CountAsync(), Is.EqualTo(1));
                await page.GotoAsync(request.Url + "/issue?key=" + Uri.EscapeDataString(created.Key));
                Assert.That(await page.TitleAsync(), Is.EqualTo("Rocket Lab"));
                Assert.That(await page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Named project", Level = 1 }).CountAsync(), Is.EqualTo(1));
            }
            finally
            {
                await application.StopAsync();
            }
        }

        [Test]
        public async Task MissingIssue_ShowsAnError()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            WebServeRequest request = WebServerHostTests.request(fixture.Directory, WebServerHostTests.freePort(), null);
            await using WebApplication application = WebServerHost.Create(request);
            await application.StartAsync();
            try
            {
                using IPlaywright playwright = await Playwright.CreateAsync();
                await using IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
                IPage page = await browser.NewPageAsync();
                await page.GotoAsync(request.Url + "/issue?key=" + Uri.EscapeDataString("missing/key"));

                Assert.That(await page.GetByRole(AriaRole.Alert).InnerTextAsync(), Does.Contain("was not found"));
            }

            finally
            {
                await application.StopAsync();
            }
        }

        [Test]
        public async Task IssuePage_UsesTheIssueTypeSchema()
        {
            using RepositoryFixture fixture = new RepositoryFixture();
            string path = Path.Combine(fixture.Directory, Paths.ConfigFileName);
            IssueConfiguration configuration = IssueConfiguration.Read(path);
            Dictionary<FieldKey, FieldInfo> bug = new Dictionary<FieldKey, FieldInfo>(configuration.GetSchema("Issue"))
            {
                ["Severity"] = new FieldInfo<GitIssue.Values.String, JsonValueField>(true),
            };
            configuration.Issues.Add(new IssueTypeConfiguration { Name = "BUG", Fields = bug });
            configuration.Save(path);

            IssueDocument created;
            using (IssueWorkspace workspace = fixture.Open())
            {
                created = await workspace.CreateIssueAsync("Bug", "Description", "BUG",
                    new Dictionary<string, string> { ["Severity"] = "high" }, CancellationToken.None);
            }

            WebServeRequest request = WebServerHostTests.request(fixture.Directory, WebServerHostTests.freePort(), null);
            await using WebApplication application = WebServerHost.Create(request);
            await application.StartAsync();
            try
            {
                using IPlaywright playwright = await Playwright.CreateAsync();
                await using IBrowser browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
                IPage page = await browser.NewPageAsync();
                await page.GotoAsync(request.Url + "/issue?key=" + Uri.EscapeDataString(created.Key));
                Assert.That(await page.GetByText("BUG (read-only)").CountAsync(), Is.EqualTo(1));
                Assert.That(await page.GetByLabel("Severity").InputValueAsync(), Is.EqualTo("high"));
                await page.GetByLabel("Severity").FillAsync("low");
                await page.Locator("label.modified[for='Severity']").WaitForAsync();
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
                Assert.That(await page.GetByRole(AriaRole.Status).InnerTextAsync(), Does.Contain("Changes saved"));
                await page.ReloadAsync();
                Assert.That(await page.GetByLabel("Severity").InputValueAsync(), Is.EqualTo("low"));
            }
            finally
            {
                await application.StopAsync();
            }
        }
    }
}
