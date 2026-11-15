using System.Linq;
using CommandLine;
using GitIssue.Tool.Commands.Create;
using GitIssue.Tool.Commands.Edit;
using GitIssue.Tool.Commands.Export;
using GitIssue.Tool.Commands.Mcp;
using GitIssue.Tool.Commands.Show;
using GitIssue.Tool.Commands.Web;
using NUnit.Framework;

namespace GitIssue.Tool.Tests
{
    [TestFixture]
    public class CommandLineOptionsTests
    {
        private static T parse<T>(params string[] arguments)
        {
            T? options = default;
            ParserResult<T> result = new Parser(settings =>
            {
                settings.HelpWriter = null;
                settings.AllowMultiInstance = true;
            }).ParseArguments<T>(arguments);
            result.WithParsed(value => options = value);

            Assert.That(result.Tag, Is.EqualTo(ParserResultType.Parsed));
            return options!;
        }

        [Test]
        public void Create_BindsTitleAndDescription()
        {
            CreateOptions options = CommandLineOptionsTests.parse<CreateOptions>("title", "description");

            Assert.That(options.Title, Is.EqualTo("title"));
            Assert.That(options.Description, Is.EqualTo("description"));
        }

        [Test]
        public void Edit_BindsKeyFieldAndValue()
        {
            EditOptions options = CommandLineOptionsTests.parse<EditOptions>("issue-key", "Title", "updated");

            Assert.That(options.Key, Is.EqualTo("issue-key"));
            Assert.That(options.Field, Is.EqualTo("Title"));
            Assert.That(options.Update, Is.EqualTo("updated"));
        }

        [Test]
        public void Export_BindsFile()
        {
            ExportOptions options = CommandLineOptionsTests.parse<ExportOptions>("issues.json");

            Assert.That(options.Export, Is.EqualTo("issues.json"));
        }

        [Test]
        public void Mcp_BindsInstallOptions()
        {
            McpOptions options = CommandLineOptionsTests.parse<McpOptions>(
                "install",
                "--scope",
                "user",
                "--insiders",
                "--command",
                "dotnet",
                "--server-arg",
                "mcp",
                "--server-arg",
                "serve");

            Assert.Multiple(() =>
            {
                Assert.That(options.Action, Is.EqualTo("install"));
                Assert.That(options.Scope, Is.EqualTo("user"));
                Assert.That(options.Insiders, Is.True);
                Assert.That(options.Command, Is.EqualTo("dotnet"));
                Assert.That(options.ServerArguments.ToArray(), Is.EqualTo(new[] { "mcp", "serve" }));
            });
        }

        [Test]
        public void Web_BindsServeOptions()
        {
            WebOptions options = CommandLineOptionsTests.parse<WebOptions>("serve", "--host", "127.0.0.1", "--port", "5090", "--open", "--token", "secret");

            Assert.Multiple(() =>
            {
                Assert.That(options.Action, Is.EqualTo("serve"));
                Assert.That(options.Host, Is.EqualTo("127.0.0.1"));
                Assert.That(options.Port, Is.EqualTo(5090));
                Assert.That(options.Open, Is.True);
                Assert.That(options.Token, Is.EqualTo("secret"));
            });
        }

        [Test]
        public void Show_BindsSubcommandWithoutAKeyCollision()
        {
            ShowOptions options = CommandLineOptionsTests.parse<ShowOptions>("all");

            Assert.That(options.Show, Is.EqualTo(ShowSubCommand.all));
        }
    }
}
