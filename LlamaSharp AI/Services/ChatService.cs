using LLama;
using LLama.Common;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace LocalLLMQA.Services
{
    public class ChatService
    {
        public async Task<string> GetResponse(LLamaWeights model, string modelPath, string prompt, CancellationToken cancellationToken = default)
        {
            // Check for null model before attempting inference
            if (model == null)
            {
                Debug.WriteLine("Model is null. Aborting inference.");
                return "Model is not loaded properly. Please ensure a valid model is selected.";
            }

            try
            {
                // Check cancellation before starting
                cancellationToken.ThrowIfCancellationRequested();

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
                const int maxTokens = 512;

                await foreach (var token in executor.InferAsync(formattedPrompt, inferenceParams, cancellationToken))
                {
                    // Check for cancellation more aggressively
                    cancellationToken.ThrowIfCancellationRequested();

                    output.Append(token);
                    tokenCount++;
                    if (tokenCount >= maxTokens) break;

                    // Add small delay to allow cancellation to be processed
                    await Task.Delay(1, cancellationToken);
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
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Inference was successfully cancelled");
                throw; // Re-throw to let the caller handle it
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during inference: {ex}");
                return $"I encountered an error: {ex.Message}";
            }
        }
    }
}
