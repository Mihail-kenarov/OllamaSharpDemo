using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using OllamaSharpSoloDemo.Support;

namespace OllamaSharpSoloDemo.Tools
{
    public sealed class SummarizeTextTool : DwmTool
    {
        public override string Name => Function.Name;
        private readonly OllamaApiClient _ollamaClient;

        public SummarizeTextTool(OllamaApiClient ollamaClient)
        {
            _ollamaClient = ollamaClient;
            Function = new Function
            {
                Description = "Summarizes input text using OllamaSharp.",
                Name = "summarize_text",
                Parameters = new Parameters
                {
                    Properties = new Dictionary<string, Property>
                    {
                        ["text"] = new() { Type = "string", Description = "Text to summarize." },
                        ["context"] = new() { Type = "string", Description = "Optional context for better summarization." }
                    },
                    Required = new[] { "text" }
                }
            };
        }

        public override async Task<string> ExecuteAsync(IDictionary<string, object> namedParameters)
        {
            string text = namedParameters?["text"]?.ToString();
            string context = namedParameters.ContainsKey("context") ? namedParameters["context"].ToString() : string.Empty;

            if (string.IsNullOrWhiteSpace(text))
                return "Error: No text provided for summarization.";

            return await SummarizeWithOllamaAsync(text, context);
        }

        private async Task<string> SummarizeWithOllamaAsync(string text, string context)
        {
            string prompt = "Summarize the following text concisely. Return only the summary with NO OTHER COMMENTS:\n\n";

            if (text.Length < 50)
                text = context;
            else if (context.Length > 50)
                prompt = $"Given this context:\n\n{context}\n\n" + prompt;

            try
            {
                var chatRequest = new ChatRequest
                {
                    Model = "llama3.2:latest",
                    Messages = new List<Message>
                    {
                        new Message { Role = "system", Content = prompt },
                        new Message { Role = "user", Content = text }
                    }
                };

                var responseContent = "";
                await foreach (var response in _ollamaClient.ChatAsync(chatRequest))
                {
                    responseContent += response.Message?.Content ?? "";
                }

                return string.IsNullOrWhiteSpace(responseContent) ? "Error: No response from LLM." : responseContent;
            }
            catch (Exception ex)
            {
                return $"Error summarizing text: {ex.Message}";
            }
        }
    }
}
