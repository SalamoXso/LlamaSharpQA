using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LocalLLMQA.Models
{
    public class ModelResponse
    {
        [JsonPropertyName("models")]
        public List<LlmModel> Models { get; set; } = new List<LlmModel>();
    }

}