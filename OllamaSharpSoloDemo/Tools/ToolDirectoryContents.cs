using System;
using System.IO;

namespace OllamaSharpSoloDemo.Tools
{
    public class ToolDirectoryComponents
    {
        public void OpenDirectory(string path)
        {
            try
            {
                if (!IsValidPath(path))
                {
                    Console.WriteLine("Invalid directory path.");
                    return;
                }

                if (DirectoryExists(path))
                {
                    Console.WriteLine($"Opening directory: {path}");

                    string[] files = Directory.GetFiles(path);
                    string[] directories = Directory.GetDirectories(path);

                    PrintFiles(files);
                    PrintDirectories(directories);
                }
                else
                {
                    Console.WriteLine("Directory does not exist. Please check the path and try again.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public string GetCurrentDirectory()
        {
            return Directory.GetCurrentDirectory(); 
        }

        public void PrintCurrentDirectory()
        {
            string currentDir = GetCurrentDirectory();
            Console.WriteLine($"\nCurrent Working Directory: {currentDir}");
        }

        private bool IsValidPath(string path) => !string.IsNullOrWhiteSpace(path);

        private bool DirectoryExists(string path) => Directory.Exists(path);

        private void PrintFiles(string[] files)
        {
            Console.WriteLine("Files:");
            if (files.Length == 0)
            {
                Console.WriteLine("No files found in this directory.");
            }
            foreach (var file in files)
            {
                Console.WriteLine(file);
            }
        }

        private void PrintDirectories(string[] directories)
        {
            Console.WriteLine("Directories:");
            foreach (var dir in directories)
            {
                Console.WriteLine(dir);
            }
        }
    }
}
