using OllamaSharp.Models.Chat;
using OllamaSharpSoloDemo.Support;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System;

namespace OllamaSharpSoloDemo.Tools
{
    public sealed class ReadFileTool : DwmTool
    {
        public override string Name => Function.Name;

        public ReadFileTool()
        {
            Function = new Function
            {
                Description = "Reads the content of a specified file. If JSON, returns formatted JSON; otherwise raw text. Accepts relative paths (from C:\\Users\\kenar) and searches parent directories if not found.",
                Name = "read_file",
                Parameters = new Parameters
                {
                    Properties = new System.Collections.Generic.Dictionary<string, Property>
                    {
                        ["path"] = new() { Type = "string", Description = "Full or relative path to the file to read." }
                    },
                    Required = new[] { "path" }
                }
            };
        }

        public override async Task<string> ExecuteAsync(System.Collections.Generic.IDictionary<string, object> namedParameters)
        {
            var inputPath = namedParameters?["path"]?.ToString();
            if (string.IsNullOrWhiteSpace(inputPath))
                return "File path is empty or no parameters provided.";

            // First, use the common ResolvePath helper.
            var candidatePath = ResolvePath(inputPath);

            // If the file doesn't exist at the resolved path, search parent directories.
            var filePath = File.Exists(candidatePath)
                           ? candidatePath
                           : SearchParentDirectories(inputPath);

            if (filePath == null)
                return $"File not found: {inputPath}";

            if (!File.Exists(filePath))
                return $"File does not exist: {filePath}";

            var content = await ReadFileContentAsync(filePath);
            if (content.StartsWith("Error reading file:"))
                return content;

            return FormatContent(Path.GetExtension(filePath), content);
        }

        // Searches up from the default root for the file.
        private string SearchParentDirectories(string inputPath)
        {
            var currentDir = new DirectoryInfo(DefaultRootDirectory);
            while (currentDir != null && currentDir.FullName.StartsWith(@"C:\Users", StringComparison.OrdinalIgnoreCase))
            {
                var testPath = Path.GetFullPath(Path.Combine(currentDir.FullName, inputPath));
                if (File.Exists(testPath))
                    return testPath;
                currentDir = currentDir.Parent;
            }
            return null;
        }

        private async Task<string> ReadFileContentAsync(string filePath)
        {
            try
            {
                using var reader = new StreamReader(filePath);
                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                return $"Error reading file: {ex.Message}";
            }
        }

        private string FormatContent(string extension, string content)
        {
            if (extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var parsedJson = JsonSerializer.Deserialize<object>(content);
                    var prettyJson = JsonSerializer.Serialize(parsedJson, new JsonSerializerOptions { WriteIndented = true });
                    return $"File extension: {extension}\n\nParsed JSON:\n{prettyJson}";
                }
                catch (Exception ex)
                {
                    return $"File extension: {extension}\n\nRaw Content:\n{content}\n\nError parsing JSON: {ex.Message}";
                }
            }
            else
            {
                return $"File extension: {extension}\n\nContent:\n{content}";
            }
        }
    }
}
