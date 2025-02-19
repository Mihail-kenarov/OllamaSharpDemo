using System;
using OllamaSharpSoloDemo.Tools;

namespace OllamaSharpSoloDemo
{
    public class ToolBox
    {
        private readonly ToolDirectoryComponents toolDirectoryComponents;
        private readonly ToolTextRead toolTextRead;

        public ToolBox(ToolDirectoryComponents toolDirectoryComponents, ToolTextRead toolTextRead)
        {
            this.toolDirectoryComponents = toolDirectoryComponents;
            this.toolTextRead = toolTextRead;
        }

        public void OpenDirectory(string path) => toolDirectoryComponents.OpenDirectory(path);

        public void PrintCurrentDirectory() => toolDirectoryComponents.PrintCurrentDirectory();

        public string GetCurrentDirectory() => toolDirectoryComponents.GetCurrentDirectory();

        public void ReadText(string filePath) => toolTextRead.ReadText(filePath);
    }
}
