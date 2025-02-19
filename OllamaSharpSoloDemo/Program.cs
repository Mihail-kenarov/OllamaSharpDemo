using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharpSoloDemo;
using OllamaSharpSoloDemo.Tools;


class Program
{
    private static Uri uri;
    private static OllamaApiClient ollama;
    private static string message;
    private static List<string> exitWords = new List<string> { "bye", "goodbye", "chao" };
    private static ToolBox toolBox = new ToolBox(new ToolDirectoryComponents());

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
        string selectedModel;
        bool isValidModel;

        do
        {
            Console.WriteLine("Write the name of the model that you would like to use:");
            selectedModel = Console.ReadLine();

            isValidModel = models.Contains(selectedModel, StringComparer.OrdinalIgnoreCase);

            if (!isValidModel)
            {
                Console.WriteLine("\nInvalid model. Please select one from the list above.\n");
            }

        } while (!isValidModel);

        ollama.SelectedModel = selectedModel;
        Console.WriteLine($"\nYou have selected to work with {ollama.SelectedModel}\n");
        Console.WriteLine("Feel free to start your chat!\n");
    }

    private static async Task StartChat()
    {
        var chat = new Chat(ollama);

        do
        {
            Console.Write("User: ");
            message = Console.ReadLine();

            if (exitWords.Contains(message.ToLower()))
            {
                Console.WriteLine("Assistant: Goodbye!");
                break;
            }
            else if (message.StartsWith("show directory:", StringComparison.OrdinalIgnoreCase))
            {
                HandleDirectoryRequest(message);
            }
            else if (message.Equals("where am I?", StringComparison.OrdinalIgnoreCase))
            {
                HandleCurrentDirectoryRequest();
            }
            else if (message.Equals("print my current directory", StringComparison.OrdinalIgnoreCase))
            {
                HandlePrintCurrentDirectory();
            }
            else
            {
                Console.Write("Assistant: ");
                await foreach (var answerToken in chat.SendAsync(message))
                {
                    Console.Write(answerToken);
                }
                Console.WriteLine("\n==============================");
            }
        } while (!string.IsNullOrEmpty(message));
    }

    private static void HandleDirectoryRequest(string message)
    {

        string[] words = message.Split(' ');
        if (words.Length > 6) 
        {
            string directoryPath = message.Substring(message.IndexOf("directory") + 9).Trim();

            Console.WriteLine($"\nAssistant: Checking directory {directoryPath}...");
            toolBox.OpenDirectory(directoryPath);
            Console.WriteLine("==============================");
        }
        else
        {
            Console.WriteLine("\nAssistant: Please provide a valid directory path.");
        }


    }

    private static void HandleCurrentDirectoryRequest()
    {
        string currentDirectory = toolBox.GetCurrentDirectory();
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
