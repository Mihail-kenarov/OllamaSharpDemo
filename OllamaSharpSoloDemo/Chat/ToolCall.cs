using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OllamaSharpSoloDemo.Chat
{

    public class ToolCall
    {
        public string Name { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
