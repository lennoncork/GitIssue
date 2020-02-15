using System;
using System.Linq;
using System.Threading.Tasks;
using GitIssue.Fields;
using GitIssue.Fields.Array;
using GitIssue.Formatters;
using Serilog;

namespace GitIssue.Util.Commands.Add
{
    /// <summary>
    ///     Edit command
    /// </summary>
    public class AddCommand : Command<AddOptions>
    {
        private static ILogger Logger => Program.Logger;

        private static IIssueManager Initialize(Options options)
        {
            return Program.Initialize(options);
        }

        /// <inheritdoc />
        public override async Task Exec(AddOptions options)
        {
            var formatter = new DetailedFormatter();

            await using var issues = Initialize(options);

            var issue = await issues
                .FindAsync(i => i.Key.ToString() == options.Key)
                .FirstOrDefaultAsync();

            if (issue == null)
            {
                Logger.Error($"Issue \"{options.Key}\" not found");
                return;
            }

            if (string.IsNullOrEmpty(options.Field))
            {
                Logger.Error($"Field must be supplied to remove a value on \"{issue.Key}\"");
                return;
            }

            var key = FieldKey.Create(options.Field);
            if (!issue.ContainsKey(key))
            {
                Logger.Error($"Field \"{key}\" does not exist on issue \"{issue.Key}\"");
                return;
            }

            var field = issue[key];
            if (field is IArrayField arrayField)
            {
                if (arrayField.TryParse(options.Add, out var value))
                {
                    if (!arrayField.Contains(value))
                    {
                        arrayField.Add(value);
                        await issue.SaveAsync();
                        Console.WriteLine(formatter.Format(arrayField));
                    }
                    else
                    {
                        Logger.Error($"Item \"{options.Add}\" already exists on field {options.Field}");
                    }
                }
                else
                {
                    Logger.Error($"Item \"{options.Add}\" is not a valid value for field {options.Field}");
                }
            }
            else
            {
                Logger.Error($"Field \"{key}\" is not an array type, cannot remove value");
            }
        }
    }
}