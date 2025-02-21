using OllamaSharp.Models.Chat;
using OllamaSharpSoloDemo.Support;
using System.Text.Json;
using System.IO;

namespace OllamaSharpSoloDemo.Tools
{
    public sealed class ReadFileTool : DwmTool
    {
        // Set your default root directory here.
        private const string DefaultRootDirectory = @"C:\Users\kenar";

        public override string Name => Function.Name;

        public ReadFileTool()
        {
            Function = new Function
            {
                Description = "Reads the content of a specified file. If it's JSON, returns parsed JSON; otherwise returns raw text. " +
                              "Can accept relative paths (resolved from C:\\Users\\kenar). Tries searching parent directories if not found." +
                              "If you are to not be able to see the document from the relative path, go 2-3 layers deeper to see if the request directory or files is there",
                Name = "read_file",
                Parameters = new Parameters
                {
                    Properties = new Dictionary<string, Property>
                    {
                        ["path"] = new()
                        {
                            Type = "string",
                            Description = "Full or relative path to the file to read."
                        }
                    },
                    Required = new[] { "path" }
                }
            };
        }

        public override async Task<string> ExecuteAsync(IDictionary<string, object> namedParameters)
        {
            // Validate parameters and extract the input path.
            var inputPath = ValidateAndExtractPath(namedParameters);
            if (inputPath == null)
                return "File path is empty or no parameters provided.";

            // Resolve the file path using the helper.
            var filePath = ResolveFilePath(inputPath);
            if (filePath == null)
                return $"File not found: {inputPath}";

            // Confirm that the file exists.
            if (!File.Exists(filePath))
                return $"File does not exist: {filePath}";

            // Get file extension.
            var extension = Path.GetExtension(filePath);

            // Read file content.
            var content = await ReadFileContentAsync(filePath);
            if (content.StartsWith("Error reading file:"))
                return content; // Propagate error message.

            // Format content based on file type (JSON vs. others).
            return FormatContent(extension, content);
        }

        
        /// Validates the input parameters and extracts the file path.
        private string ValidateAndExtractPath(IDictionary<string, object> parameters)
        {
            if (parameters == null)
                return null;

            var path = parameters.ContainsKey("path") ? parameters["path"]?.ToString() : null;
            return string.IsNullOrWhiteSpace(path) ? null : path;
        }

        /// <summary>
        /// Attempts to locate the file by:
        /// 1) Checking if the path is absolute and exists.
        /// 2) If relative, combining with DefaultRootDirectory and checking existence.
        /// 3) If still not found, walking up the parent directories of DefaultRootDirectory (until reaching C:\Users) to see if the file is there.
        /// </summary>
        private string ResolveFilePath(string inputPath)
        {
            // If the path is absolute, check it directly.
            if (Path.IsPathRooted(inputPath))
            {
                var absolutePath = Path.GetFullPath(inputPath);
                return File.Exists(absolutePath) ? absolutePath : null;
            }

            // Otherwise, combine with the default root directory.
            var candidatePath = Path.Combine(DefaultRootDirectory, inputPath);
            candidatePath = Path.GetFullPath(candidatePath);
            if (File.Exists(candidatePath))
                return candidatePath;

            // Optionally search up the directory tree from DefaultRootDirectory.
            var currentDir = new DirectoryInfo(DefaultRootDirectory);
            while (currentDir != null && currentDir.FullName.StartsWith(@"C:\Users", StringComparison.OrdinalIgnoreCase))
            {
                var testPath = Path.Combine(currentDir.FullName, inputPath);
                testPath = Path.GetFullPath(testPath);
                if (File.Exists(testPath))
                    return testPath;

                currentDir = currentDir.Parent;
            }

            // Not found.
            return null;
        }

        private async Task<string> ReadFileContentAsync(string filePath)
        {
            try
            {
                using (var reader = new StreamReader(filePath))
                {
                    return await reader.ReadToEndAsync();
                }
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
