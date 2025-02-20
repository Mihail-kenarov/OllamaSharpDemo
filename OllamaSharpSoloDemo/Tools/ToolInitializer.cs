using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OllamaSharpSoloDemo.Tools
{
    public static class ToolInitializer
    {
  
        /// Initializes and registers all tools in the application.
        public static void InitializeTools()
        {
            // Register the Directory Tool.
            var listDirectoryMetadata = new ToolMetadata
            {
                Name = "list_directory",
                Description = "Lists files and directories in the specified directory. Defaults to the current directory if no path is provided.",
                Parameters = new Dictionary<string, object>
                {
                    { "path", "string (optional)" }
                }
            };

            ToolRegistry.RegisterTool("list_directory", DirectoryTool.ListDirectory, listDirectoryMetadata);

            // You can register additional tools here.
            // For example:
            // ToolRegistry.RegisterTool("read_file", FileTool.ReadFile, readFileMetadata);
        }
    }
}
