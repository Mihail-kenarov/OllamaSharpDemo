using OllamaSharp.Models.Chat;
using OllamaSharpSoloDemo.Support;

namespace OllamaSharpSoloDemo.Tools
{
    public sealed class ReadFileTool : DwmTool
    {
        public override string Name => Function.Name;

        public ReadFileTool()
        {
            Function = new Function
            {
                Description = "Reads the content of a specified file and returns its extension along with the file content. Requires the file path.",
                Name = "read_file",
                Parameters = new Parameters
                {
                    Properties = new Dictionary<string, Property>
                    {
                        ["path"] = new() { Type = "string", Description = "The full path to the file to read." }
                    },
                    Required = ["path"]
                }
            };
        }

        public override async Task<string> ExecuteAsync(IDictionary<string, object> namedParameters)
        {
            if (namedParameters == null)
                return "No parameters provided.";

            var filePath = namedParameters["path"]?.ToString();

            // Validate the file path using a separate method.
            var validationMessage = ValidateFilePath(filePath);
            if (!string.IsNullOrEmpty(validationMessage))
                return validationMessage;

            // Get the file extension.
            var extension = GetFileExtension(filePath);

            // Read the file content.
            var content = await ReadFileContentAsync(filePath);
            if (content == null)
                return "Error reading file.";

            return $"File extension: {extension}\n\nContent:\n{content}";
        }

        private string ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return "File path is empty.";

            if (!File.Exists(filePath))
                return $"File does not exist: {filePath}";

            return null;
        }

        private string GetFileExtension(string filePath)
        {
            return Path.GetExtension(filePath);
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
    }
}
