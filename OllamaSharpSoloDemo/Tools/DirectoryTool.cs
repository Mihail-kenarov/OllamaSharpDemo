using OllamaSharp.Models.Chat;
using OllamaSharpSoloDemo.Support;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OllamaSharpSoloDemo.Tools
{
    public sealed class DirectoryTool : DwmTool
    {
        public override string Name => Function.Name;

        public DirectoryTool()
        {
            Function = new Function
            {
                Description = "Lists the names of files and directories in a specified directory (first level only) starting from C:\\Users\\kenar. Skips hidden files/directories.",
                Name = "list_directory",
                Parameters = new Parameters
                {
                    Properties = new Dictionary<string, Property>
                    {
                        ["path"] = new() { Type = "string", Description = "Path to the directory to list. Defaults to C:\\Users\\kenar." }
                    },
                    Required = System.Array.Empty<string>()
                }
            };
        }

        public override async Task<string> ExecuteAsync(IDictionary<string, object> namedParameters)
        {
            // Use the common ResolvePath helper.
            var path = ResolvePath(namedParameters?["path"]?.ToString());
            return await ListDirectoryAsync(path);
        }

        private async Task<string> ListDirectoryAsync(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return JsonSerializer.Serialize(new { error = $"Directory does not exist: {directoryPath}" });

            var dirInfo = new DirectoryInfo(directoryPath);
            var result = new
            {
                Directories = dirInfo.GetDirectories()
                                     .Where(d => (d.Attributes & FileAttributes.Hidden) == 0)
                                     .Select(d => d.Name)
                                     .ToList(),
                Files = dirInfo.GetFiles()
                               .Where(f => (f.Attributes & FileAttributes.Hidden) == 0)
                               .Select(f => f.Name)
                               .ToList()
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
