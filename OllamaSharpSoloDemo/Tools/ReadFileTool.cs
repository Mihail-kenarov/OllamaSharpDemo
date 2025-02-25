using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OllamaSharp.Models.Chat;
using OllamaSharpSoloDemo.Support;

namespace OllamaSharpSoloDemo.Tools
{
    public sealed class ReadFileOnlyTool : DwmTool
    {
        public override string Name => Function.Name;

        public ReadFileOnlyTool()
        {
            Function = new Function
            {
                Description = "Reads contents from a file and returns the text.",
                Name = "read_file_only",
                Parameters = new Parameters
                {
                    Properties = new Dictionary<string, Property>
                    {
                        ["file_path"] = new() { Type = "string", Description = "Path to the file to read." },
                        ["encoding"] = new() { Type = "string", Description = "Encoding to use (default: utf-8)." }
                    },
                    Required = new[] { "file_path" }
                }
            };
        }

        public override async Task<string> ExecuteAsync(IDictionary<string, object> namedParameters)
        {
            string filePath = namedParameters?["file_path"]?.ToString();
            string encoding = namedParameters.ContainsKey("encoding") ? namedParameters["encoding"].ToString() : "utf-8";

            if (string.IsNullOrWhiteSpace(filePath))
                return "Error: File path is empty.";

            return await ReadFileAsync(filePath, encoding);
        }

        private async Task<string> ReadFileAsync(string filePath, string encoding)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(filePath);
                if (!fileInfo.Exists)
                    return $"Error: File not found - {filePath}";

                using var reader = new StreamReader(fileInfo.FullName, Encoding.GetEncoding(encoding));
                string content = await reader.ReadToEndAsync();

                return $"Contents of file '{filePath}':\n{content}\n";
            }
            catch (Exception ex)
            {
                return $"Error reading file '{filePath}': {ex.Message}";
            }
        }
    }
}
