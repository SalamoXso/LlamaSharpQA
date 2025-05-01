namespace LocalLLMQA.Models
{
    public class LlmModel
    {
        public required string Name { get; set; }
        public required string FilePath { get; set; }
        public string? Description { get; set; }
        public bool IsUserAdded { get; set; }

        public override string ToString() => Name;
    }
}