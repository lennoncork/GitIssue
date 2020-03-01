using System.ComponentModel;

namespace GitIssue
{
#pragma warning disable 1591
    public enum ChangeType
    {
        [Description("Created new issue")] Create,

        [Description("Deleted existing issue")] Delete
    }
#pragma warning restore 1591
}