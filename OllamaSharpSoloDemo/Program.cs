using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharpSoloDemo;
using OllamaSharpSoloDemo.Tools;
using System.IO;

class Program
{
    private static Uri uri;
    private static OllamaApiClient ollama;
    private static string message;
    private static readonly HashSet<string> exitWords = new HashSet<string> { "bye", "goodbye", "chao" };
    private static readonly ToolBox toolBox = new ToolBox(new ToolDirectoryComponents());

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
        var chat = new Chat(ollama);

        while (true)
        {
            Console.Write("User: ");
            message = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(message))
                break;

            var lowerMessage = message.ToLower();

            if (exitWords.Contains(lowerMessage))
            {
                Console.WriteLine("Assistant: Goodbye!");
                break;
            }

            switch (lowerMessage)
            {
                case string msg when msg.StartsWith("show directory:"):
                    HandleDirectoryRequest(message);
                    break;

                case "pwd":
                    HandleCurrentDirectoryRequest();
                    break;

                case "print my current directory":
                    HandlePrintCurrentDirectory();
                    break;

                default:
                    Console.Write("Assistant: ");
                    await foreach (var answerToken in chat.SendAsync(message))
                    {
                        Console.Write(answerToken);
                    }
                    Console.WriteLine("\n==============================");
                    break;
            }
        }
    }

    private static void HandleDirectoryRequest(string message)
    {
        var directoryPath = message.Substring("show directory:".Length).Trim();

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            Console.WriteLine("\nAssistant: Please provide a valid directory path.");
            return;
        }

        Console.WriteLine($"\nAssistant: Checking directory {directoryPath}...");
        toolBox.OpenDirectory(Path.GetFullPath(directoryPath));
        Console.WriteLine("==============================");
    }

    private static void HandleCurrentDirectoryRequest()
    {
        var currentDirectory = toolBox.GetCurrentDirectory();
        Console.WriteLine($"\nAssistant: You are currently in: {currentDirectory}");
        Console.WriteLine("==============================");
    }

    private static void HandlePrintCurrentDirectory()
    {
        Console.WriteLine("\nAssistant: Printing your current directory...");
        toolBox.PrintCurrentDirectory();
        Console.WriteLine("==============================");
    }
}
