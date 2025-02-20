using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace OllamaSharpSoloDemo.Tools
{

    public class ToolDefinition
    {
        /// <summary>
        /// The function delegate that implements the tool.
        /// Expects a dictionary of parameters and returns a string output.
        /// </summary>
        public Func<Dictionary<string, object>, string> Function { get; set; }

        /// <summary>
        /// The metadata that describes the tool.
        /// </summary>
        public ToolMetadata Metadata { get; set; }
    }

}
