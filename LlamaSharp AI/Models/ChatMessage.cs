namespace LocalLLMQA.Models
{
    public class ChatMessage
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
    }
}