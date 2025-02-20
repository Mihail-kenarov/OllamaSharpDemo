using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OllamaSharpSoloDemo.Tools
{
    public class DirectoryTool
    {
        /// <summary>
        /// Lists the files and directories in a specified directory.
        /// If no path is provided, defaults to the current working directory.
        /// </summary>
        /// <param name="parameters">
        /// Dictionary that can include an optional "path" key.
        /// </param>
        /// <returns>A formatted string of directory contents or an error message.</returns>
        public static string ListDirectory(Dictionary<string, object> parameters)
        {
            // Extract the directory path.
            string directoryPath = GetDirectoryPath(parameters);
            string fullPath = Path.GetFullPath(directoryPath);

            if (!IsValidDirectory(fullPath))
            {
                return $"Directory does not exist: {fullPath}";
            }

            List<string> fileNames = GetSortedFileNames(fullPath);
            List<string> directoryNames = GetSortedDirectoryNames(fullPath);

          
            return FormatOutput(fullPath, fileNames, directoryNames);
        }

     
        private static string GetDirectoryPath(Dictionary<string, object> parameters)
        {
            return parameters.ContainsKey("path") ? parameters["path"].ToString() : Directory.GetCurrentDirectory();
        }

    
        private static bool IsValidDirectory(string fullPath)
        {
            return Directory.Exists(fullPath);
        }

        private static List<string> GetSortedFileNames(string fullPath)
        {
            string[] fileArray = Directory.GetFiles(fullPath);
            List<string> fileNames = fileArray
                .Select(file => Path.GetFileName(file))
                .OrderBy(name => name)
                .ToList();
            return fileNames;
        }
        private static List<string> GetSortedDirectoryNames(string fullPath)
        {
            string[] directoryArray = Directory.GetDirectories(fullPath);
            List<string> directoryNames = directoryArray
                .Select(dir => Path.GetFileName(dir))
                .OrderBy(name => name)
                .ToList();
            return directoryNames;
        }

        private static string FormatOutput(string fullPath, List<string> fileNames, List<string> directoryNames)
        {
            string filesOutput = fileNames.Count > 0 ? string.Join(", ", fileNames) : "No files found";
            string directoriesOutput = directoryNames.Count > 0 ? string.Join(", ", directoryNames) : "No subdirectories found";
            return $"Contents of directory {fullPath}:\nFiles: {filesOutput}\nDirectories: {directoriesOutput}";
        }
    }
}
