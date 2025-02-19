using System;
using OllamaSharpSoloDemo.Tools;

namespace OllamaSharpSoloDemo
{
    public class ToolBox
    {
        private readonly ToolDirectoryComponents toolDirectoryComponents;

        public ToolBox(ToolDirectoryComponents toolDirectoryComponents)
            => this.toolDirectoryComponents = toolDirectoryComponents;

        public void OpenDirectory(string path) => toolDirectoryComponents.OpenDirectory(path);

        public void PrintCurrentDirectory() => toolDirectoryComponents.PrintCurrentDirectory();

        public string GetCurrentDirectory() => toolDirectoryComponents.GetCurrentDirectory();
    }
}
