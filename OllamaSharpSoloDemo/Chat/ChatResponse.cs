using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OllamaSharp.Models.Chat.Message;

namespace OllamaSharpSoloDemo.Chat
{
    public class ChatResponse
    {
        public string Content { get; set; }
        public ToolCall ToolCall { get; set; }
    }
}
