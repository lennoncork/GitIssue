using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using GitIssue.Converters;
using GitIssue.Json;
using Newtonsoft.Json.Linq;

namespace GitIssue.Values
{
    /// <summary>
    /// Version value type
    /// </summary>
    [TypeConverter(typeof(VersionTypeConverter))]
    public class Version : IJsonValue
    {
        private static string regex = @"^[\s]*(\d*).(\d*).(\d*)(\-[\w.]*)?(\+[\w.]*)?[\s]*$";

        /// <summary>
        /// Tries to pars the semantic version
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Version Parse(string str) => new Version(str);

        /// <summary>
        /// Tries to pars the semantic version
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
            if (string.IsNullOrEmpty(version))
            {
                this.IsValid = false;
                return;
            }

            if (IsMatch(version, regex, out Match match))
            {
                if (match.Groups.Count == 6)
                {
                    this.MajorVersion = int.Parse(match.Groups[1].ToString());
                    this.MinorVersion = int.Parse(match.Groups[2].ToString());
                    this.PatchVersion = int.Parse(match.Groups[3].ToString());
                    this.PreRelease = match.Groups[4].ToString().TrimStart('-');
                    this.BuildMetadata = match.Groups[5].ToString().TrimStart('+');
                    this.IsValid = true;
                }
            }
        }

        /// <summary>
        /// Gets the value determining if the version is valid
        /// </summary>
        public bool IsValid { get; protected set; } = false;

        /// <summary>
        /// The Major Version
        /// </summary>
        public int MajorVersion { get; protected set; } = 0;

        /// <summary>
        /// The Minor Version
        /// </summary>
        public int MinorVersion { get; protected set; } = 1;

        /// <summary>
        /// The Patch Version
        /// </summary>
        public int PatchVersion { get; protected set; } = 0;

        /// <summary>
        /// The Pre-Release String
        /// </summary>
        public string PreRelease { get; protected set; } = null;

        /// <summary>
        /// The Build Metadata
        /// </summary>
        public string BuildMetadata { get; protected set; } = null;

        /// <inheritdoc/>
        public override string ToString()
        {
            string version = $"{this.MajorVersion}.{this.MinorVersion}.{this.PatchVersion}";
            if (string.IsNullOrEmpty(this.PreRelease) == false)
                version += $"-{this.PreRelease}";
            if (string.IsNullOrEmpty(this.BuildMetadata) == false)
                version += $"+{this.BuildMetadata}";
            return version;
        }

        /// <summary>
        /// Matches a string and patter, returning the Match
        /// </summary>
        /// <param name="input">the input string</param>
        /// <param name="pattern">the regex pattern</param>
        /// <param name="match">the match output</param>
        /// <returns></returns>
        public static bool IsMatch(string input, string pattern, out Match match)
        {
            match = Regex.Match(input, pattern);
            return match.Success;
        }

        /// <inheritdoc />
        public JToken ToJson() => new JValue(this.ToString());

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (obj is Version version)
            {
                return (this.MajorVersion == version.MajorVersion) &&
                       (this.MinorVersion == version.MinorVersion) &&
                       (this.PatchVersion == version.PatchVersion) &&
                       (this.PreRelease == version.PreRelease) &&
                       (this.BuildMetadata == version.BuildMetadata);
            }
            return false;
        }
    }
}
