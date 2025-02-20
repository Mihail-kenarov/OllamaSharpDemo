using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace OllamaSharpSoloDemo.Tools
{
    /// <summary>
    /// A helper class that manages the registration and lookup of tools.
    /// </summary>
    public static class ToolRegistry
    {
        // Dictionary mapping a tool's name to its ToolDefinition (which holds the function delegate and metadata).
        private static readonly Dictionary<string, ToolDefinition> _tools = new Dictionary<string, ToolDefinition>();

        /// Registers a new tool in the registry.
        /// name - The unique identifier for the tool.
        /// function - The function implementing the tool's functionality.
        /// metadata - Metadata describing the tool.
        public static void RegisterTool(string name, Func<Dictionary<string, object>, string> function, ToolMetadata metadata)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Tool name cannot be null or empty.", nameof(name));

            if (_tools.ContainsKey(name))
                throw new ArgumentException($"A tool with the name '{name}' is already registered.");

            _tools[name] = new ToolDefinition { Function = function, Metadata = metadata };
        }

        /// Tries to retrieve a tool from the registry.
        /// name - The tool's unique name.
        /// toolDefinition - The tool's definition if found.
        /// Returns true if the tool is found; otherwise, false.
        public static bool TryGetTool(string name, out ToolDefinition toolDefinition)
        {
            return _tools.TryGetValue(name, out toolDefinition);
        }

        /// Returns an enumerable list of all registered tool metadata.
        public static IEnumerable<ToolMetadata> GetAllToolMetadata()
        {
            var allMetadata = new List<ToolMetadata>();
            foreach (var tool in _tools.Values)
            {
                allMetadata.Add(tool.Metadata);
            }
            return allMetadata;
        }
    }
}
