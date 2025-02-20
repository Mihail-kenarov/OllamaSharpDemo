using OllamaSharp.Models.Chat;
using OllamaSharpSoloDemo.Support;

namespace OllamaSharpSoloDemo.Tools
{
    public sealed class DirectoryTool : DwmTool
    {
        public override string Name => Function.Name;

        public DirectoryTool()
        {
            Function = new Function
            {
                Description = "Lists the files and directories in a specified directory. You are looking for a specific word that is common when it comes to the" +
                "structure of the computers and the If no path is provided, defaults to the current working directory.",
                Name = "list_directory",
                Parameters = new Parameters
                {
                    Properties = new Dictionary<string, Property>
                    {
                        ["path"] = new() { Type = "string", Description = "The path to the directory to list files and directories for. Defaults to the current working directory." }
                    },
                    Required = ["path"],
                }
            };
        }

        public override async Task<string> ExecuteAsync(IDictionary<string, object> namedParameters)
        {
            if (namedParameters == null)
                return null;

            var path = GetDirectoryPath(namedParameters);
            return await ListDirectoryAsync(path, namedParameters);
        }

        /// <summary>
        /// Lists the files and directories in a specified directory.
        /// If no path is provided, defaults to the current working directory.
        /// </summary>
        /// <param name="parameters">
        /// IDictionary that can include an optional "path" key.
        /// </param>
        /// <returns>A formatted string of directory contents or an error message.</returns>
        public static async Task<string> ListDirectoryAsync(string directoryPath, IDictionary<string, object> parameters)
        {
            // Safely get the path parameter or default to current directory
            string fullPath = Path.GetFullPath(directoryPath);

            if (!IsValidDirectory(fullPath))
            {
                return $"Directory does not exist: {fullPath}";
            }

            var fileNames = GetSortedFileNames(fullPath);
            var directoryNames = GetSortedDirectoryNames(fullPath);

            return await FormatOutputAsync(fullPath, fileNames, directoryNames);
        }

        private static string GetDirectoryPath(IDictionary<string, object> parameters)
        {
            // Check if "path" is present AND not empty
            if (parameters != null && parameters.ContainsKey("path") && !string.IsNullOrWhiteSpace(parameters["path"]?.ToString()))
            {
                var path = parameters["path"].ToString();
                if (Directory.Exists(path))
                    return path;
            }

            // Otherwise, default to current directory
            return Directory.GetCurrentDirectory();
        }

        private static bool IsValidDirectory(string fullPath)
        {
            return Directory.Exists(fullPath);
        }

        private static List<string> GetSortedFileNames(string fullPath)
        {
            var fileArray = Directory.GetFiles(fullPath);
            return fileArray
                .Select(file => Path.GetFileName(file))
                .OrderBy(name => name)
                .ToList();
        }

        private static List<string> GetSortedDirectoryNames(string fullPath)
        {
            var directoryArray = Directory.GetDirectories(fullPath);
            return directoryArray
                .Select(dir => Path.GetFileName(dir))
                .OrderBy(name => name)
                .ToList();
        }

        private static Task<string> FormatOutputAsync(string fullPath, List<string> fileNames, List<string> directoryNames)
        {
            var filesOutput = fileNames.Count > 0
                ? string.Join("\n", fileNames)
                : "No files found";

            var directoriesOutput = directoryNames.Count > 0
                ? string.Join("\n", directoryNames)
                : "No subdirectories found";

            return Task.FromResult($"Contents of directory {fullPath}:\nFiles:\n\n{filesOutput}\n\nDirectories:\n{directoriesOutput}");
        }
    }
}
