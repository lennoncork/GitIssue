using System.ComponentModel;
using System.Text.RegularExpressions;
using GitIssue.Converters;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     Version value type
    /// </summary>
    [TypeConverter(typeof(VersionTypeConverter))]
    [TypeAlias(nameof(Version))]
    public struct Version : IJsonValue
    {
        private static readonly string regex = @"^[\s]*(\d*).(\d*).(\d*)(\-[\w.]*)?(\+[\w.]*)?[\s]*$";

        /// <summary>
        ///     Tries to parse the semantic version
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Version Parse(string str)
        {
            return new Version(str);
        }

        /// <summary>
        ///     Tries to parse the semantic version
        /// </summary>
        /// <param name="str"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool TryParse(string str, out Version version)
        {
            version = new Version(str);
            return version.IsValid;
        }

        internal Version(string version)
        {
            if (IsMatch(version, regex, out var match) &&
                match.Groups.Count == 6)
            {
                MajorVersion = int.Parse(match.Groups[1].ToString());
                MinorVersion = int.Parse(match.Groups[2].ToString());
                PatchVersion = int.Parse(match.Groups[3].ToString());
                PreRelease = match.Groups[4].ToString().TrimStart('-');
                BuildMetadata = match.Groups[5].ToString().TrimStart('+');
                IsValid = true;
            }
            else
            {
                MajorVersion = 1;
                MinorVersion = 0;
                PatchVersion = 0;
                PreRelease = null;
                BuildMetadata = null;
                IsValid = false;
            }
        }

        /// <summary>
        ///     Gets the value determining if the version is valid
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        ///     The Major Version
        /// </summary>
        public int MajorVersion { get; }

        /// <summary>
        ///     The Minor Version
        /// </summary>
        public int MinorVersion { get; }

        /// <summary>
        ///     The Patch Version
        /// </summary>
        public int PatchVersion { get; }

        /// <summary>
        ///     The Pre-Release String
        /// </summary>
        public string PreRelease { get; }

        /// <summary>
        ///     The Build Metadata
        /// </summary>
        public string BuildMetadata { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var version = $"{MajorVersion}.{MinorVersion}.{PatchVersion}";
            if (string.IsNullOrEmpty(PreRelease) == false)
                version += $"-{PreRelease}";
            if (string.IsNullOrEmpty(BuildMetadata) == false)
                version += $"+{BuildMetadata}";
            return version;
        }

        /// <summary>
        ///     Matches a string and patter, returning the Match
        /// </summary>
        /// <param name="input">the input string</param>
        /// <param name="pattern">the regex pattern</param>
        /// <param name="match">the match output</param>
        /// <returns></returns>
        public static bool IsMatch(string input, string pattern, out Match match)
        {
            if (!string.IsNullOrEmpty(input))
            {
                match = Regex.Match(input, pattern);
                return match.Success;
            }

            match = null;
            return false;
        }

        /// <inheritdoc />
        public JToken ToJson()
        {
            return new JValue(ToString());
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Version version)
                return MajorVersion == version.MajorVersion &&
                       MinorVersion == version.MinorVersion &&
                       PatchVersion == version.PatchVersion &&
                       PreRelease == version.PreRelease &&
                       BuildMetadata == version.BuildMetadata;
            return false;
        }
    }
}