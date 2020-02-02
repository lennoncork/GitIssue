using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace GitIssue.Tests
{
    public static class Helpers
    {
        private static readonly Random random = new Random();

        /// <summary>
        ///     Gets a random unique string, max length 64
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
        ///     Gets the test directory
        /// </summary>
        /// <returns></returns>
        public static string GetTestDirectory()
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
        }

        /// <summary>
        ///     Gets the path of a new temp file, using the default temp directory
        /// </summary>
        /// <returns></returns>
        public static string GetTempFile()
        {
            return GetTempFile(GetTestDirectory());
        }

        /// <summary>
        ///     Gets the path of a new temp file
        /// </summary>
        /// <param name="path">the parent path for the file</param>
        /// <returns></returns>
        public static string GetTempFile(string path)
        {
            return Path.Combine(path, $"{GetRandomString()}.txt");
        }

        /// <summary>
        ///     Creates a new Temporary File, using the default temp directory
        /// </summary>
        /// <returns></returns>
        public static string CreateTempFile()
        {
            return CreateTempFile(GetTestDirectory());
        }

        /// <summary>
        ///     Creates a new Temporary File
        /// </summary>
        /// <param name="path">the parent path for the directory</param>
        /// <returns></returns>
        public static string CreateTempFile(string path)
        {
            var file = GetTempFile(path);
            File.Create(file).Dispose();
            return file;
        }

        /// <summary>
        ///     Gets the path of a new temp directory, using the default temp directory
        /// </summary>
        /// <returns></returns>
        public static string GetTempDirectory()
        {
            return GetTempDirectory(GetTestDirectory());
        }

        /// <summary>
        ///     Gets the path of a new temp directory
        /// </summary>
        /// <param name="path">the parent path for the directory</param>
        /// <returns></returns>
        public static string GetTempDirectory(string path)
        {
            return Path.Combine(path, GetRandomString());
        }

        /// <summary>
        ///     Creates a new temporary directory, using the default temp directory
        /// </summary>
        /// <returns></returns>
        public static string CreateTempDirectory()
        {
            return CreateTempDirectory(GetTestDirectory());
        }

        /// <summary>
        ///     Creates a new temporary directory
        /// </summary>
        /// <param name="path">the parent path for the directory</param>
        /// <returns></returns>
        public static string CreateTempDirectory(string path)
        {
            var directory = GetTempDirectory(path);
            Directory.CreateDirectory(directory);
            return directory;
        }

        /// <summary>
        ///     Temporarily sets the current directory
        /// </summary>
        public class EnvironmentCurrentDirectory : IDisposable
        {
            private readonly string environment;

            /// <summary>
            ///     Creates a new instance of the <see cref="EnvironmentCurrentDirectory" /> class
            /// </summary>
            /// <param name="directory"></param>
            public EnvironmentCurrentDirectory(string directory)
            {
                if (Directory.Exists(directory))
                {
                    environment = Environment.CurrentDirectory;
                    Environment.CurrentDirectory = directory;
                }
            }

            /// <inheritdoc cref="IDisposable" />
            public void Dispose()
            {
                if (environment != null)
                    Environment.CurrentDirectory = environment;
            }
        }
    }
}