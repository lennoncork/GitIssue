using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace GitIssue.Tests
{
    public static class Helpers
    {
        private static readonly Random random = new Random();

        public static string TestData = "TestData";

        /// <summary>
        ///     Creates a new temporary directory, using the default temp directory
        /// </summary>
        /// <returns></returns>
        public static string CreateTempDirectory()
        {
            return Helpers.CreateTempDirectory(Helpers.GetTestDirectory());
        }

        /// <summary>
        ///     Creates a new temporary directory
        /// </summary>
        /// <param name="path">the parent path for the directory</param>
        /// <returns></returns>
        public static string CreateTempDirectory(string path)
        {
            string directory = Helpers.GetTempDirectory(path);
            Directory.CreateDirectory(directory);
            return directory;
        }

        /// <summary>
        ///     Creates a new Temporary File, using the default temp directory
        /// </summary>
        /// <returns></returns>
        public static string CreateTempFile()
        {
            return Helpers.CreateTempFile(Helpers.GetTestDirectory());
        }

        /// <summary>
        ///     Creates a new Temporary File
        /// </summary>
        /// <param name="path">the parent path for the directory</param>
        /// <returns></returns>
        public static string CreateTempFile(string path)
        {
            string file = Helpers.GetTempFile(path);
            File.Create(file).Dispose();
            return file;
        }

        /// <summary>
        ///     Gets a random unique string, max length 64
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandomString(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Helpers.random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        ///     Gets the path of a new temp directory, using the default temp directory
        /// </summary>
        /// <returns></returns>
        public static string GetTempDirectory()
        {
            return Helpers.GetTempDirectory(Helpers.GetTestDirectory());
        }

        /// <summary>
        ///     Gets the path of a new temp directory
        /// </summary>
        /// <param name="path">the parent path for the directory</param>
        /// <returns></returns>
        public static string GetTempDirectory(string path)
        {
            return Path.Combine(path, Helpers.GetRandomString());
        }

        /// <summary>
        ///     Gets the path of a new temp file, using the default temp directory
        /// </summary>
        /// <returns></returns>
        public static string GetTempFile()
        {
            return Helpers.GetTempFile(Helpers.GetTestDirectory());
        }

        /// <summary>
        ///     Gets the path of a new temp file
        /// </summary>
        /// <param name="path">the parent path for the file</param>
        /// <returns></returns>
        public static string GetTempFile(string path)
        {
            return Path.Combine(path, $"{Helpers.GetRandomString()}.txt");
        }

        /// <summary>
        ///     Gets the test directory
        /// </summary>
        /// <returns></returns>
        public static string GetTestDirectory()
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, Helpers.TestData);
        }

        /// <summary>
        ///     Temporarily sets the current directory
        /// </summary>
        public class EnvironmentCurrentDirectory : IDisposable
        {
            private readonly string? environment;

            /// <summary>
            ///     Creates a new instance of the <see cref="EnvironmentCurrentDirectory" /> class
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

            /// <inheritdoc cref="IDisposable" />
            public void Dispose()
            {
                if (this.environment != null)
                {
                    Environment.CurrentDirectory = this.environment;
                }
            }
        }
    }
}