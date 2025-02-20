using OllamaSharp.Models.Chat;

namespace OllamaSharpSoloDemo.Support
{
    public abstract class DwmTool : Tool
    {
        public abstract string Name { get; }

        public abstract Task<string> ExecuteAsync(IDictionary<string, object> namedParameters);
    }
}
