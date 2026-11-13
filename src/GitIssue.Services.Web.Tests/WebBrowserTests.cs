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
                float? expectedGap = null;
                foreach (string field in new[] { "Title", "Description", "Labels", "Comments" })
                {
                    LocatorBoundingBoxResult? input = await page.GetByLabel(field, new PageGetByLabelOptions { Exact = true }).BoundingBoxAsync();
                    LocatorBoundingBoxResult? button = await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = $"Save {field}", Exact = true }).BoundingBoxAsync();
                    Assert.That(input, Is.Not.Null);
                    Assert.That(button, Is.Not.Null);
                    float gap = button!.Y - (input!.Y + input.Height);
                    Assert.That(gap, Is.GreaterThan(0), $"Missing spacing for {field}");
                    expectedGap = expectedGap ?? gap;
                    Assert.That(gap, Is.EqualTo(expectedGap.Value).Within(0.1), $"Inconsistent spacing for {field}");
                }

                await page.GetByLabel("Title", new PageGetByLabelOptions { Exact = true }).FillAsync("Updated title");
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save Title" }).ClickAsync();
                Assert.That(await page.GetByRole(AriaRole.Status).InnerTextAsync(), Does.Contain("Title saved"));
                await page.GetByLabel("Description", new PageGetByLabelOptions { Exact = true }).FillAsync("Updated description");
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save Description" }).ClickAsync();
                await page.GetByLabel("Labels", new PageGetByLabelOptions { Exact = true }).FillAsync("+feature");
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save Labels" }).ClickAsync();
                await page.ReloadAsync();
                Assert.That(await page.GetByLabel("Title", new PageGetByLabelOptions { Exact = true }).InputValueAsync(), Is.EqualTo("Updated title"));
                Assert.That(await page.GetByLabel("Description", new PageGetByLabelOptions { Exact = true }).InputValueAsync(), Is.EqualTo("Updated description"));
                Assert.That(await page.GetByText("Current value:", new PageGetByTextOptions { Exact = false }).First.InnerTextAsync(), Does.Contain("feature"));
                await page.GetByLabel("Labels", new PageGetByLabelOptions { Exact = true }).FillAsync("[valid, invalid label]");
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save Labels" }).ClickAsync();
                Assert.That(await page.GetByRole(AriaRole.Alert).CountAsync(), Is.EqualTo(1));
                Assert.That(await page.GetByLabel("Labels", new PageGetByLabelOptions { Exact = true }).InputValueAsync(), Is.EqualTo("[valid, invalid label]"));
                await page.GetByLabel("Title", new PageGetByLabelOptions { Exact = true }).FillAsync(string.Empty);
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save Title" }).ClickAsync();
                Assert.That(await page.GetByRole(AriaRole.Alert).CountAsync(), Is.EqualTo(1));

                using IssueWorkspace verify = fixture.Open();
                IssueDocument loaded = await verify.GetIssueAsync(created.Key, CancellationToken.None);
                Assert.That(loaded.Title, Is.EqualTo("Updated title"));
                Assert.That(loaded.Fields["Labels"].ToString(), Does.Contain("feature").And.Not.Contain("valid"));
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
            string path = Path.Combine(fixture.Directory, ".issues", "config.json");
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
                await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save Severity" }).ClickAsync();
                Assert.That(await page.GetByRole(AriaRole.Status).InnerTextAsync(), Does.Contain("Severity saved"));
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
