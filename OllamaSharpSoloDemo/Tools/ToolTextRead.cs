using System;
using System.IO;
using OllamaSharpSoloDemo.Tools;

namespace OllamaSharpSoloDemo.Tools
{
    public class ToolTextRead
    {
        private readonly ToolDirectoryComponents toolDirectoryComponents;

        public ToolTextRead(ToolDirectoryComponents toolDirectoryComponents)
        { 
            this.toolDirectoryComponents = toolDirectoryComponents; 
        }

        public void ReadText(string filePath)
        {
            try
            {
                string fullPath = Path.GetFullPath(filePath);

                if (!toolDirectoryComponents.IsValidPath(fullPath))
                {
                    Console.WriteLine("Invalid file path.");
                    return;
                }

                if (!File.Exists(fullPath))
                {
                    Console.WriteLine("File does not exist. Please check the path and try again.");
                    return;
                }

                string content = File.ReadAllText(fullPath);
                Console.WriteLine($"\nContents of {Path.GetFileName(fullPath)}:\n");
                Console.WriteLine(content);
                Console.WriteLine("==============================");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while reading the file: {ex.Message}");
            }
        }
    }
}
