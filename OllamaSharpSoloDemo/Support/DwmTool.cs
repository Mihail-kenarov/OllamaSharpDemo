using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using OllamaSharp.Models.Chat;

namespace OllamaSharpSoloDemo.Support
{
    public abstract class DwmTool : Tool
    {
        protected const string DefaultRootDirectory = @"C:\Users\kenar";

        public abstract string Name { get; }
        public abstract Task<string> ExecuteAsync(IDictionary<string, object> namedParameters);

        // Helper to resolve relative paths against the default root.
        protected string ResolvePath(string inputPath)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
                return DefaultRootDirectory;

            return Path.GetFullPath(
                Path.IsPathRooted(inputPath)
                    ? inputPath
                    : Path.Combine(DefaultRootDirectory, inputPath)
            );
        }
    }
}
