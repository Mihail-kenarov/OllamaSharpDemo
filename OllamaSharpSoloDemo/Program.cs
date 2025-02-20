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
    private static readonly HashSet<string> exitWords = new HashSet<string> { "bye", "goodbye", "chao", "exit" };

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

        var tools = new List<DwmTool>
        {
            new DirectoryTool(),
            new ReadFileTool()
        };

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

                case "/list":
                    break;

                case "/clear":
                    chat.Messages.RemoveAll(x => x.Role != ChatRole.System);
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

                // calling ollama
                try
                {
                    await foreach (var answerToken in chat.SendAsync(message, tools))
                        Console.WriteLine(answerToken);
                }
                catch (OllamaException ex)
                {
                    //                AnsiConsole.MarkupLineInterpolated($"[{ErrorTextColor}]{ex.Message}[/]");
                }

                var toolCalls = chat.Messages.LastOrDefault()?.ToolCalls?.ToArray() ?? [];
                if (toolCalls.Any())
                {
                    Console.WriteLine("Tools used:");
                    foreach (var function in toolCalls
                        .Where(t => t.Function != null)
                        .Select(t => t.Function))
                    {
                        Console.WriteLine($"  -{function.Name}");
                        Console.WriteLine($"    - parameter");

                        if (function.Arguments is not null)
                        {
                            foreach (var argument in function.Arguments)
                                Console.WriteLine($"      - [purple]{argument.Key}[/]: value :{argument.Value}");
                        }

                        // find tool
                        var tool = tools.FirstOrDefault(t => t.Name == function.Name);
                        var response = tool == null
                            ? "tool not found"
                            : await tool.ExecuteAsync(function.Arguments);

                        Console.WriteLine($"    - return value: \"{response}\"");

                        await foreach (var answerToken in chat.SendAsAsync(ChatRole.Tool, response, tools))
                            Console.WriteLine(answerToken);
                    }
                }
            }
        }
    }
}
