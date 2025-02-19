using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OllamaSharpSoloDemo.Tools;

namespace OllamaSharpSoloDemo
{
    public class ToolBox
    {
        private readonly ToolDirectoryComponents toolDirectoryComponents;

        public ToolBox(ToolDirectoryComponents _toolDirectoryComponents) 
        {
            toolDirectoryComponents = _toolDirectoryComponents;
        }

        public void OpenDirectory(string path)
        {
            toolDirectoryComponents.OpenDirectory(path);
        }
        public void PrintCurrentDirectory()
        {
            toolDirectoryComponents.PrintCurrentDirectory();
        }

        public string GetCurrentDirectory()
        {
            return toolDirectoryComponents.GetCurrentDirectory();
        }
    }
}
