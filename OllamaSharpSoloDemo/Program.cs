using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using OllamaSharp.Models.Exceptions;
using OllamaSharpSoloDemo.Support;
using OllamaSharpSoloDemo.Tools;

class Program
{
    private static Uri uri;
    private static OllamaApiClient ollama;
    private static string message;

    static async Task Main()
    {
        ClientSetup();
        var models = await ShowModels();
        SelectModel(models);
        await StartChat();
    }

    private static void ClientSetup()
    {
        uri = new Uri("http://localhost:11434");
        ollama = new OllamaApiClient(uri);
    }

    private static async Task<List<string>> ShowModels()
    {
        var models = await ollama.ListLocalModelsAsync();

        Console.WriteLine("Available models:");
        foreach (var model in models)
        {
            Console.WriteLine(model.Name);
        }
        Console.WriteLine();

        return models.Select(m => m.Name).ToList();
    }

    private static void SelectModel(List<string> models)
    {
        while (true)
        {
            Console.WriteLine("Write the name of the model that you would like to use:");
            var selectedModel = Console.ReadLine()?.Trim();

            if (!string.IsNullOrWhiteSpace(selectedModel) && models.Contains(selectedModel, StringComparer.OrdinalIgnoreCase))
            {
                ollama.SelectedModel = selectedModel;
                Console.WriteLine($"\nYou have selected to work with {ollama.SelectedModel}\n");
                Console.WriteLine("Feel free to start your chat!\n");
                break;
            }
            else
            {
                Console.WriteLine("\nInvalid model. Please select one from the list above.\n");
            }
        }
    }

    private static async Task StartChat()
    {
        var chat = new Chat(ollama);
        chat.Options = new OllamaSharp.Models.RequestOptions
        {
            Temperature = 0f
        };

        var tools = new List<DwmTool>
        {
            new DirectoryTool(),
            new ReadFileOnlyTool(),
            new SummarizeTextTool(ollama)
        };

        Dictionary<string, string> fileContents = new();

        while (true)
        {
            Console.Write("User: ");
            message = Console.ReadLine();

            var exit = false;
            var send = true;
            switch (message.ToLowerInvariant())
            {
                case "bye":
                case "goodbye":
                case "chao":
                case "exit":
                    exit = true;
                    break;

                case "/clear":
                    chat.Messages.RemoveAll(x => x.Role != "system");
                    send = false;
                    break;
            }

            if (exit)
            {
                Console.WriteLine("Assistant: Goodbye!");
                break;
            }

            if (send)
            {
                try
                {
                    // Implement hybrid approach for tool vs direct LLM response determination
                    bool useTool = ShouldUseTool(message, tools);

                    if (useTool)
                    {
                        // Continue with the existing tool-calling logic
                        await foreach (var answerToken in chat.SendAsync(message, tools))
                            Console.Write(answerToken);
                        Console.WriteLine();

                        await ProcessToolCalls(chat, tools, fileContents);
                    }
                    else
                    {
                        // Use direct LLM response without tools
                        await foreach (var answerToken in chat.SendAsync(message))
                            Console.Write(answerToken);
                        Console.WriteLine();
                    }
                }
                catch (OllamaException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
    }

    private static bool ShouldUseTool(string message, List<DwmTool> tools)
    {
        // STEP 1: Fast path with keyword matching for specific tools

        // DirectoryTool keywords
        if (ContainsKeywords(message, new[] { "list", "directory", "folder", "files", "show files" }))
        {
            return true;
        }

        // ReadFileOnlyTool keywords
        if (ContainsKeywords(message, new[] { "read file", "open file", "show content", "display file", "get file" }))
        {
            return true;
        }

        // SummarizeTextTool keywords
        if (ContainsKeywords(message, new[] { "summarize", "summary", "brief", "shorten" }))
        {
            return true;
        }

        // STEP 2: For unclear cases, determine if it's a general knowledge/conversation query
        if (IsGeneralKnowledgeQuery(message))
        {
            return false;
        }

        // STEP 3: If still uncertain, use LLM classification (simplified here)
        // In a production environment, you would add the LLM classification here
        // For demo purposes, we'll use a heuristic
        if (message.Length < 15 || message.EndsWith("?"))
        {
            // Short queries or questions are more likely general conversation
            return false;
        }

        // Default to using tools for anything else
        // In production, you'd want a more sophisticated approach here
        return true;
    }

    private static bool ContainsKeywords(string input, string[] keywords)
    {
        return keywords.Any(keyword =>
            input.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsGeneralKnowledgeQuery(string message)
    {
        // General knowledge patterns
        string[] generalPatterns = new[] {
            "what is", "how does", "explain", "tell me about",
            "meaning of", "define", "difference between",
            "who is", "when did", "why is"
        };

        return generalPatterns.Any(pattern =>
            message.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task ProcessToolCalls(Chat chat, List<DwmTool> tools, Dictionary<string, string> fileContents)
    {
        var toolCalls = chat.Messages.LastOrDefault()?.ToolCalls?.ToArray() ?? [];
        if (toolCalls.Any())
        {
            Console.WriteLine("Tools used:");
            foreach (var function in toolCalls.Where(t => t.Function != null).Select(t => t.Function))
            {
                Console.WriteLine($"  - {function.Name}");
                Console.WriteLine($"  - Parameter:");

                if (function.Arguments is not null)
                {
                    foreach (var argument in function.Arguments)
                        Console.WriteLine($"  {argument.Key}: {argument.Value}");
                }

                var tool = tools.FirstOrDefault(t => t.Name == function.Name);
                var response = tool == null
                    ? "Tool not found"
                    : await tool.ExecuteAsync(function.Arguments);

                if (function.Name == "read_file_only" && function.Arguments.ContainsKey("file_path"))
                {
                    string filePath = function.Arguments["file_path"].ToString();
                    if (!string.IsNullOrWhiteSpace(filePath))
                    {
                        fileContents[filePath] = response;
                    }
                }

                Console.WriteLine($"- Return value: \"{response}\"");

                if (function.Name == "summarize_text")
                {
                    string fileToSummarize = function.Arguments.ContainsKey("file_path")
                        ? function.Arguments["file_path"].ToString()
                        : null;

                    string contentToSummarize = null;

                    if (!string.IsNullOrWhiteSpace(fileToSummarize) && fileContents.ContainsKey(fileToSummarize))
                    {
                        contentToSummarize = fileContents[fileToSummarize];
                    }
                    else if (fileContents.Count == 1 && function.Arguments.ContainsKey("file_path"))
                    {
                        contentToSummarize = fileContents.Values.First();
                    }
                    else
                    {
                        response = "Error: Please specify which file to summarize.";
                    }

                    if (!string.IsNullOrWhiteSpace(contentToSummarize))
                    {
                        var summarizeTool = tools.FirstOrDefault(t => t.Name == "summarize_text");
                        response = summarizeTool == null
                            ? "Summarization tool not found."
                            : await summarizeTool.ExecuteAsync(new Dictionary<string, object> { { "text", contentToSummarize } });
                    }

                    Console.WriteLine($"- Return value: \"{response}\"");
                }

                await foreach (var answerToken in chat.SendAsAsync("tool", response, tools))
                    Console.Write(answerToken);
                Console.WriteLine();
            }
        }
    }
}