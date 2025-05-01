using LLama;
using LLama.Common;
using System.Text;
using System.Diagnostics;

namespace LocalLLMQA.Services
{
    public class ChatService
    {
        public async Task<string> GetResponse(LLamaWeights model, string modelPath, string prompt)
        {
            try
            {
                // Diagnostic logging
                Debug.WriteLine($"Starting inference with model: {modelPath}");
                Debug.WriteLine($"Original prompt: {prompt}");

                // Simplified prompt formatting
                string formattedPrompt = $"[INST] {prompt} [/INST]";
                Debug.WriteLine($"Formatted prompt: {formattedPrompt}");

                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = 2048,  // Reduced for stability
                    GpuLayerCount = 0    // Start with CPU only
                };

                using var context = model.CreateContext(parameters);
                var executor = new InteractiveExecutor(context);

                var inferenceParams = new InferenceParams
                {
                    TokensKeep = 512,  // Reduced token count
                    AntiPrompts = new[] { "User:", "[INST]" }
                };

                var output = new StringBuilder();
                int tokenCount = 0;
                
                await foreach (var token in executor.InferAsync(formattedPrompt, inferenceParams))
                {
                    output.Append(token);
                    tokenCount++;
                    if (tokenCount > 100) break; // Safety limit
                }

                string rawResponse = output.ToString();
                Debug.WriteLine($"Raw response: {rawResponse}");

                // Basic cleaning
                string cleanedResponse = rawResponse
                    .Replace("[INST]", "")
                    .Replace("[/INST]", "")
                    .Trim();

                if (string.IsNullOrWhiteSpace(cleanedResponse))
                {
                    Debug.WriteLine("Received empty response");
                    return "Let me think about that differently...";
                }

                return cleanedResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during inference: {ex}");
                return $"I encountered an error: {ex.Message}";
            }
        }
    }
}