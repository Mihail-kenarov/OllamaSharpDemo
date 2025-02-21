
namespace OllamaSharpSoloDemo.Tools
{
    internal class LlmFileModel
    {
        public string Filename { get; set; }
        public long Size { get; set; }
        public DateTime LastModified { get; internal set; }
        public bool IsFile { get; internal set; }
        public bool IsDirectory { get; internal set; }
        public List<LlmFileModel> Contents { get; set; }
    }
}