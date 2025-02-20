using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharpSoloDemo;
using OllamaSharpSoloDemo.Tools;
using OllamaSharpSoloDemo.Chat;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    private static Uri uri;
    private static OllamaApiClient ollama;
    private static string message;
    private static readonly HashSet<string> exitWords = new HashSet<string> { "bye", "goodbye", "chao" };

    static async Task Main()
    {
        ClientSetup();

        // Initialize tools (ToolInitializer or similar)
        ToolInitializer.InitializeTools();

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
            var selectedModel = Console.ReadLine();

            if (models.Contains(selectedModel, StringComparer.OrdinalIgnoreCase))
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
        var chat = new OllamaSharpSoloDemo.Chat.Chat(ollama);

        while (true)
        {
            Console.Write("User: ");
            message = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(message))
                break;

            if (exitWords.Contains(message.ToLower()))
            {
                Console.WriteLine("Assistant: Goodbye!");
                break;
            }

            // Create a message payload that includes the tools metadata.
            // Assume ChatMessage is a model that accepts a Tools property.
            var toolsMetadata = ToolRegistry.GetAllToolMetadata().ToList();
            var chatMessage = new ChatMessage
            {
                Role = "user",
                Content = message,
                Tools = toolsMetadata  // This informs the model of the available tools.
            };

            // Send the chat message and get the response.
            ChatResponse response = await chat.SendAsync(chatMessage);

            // Check if the response includes a tool call.
            if (response.ToolCall != null)
            {
                // Look up the tool from the registry.
                if (ToolRegistry.TryGetTool(response.ToolCall.Name, out ToolDefinition toolDefinition))
                {
                    // Execute the tool using the parameters from the response.
                    string toolOutput = toolDefinition.Function(response.ToolCall.Parameters);
                    Console.WriteLine($"\nAssistant (Tool Output): {toolOutput}");
                }
                else
                {
                    Console.WriteLine("\nAssistant: Requested tool not found.");
                }
            }
            else
            {
                // If no tool call, simply output the content.
                Console.WriteLine($"\nAssistant: {response.Content}");
            }

            Console.WriteLine("\n==============================");
        }
    }
}
