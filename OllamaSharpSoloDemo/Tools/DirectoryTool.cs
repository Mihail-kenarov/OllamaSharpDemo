using OllamaSharp.Models.Chat;
using OllamaSharpSoloDemo.Support;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace OllamaSharpSoloDemo.Tools
{
    public sealed class DirectoryTool : DwmTool
    {
        private const string DefaultRootDirectory = @"C:\Users\kenar";

        public override string Name => Function.Name;

        public DirectoryTool()
        {
            Function = new Function
            {
                Description = "Lists the names of files and directories in a specified directory (only the first level) starting from C:\\Users\\kenar. " +
                              "Skips hidden files and directories, and returns two lists: one for directories and one for files.",
                Name = "list_directory",
                Parameters = new Parameters
                {
                    Properties = new Dictionary<string, Property>
                    {
                        ["path"] = new() { Type = "string", Description = "The path to the directory to list. Defaults to C:\\Users\\kenar." }
                    },
                    Required = new string[] { }
                }
            };
        }

        public override async Task<string> ExecuteAsync(IDictionary<string, object> namedParameters)
        {
            var path = GetDirectoryPath(namedParameters);
            return await ListDirectoryAsync(path);
        }

        private string GetDirectoryPath(IDictionary<string, object> namedParameters)
        {
            if (namedParameters != null &&
                namedParameters.ContainsKey("path") &&
                !string.IsNullOrWhiteSpace(namedParameters["path"]?.ToString()))
            {
                var inputPath = namedParameters["path"].ToString();
                // Resolve relative paths relative to the root directory
                string resolvedPath = Path.IsPathRooted(inputPath)
                    ? inputPath
                    : Path.Combine(DefaultRootDirectory, inputPath);
                return Path.GetFullPath(resolvedPath);
            }

            return DefaultRootDirectory; // Default to C:\Users\kenar
        }

        private async Task<string> ListDirectoryAsync(string directoryPath)
        {
            string fullPath = Path.GetFullPath(directoryPath);

            if (!Directory.Exists(fullPath))
            {
                return JsonSerializer.Serialize(new { error = $"Directory does not exist: {fullPath}" });
            }

            var dirInfo = new DirectoryInfo(fullPath);
            var rootItem = BuildDirectoryItem(dirInfo);

            // Use the helper to extract just the names of files and directories
            var result = ExtractNames(rootItem);

            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }

        private LlmFileModel BuildDirectoryItem(DirectoryInfo dirInfo)
        {
            var item = new LlmFileModel
            {
                Filename = dirInfo.Name,
                Size = 0,
                LastModified = dirInfo.LastWriteTimeUtc,
                IsFile = false,
                IsDirectory = true,
                Contents = new List<LlmFileModel>()
            };

            try
            {
                // Add non-hidden files in the current directory
                foreach (var file in dirInfo.GetFiles())
                {
                    if ((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                        continue; // Skip hidden files

                    item.Contents.Add(new LlmFileModel
                    {
                        Filename = file.Name,
                        Size = file.Length,
                        LastModified = file.LastWriteTimeUtc,
                        IsFile = true,
                        IsDirectory = false,
                        Contents = null
                    });
                }

                // Add non-hidden subdirectories (only one level deep, no recursion)
                foreach (var subDir in dirInfo.GetDirectories())
                {
                    if ((subDir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                        continue; // Skip hidden directories

                    item.Contents.Add(new LlmFileModel
                    {
                        Filename = subDir.Name,
                        Size = 0,
                        LastModified = subDir.LastWriteTimeUtc,
                        IsFile = false,
                        IsDirectory = true,
                        Contents = new List<LlmFileModel>() // or null, if you prefer
                    });
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Handle access denied
                item.Contents.Add(new LlmFileModel
                {
                    Filename = "Access Denied",
                    Size = 0,
                    LastModified = DateTime.UtcNow,
                    IsFile = false,
                    IsDirectory = true,
                    Contents = null
                });
            }
            catch (Exception ex)
            {
                item.Contents.Add(new LlmFileModel
                {
                    Filename = $"Error: {ex.Message}",
                    Size = 0,
                    LastModified = DateTime.UtcNow,
                    IsFile = false,
                    IsDirectory = true,
                    Contents = null
                });
            }

            return item;
        }

        /// <summary>
        /// Extracts the names of files and directories from the first level of the provided LlmFileModel.
        /// </summary>
        /// <param name="model">The root model containing the contents of the directory.</param>
        /// <returns>An object with two lists: Directories and Files.</returns>
        private object ExtractNames(LlmFileModel model)
        {
            var files = new List<string>();
            var directories = new List<string>();

            // Process only the immediate children
            foreach (var item in model.Contents)
            {
                if (item.IsFile)
                {
                    files.Add(item.Filename);
                }
                else if (item.IsDirectory)
                {
                    directories.Add(item.Filename);
                }
            }

            return new
            {
                Directories = directories,
                Files = files
            };
        }
    }
}
