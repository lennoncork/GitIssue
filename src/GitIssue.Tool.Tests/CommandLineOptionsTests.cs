using CommandLine;
using GitIssue.Tool.Commands.Create;
using GitIssue.Tool.Commands.Edit;
using GitIssue.Tool.Commands.Export;
using GitIssue.Tool.Commands.Show;
using NUnit.Framework;

namespace GitIssue.Tool.Tests
{
    [TestFixture]
    public class CommandLineOptionsTests
    {
        [Test]
        public void Create_BindsTitleAndDescription()
        {
            CreateOptions options = Parse<CreateOptions>("title", "description");

            Assert.That(options.Title, Is.EqualTo("title"));
            Assert.That(options.Description, Is.EqualTo("description"));
        }

        [Test]
        public void Edit_BindsKeyFieldAndValue()
        {
            EditOptions options = Parse<EditOptions>("issue-key", "Title", "updated");

            Assert.That(options.Key, Is.EqualTo("issue-key"));
            Assert.That(options.Field, Is.EqualTo("Title"));
            Assert.That(options.Update, Is.EqualTo("updated"));
        }

        [Test]
        public void Export_BindsFile()
        {
            ExportOptions options = Parse<ExportOptions>("issues.json");

            Assert.That(options.Export, Is.EqualTo("issues.json"));
        }

        [Test]
        public void Show_BindsSubcommandWithoutAKeyCollision()
        {
            ShowOptions options = Parse<ShowOptions>("all");

            Assert.That(options.Show, Is.EqualTo(ShowSubCommand.all));
        }

        private static T Parse<T>(params string[] arguments)
        {
            T? options = default;
            ParserResult<T> result = new Parser(settings => settings.HelpWriter = null)
                .ParseArguments<T>(arguments);
            result.WithParsed(value => options = value);

            Assert.That(result.Tag, Is.EqualTo(ParserResultType.Parsed));
            return options!;
        }
    }
}