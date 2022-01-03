using System;
using System.Collections.Generic;
using System.Globalization;
using GitIssue.Fields;
using GitIssue.Issues;
using Moq;

namespace GitIssue.Tests
{
    public static class Moqs
    {
        private delegate void MockOutDelegate<T1, T2>(T1 s, out T2 value);

        public static void AddField(this Dictionary<FieldKey, IField> dict, string key, string value)
        {
            var fieldKey = FieldKey.Create(key);
            var field = CreateField(key, value);
            dict.Add(fieldKey, field);
        }

        public static IField CreateField(FieldKey key, string value)
        {
            var moq = new Mock<IField>(MockBehavior.Strict);
            moq.Setup(f => f.Key).Returns(key);
            moq.Setup(f => f.ToString()).Returns(value);
            moq.Setup(f => f.GetHashCode()).Returns(value.GetHashCode());
            return moq.Object;
        }

        public static IssueKey CreateIssueKey() => IssueKey.Create(Helpers.GetRandomString());

        public static IReadOnlyIssue CreateIssue(string title) => CreateIssue(title, string.Empty);

        public static IReadOnlyIssue CreateIssue(string title, string description) =>
            CreateIssue(CreateIssueKey(), title, description);

        public static IReadOnlyIssue CreateIssue(IssueKey key, string title, string description) =>
            CreateIssue(key, title, description, DateTime.Now, DateTime.Now);

        public static IReadOnlyIssue CreateIssue(IssueKey key, string title, string description, DateTime created, DateTime updated)
        {
            var fields = new Dictionary<FieldKey, IField>();
            fields.AddField(nameof(IIssue.Key), key);
            fields.AddField(nameof(IIssue.Title), title);
            fields.AddField(nameof(IIssue.Description), description);
            fields.AddField(nameof(IIssue.Created), created.ToString(CultureInfo.InvariantCulture));
            fields.AddField(nameof(IIssue.Updated), updated.ToString(CultureInfo.InvariantCulture));
            return CreateIssue(fields);
        }

        public static IReadOnlyIssue CreateIssue(IReadOnlyDictionary<FieldKey, IField> fields)
        {
            var moq = new Mock<IReadOnlyIssue>(MockBehavior.Strict);

            FieldKey? tryGetFieldKey = null;
            IField? tryGetResult = null;

            moq.Setup(i => i[It.IsAny<FieldKey>()]).Returns((FieldKey key) => fields[key]);

            moq.Setup(i => i.TryGetValue(It.IsAny<FieldKey>(), out tryGetResult))
                .Callback(new MockOutDelegate<FieldKey, IField>((FieldKey key, out IField output) =>
                {
                    tryGetFieldKey = key;
                    fields.TryGetValue(key, out tryGetResult);
                    output = tryGetResult ?? new EmptyField(key);
                }))
                .Returns(() =>
                {
                    if (tryGetFieldKey == null)
                    {
                        return false;
                    }
                    return fields.ContainsKey(tryGetFieldKey.Value);
                });

            moq.Setup(i => i.GetEnumerator()).Returns(fields.GetEnumerator());

            foreach (var field in fields.Values)
            {
                if (field.Key == nameof(IIssue.Key))
                    moq.Setup(i => i.Key).Returns(IssueKey.Create(field.ToString()));

                if (field.Key == nameof(IIssue.Title))
                    moq.Setup(i => i.Title).Returns(field.ToString() ?? string.Empty);

                if (field.Key == nameof(IIssue.Description))
                    moq.Setup(i => i.Description).Returns(field.ToString() ?? string.Empty);

                if (field.Key == nameof(IIssue.Created))
                    moq.Setup(i => i.Created).Returns(DateTime.Parse(field.ToString() ?? string.Empty));

                if (field.Key == nameof(IIssue.Updated))
                    moq.Setup(i => i.Updated).Returns(DateTime.Parse(field.ToString() ?? string.Empty));
            }

            return moq.Object;
        }

        public static IReadOnlyIssue Issue
        {
            get
            {
                var moq = new Mock<IReadOnlyIssue>(MockBehavior.Strict);
                return moq.Object;
            }
        }

        public static IIssueManager IssueManager
        {
            get
            {
                var moq = new Mock<IIssueManager>(MockBehavior.Strict);
                return moq.Object;
            }
        }
    }
}
