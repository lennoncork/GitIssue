using CommandLine;

namespace GitIssue.Util
{
#pragma warning disable 1591
    [Verb(nameof(CommandType.Track), HelpText = "Tracks an existing issue")]
    public class TrackOptions : KeyOptions
    {
    }
#pragma warning restore 1591
}