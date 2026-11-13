using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GitIssue.Issues;
using LibGit2Sharp;
using NUnit.Framework;
using Signature = GitIssue.Values.Signature;

namespace GitIssue.Tests.IssueManagerTests
{
    [TestFixture]
    public partial class IssueManagerTests
    {
        [TestFixture]
        public class Create : IssueManagerTests
        {
            [TestCase("New Issue", "This Is A New Issue")]
            public async Task CreatesNewIssue(string title, string description)
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(title, description)
                    .WithSafeResultAsync();
                Assert.IsTrue(create.IsSuccess);
                Assert.That(create.Result.Title, Is.EqualTo(title));
                Assert.That(create.Result.Description, Is.EqualTo(description));
            }

            [Test]
            public void CreatesNewIssueUsingSyncOverload()
            {
                this.Initialize(this.TestDirectory);
                string title = "New Sync Issue";
                IIssue issue = this.Sut.Create(title);
                Assert.That(issue.Title, Is.EqualTo(title));
                Assert.That(issue.Key, Is.Not.EqualTo(IssueKey.None));
            }

            [Test]
            public void CreatesNewIssueUsingSyncOverloadWithDescription()
            {
                this.Initialize(this.TestDirectory);
                string title = "New Sync Issue With Description";
                string description = "This Is A New Sync Issue";
                IIssue issue = this.Sut.Create(title, description);
                Assert.That(issue.Title, Is.EqualTo(title));
                Assert.That(issue.Description, Is.EqualTo(description));
            }

            [Test]
            public async Task GeneratesUniqueId()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create1 = await this.Sut
                    .CreateAsync(nameof(Create.CreatesNewIssue), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create1.IsSuccess);
                SafeResult<IIssue> create2 = await this.Sut
                    .CreateAsync(nameof(Create.CreatesNewIssue), string.Empty)
                    .WithSafeResultAsync();
                Assert.IsTrue(create2.IsSuccess);
                Assert.That(create1.Result.Key, Is.Not.EqualTo(create2.Result.Key));
            }

            [Test]
            public async Task SetsAuthorFromRepositoryConfig()
            {
                this.Initialize(this.TestDirectory);
                this.Sut.Repository.Config.Set("user.name", "Tester");
                this.Sut.Repository.Config.Set("user.email", "tester@example.com");

                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Create.SetsAuthorFromRepositoryConfig), string.Empty)
                    .WithSafeResultAsync();
                Assert.That(create.Result.Author, Is.EqualTo(Signature.Parse("Tester <tester@example.com>")));
                Assert.That((DateTime)create.Result.Created, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(2)));
            }

            [Test]
            public async Task SetsCreatedDate()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Create.SetsCreatedDate), string.Empty)
                    .WithSafeResultAsync();
                Assert.That((DateTime)create.Result.Created, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(2)));
            }

            [Test]
            public async Task SetsUpdatedDate()
            {
                this.Initialize(this.TestDirectory);
                SafeResult<IIssue> create = await this.Sut
                    .CreateAsync(nameof(Create.SetsUpdatedDate), string.Empty)
                    .WithSafeResultAsync();
                Assert.That((DateTime)create.Result.Updated, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(2)));
            }

            [Test]
            public async Task CreatesNamedKeyWhenNameProviderIsConfigured()
            {
                Repository.Init(this.TestDirectory);
                using Repository repository = new Repository(this.TestDirectory);
                repository.Config.Set("user.name", "Tester");
                repository.Config.Set("user.email", "tester@example.com");

                IssueConfiguration configuration = new IssueConfiguration
                {
                    KeyProvider = GitIssue.Values.TypeValue.Create<NameIssueKeyProvider>(),
                };
                this.Sut = IssueManager.Init(configuration, this.TestDirectory);

                IIssue issue = await this.Sut.CreateAsync("Add login page", "Feature work");

                Assert.That(issue.Key.ToString(), Does.Match(new Regex(@"^[A-F0-9]{8} - Add login page$")));
                Assert.That(issue.Title, Is.EqualTo("Add login page"));
                Assert.That(this.Sut.KeyProvider, Is.InstanceOf<NameIssueKeyProvider>());
                Assert.That(this.Sut.KeyProvider.TryGetKey(issue.Key.ToString()[..8], out IssueKey found), Is.True);
                Assert.That(found, Is.EqualTo(issue.Key));
            }

            [Test]
            public async Task CreatesKeyFromConfiguredTemplate()
            {
                Repository.Init(this.TestDirectory);
                using Repository repository = new Repository(this.TestDirectory);
                repository.Config.Set("user.name", "Tester");
                repository.Config.Set("user.email", "tester@example.com");

                IssueConfiguration configuration = new IssueConfiguration
                {
                    KeyProvider = GitIssue.Values.TypeValue.Create<TemplateIssueKeyProvider>(),
                    KeyTemplate = "{type}-{uid}-{slug}",
                };
                configuration.Issues[0].Name = "FEAT";
                configuration.IssueType = "FEAT";
                this.Sut = IssueManager.Init(configuration, this.TestDirectory);

                IIssue issue = await this.Sut.CreateAsync("Add login page", "Feature work");

                Assert.That(issue.Key.ToString(), Does.Match(new Regex(@"^FEAT-[A-F0-9]{8}-add-login-page$")));
            }
        }
    }
}
