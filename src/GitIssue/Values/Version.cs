using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using GitIssue.Issues.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    ///     Version value type
    /// </summary>
    [TypeConverter(typeof(VersionTypeConverter))]
    [TypeAlias(nameof(Version))]
    public struct Version : IJsonValue, IEquatable<Version>
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
            if (Version.IsMatch(version, Version.regex, out Match match) &&
                (match.Groups.Count == 6))
            {
                this.MajorVersion = int.Parse(match.Groups[1].ToString());
                this.MinorVersion = int.Parse(match.Groups[2].ToString());
                this.PatchVersion = int.Parse(match.Groups[3].ToString());
                this.PreRelease = match.Groups[4].ToString().TrimStart('-');
                this.BuildMetadata = match.Groups[5].ToString().TrimStart('+');
                this.IsValid = true;
            }
            else
            {
                this.MajorVersion = 1;
                this.MinorVersion = 0;
                this.PatchVersion = 0;
                this.PreRelease = null;
                this.BuildMetadata = null;
                this.IsValid = false;
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
        public string? PreRelease { get; }

        /// <summary>
        ///     The Build Metadata
        /// </summary>
        public string? BuildMetadata { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            string version = $"{this.MajorVersion}.{this.MinorVersion}.{this.PatchVersion}";
            if (!string.IsNullOrEmpty(this.PreRelease))
            {
                version += $"-{this.PreRelease}";
            }

            if (!string.IsNullOrEmpty(this.BuildMetadata))
            {
                version += $"+{this.BuildMetadata}";
            }

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

            match = null!;
            return false;
        }

        /// <inheritdoc />
        public JToken ToJson()
        {
            return new JValue(this.ToString());
        }

        /// <inheritdoc />
        public bool Equals(Version other)
        {
            return
                (this.MajorVersion == other.MajorVersion) &&
                (this.MinorVersion == other.MinorVersion) &&
                (this.PatchVersion == other.PatchVersion) &&
                (this.PreRelease == other.PreRelease) &&
                (this.BuildMetadata == other.BuildMetadata);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Version version)
            {
                return this.Equals(version);
            }

            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}