using GitIssue.Fields;

namespace GitIssue.Issues
{
    public class FileFieldKeyProvider : FieldKeyProvider
    {
        public override FieldKey FromString(string key)
        {
            return FieldKey.Create(key);
        }
    }
}
