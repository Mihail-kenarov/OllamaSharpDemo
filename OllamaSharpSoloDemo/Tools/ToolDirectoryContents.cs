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
                string fullPath = Path.GetFullPath(path);

                if (!IsValidPath(fullPath))
                {
                    Console.WriteLine("Invalid directory path.");
                    return;
                }

                if (DirectoryExists(fullPath))
                {
                    Console.WriteLine($"Opening directory: {fullPath}");

                    string[] files = Directory.GetFiles(fullPath);
                    string[] directories = Directory.GetDirectories(fullPath);

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

        public string GetCurrentDirectory() => Directory.GetCurrentDirectory();

        public void PrintCurrentDirectory()
        {
            string currentDir = GetCurrentDirectory();
            Console.WriteLine($"\nCurrent Working Directory: {currentDir}");
        }

        private bool IsValidPath(string path) => Path.IsPathRooted(path) && !string.IsNullOrWhiteSpace(path);

        private bool DirectoryExists(string path) => Directory.Exists(path);

        private void PrintFiles(string[] files)
        {
            Console.WriteLine("Files:");
            if (files.Length == 0)
            {
                Console.WriteLine("No files found in this directory.");
            }
            else
            {
                foreach (var file in files)
                {
                    Console.WriteLine(Path.GetFileName(file));
                }
            }
        }

        private void PrintDirectories(string[] directories)
        {
            Console.WriteLine("Directories:");
            if (directories.Length == 0)
            {
                Console.WriteLine("No subdirectories found.");
            }
            else
            {
                foreach (var dir in directories)
                {
                    Console.WriteLine(Path.GetFileName(dir));
                }
            }
        }
    }
}
