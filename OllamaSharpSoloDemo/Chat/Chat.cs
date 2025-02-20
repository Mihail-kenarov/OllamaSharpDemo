using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharpSoloDemo.Chat;

namespace OllamaSharpSoloDemo.Chat
{
    public class Chat
    {
        private readonly OllamaApiClient _ollama;

        public Chat(OllamaApiClient ollama)
        {
            _ollama = ollama;
        }

        public async Task<ChatResponse> SendAsync(ChatMessage message)
        {
            var sb = new StringBuilder();

            await foreach (var token in _ollama.GenerateAsync(message.Content))
            {
                // Append the generated text using the Response property.
                sb.Append(token.Response);
            }

            // Return the generated response in a ChatResponse.
            return new ChatResponse
            {
                Content = sb.ToString(),
                ToolCall = null // Optionally, parse token output to detect tool calls.
            };
        }
    }
}
