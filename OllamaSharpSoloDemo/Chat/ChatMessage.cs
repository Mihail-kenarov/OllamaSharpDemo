using OllamaSharpSoloDemo.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OllamaSharpSoloDemo.Chat
{
    public class ChatMessage
    {
        public string Role { get; set; }
        public string Content { get; set; }
        public IEnumerable<ToolMetadata> Tools { get; set; }
    }
}
