using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace GitIssue.Tests
{
    public static class Helpers
    {
        private static Random random = new Random();

        /// <summary>
        /// Gets a random unique string, max length 64
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandomString(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Gets the test directory
        /// </summary>
        /// <returns></returns>
        public static string GetTestDirectory() => Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");

        /// <summary>
        /// Gets the path of a new temp file, using the default temp directory
        /// </summary>
        /// <returns></returns>
        public static string GetTempFile() => GetTempFile(GetTestDirectory());

        /// <summary>
        /// Gets the path of a new temp file
        /// </summary>
        /// <param name="path">the parent path for the file</param>
        /// <returns></returns>
        public static string GetTempFile(string path) => Path.Combine(path, $"{GetRandomString()}.txt");

        /// <summary>
        /// Creates a new Temporary File, using the default temp directory
        /// </summary>
        /// <returns></returns>
        public static string CreateTempFile() => CreateTempFile(GetTestDirectory());

        /// <summary>
        /// Creates a new Temporary File
        /// </summary>
        /// <param name="path">the parent path for the directory</param>
        /// <returns></returns>
        public static string CreateTempFile(string path)
        {
            string file = GetTempFile(path);
            File.Create(file).Dispose();
            return file;
        }

        /// <summary>
        /// Gets the path of a new temp directory, using the default temp directory
        /// </summary>
        /// <returns></returns>
        public static string GetTempDirectory() => GetTempDirectory(GetTestDirectory());

        /// <summary>
        /// Gets the path of a new temp directory
        /// </summary>
        /// <param name="path">the parent path for the directory</param>
        /// <returns></returns>
        public static string GetTempDirectory(string path) => Path.Combine(path, GetRandomString());

        /// <summary>
        /// Creates a new temporary directory, using the default temp directory
        /// </summary>
        /// <returns></returns>
        public static string CreateTempDirectory() => CreateTempDirectory(GetTestDirectory());

        /// <summary>
        /// Creates a new temporary directory
        /// </summary>
        /// <param name="path">the parent path for the directory</param>
        /// <returns></returns>
        public static string CreateTempDirectory(string path)
        {
            string directory = GetTempDirectory(path);
            Directory.CreateDirectory(directory);
            return directory;
        }

        /// <summary>
        /// Temporarily sets the current directory
        /// </summary>
        public class EnvironmentCurrentDirectory : IDisposable
        {
            private readonly string environment;

            /// <summary>
            /// Creates a new instance of the <see cref="EnvironmentCurrentDirectory"/> class
            /// </summary>
            /// <param name="directory"></param>
            public EnvironmentCurrentDirectory(string directory)
            {
                if (Directory.Exists(directory))
                {
                    this.environment = Environment.CurrentDirectory;
                    Environment.CurrentDirectory = directory;
                }
            }

            /// <inheritdoc cref="IDisposable"/>
            public void Dispose()
            {
                if(this.environment != null)
                    Environment.CurrentDirectory = this.environment;
            }
        }
    }
}
