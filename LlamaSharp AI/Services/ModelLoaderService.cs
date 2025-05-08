using LLama;
using LLama.Common;
using LocalLLMQA.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LocalLLMQA.Services
{
    public class ModelLoaderService
    {
        private readonly IConfiguration _config;
        private readonly string _modelsDirectory;

        public ObservableCollection<LlmModel> AvailableModels { get; } = new ObservableCollection<LlmModel>();

        public ModelLoaderService(IConfiguration config)
        {
            _config = config;
            _modelsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "models");
            InitializeModels();
        }
        public bool ValidateModel(string modelPath)
        {
            try
            {
                if (!File.Exists(modelPath)) return false;
                if (!modelPath.EndsWith(".gguf", StringComparison.OrdinalIgnoreCase)) return false;

                // Quick check if file is a valid GGUF
                using var stream = File.OpenRead(modelPath);
                return stream.Length > 1024; // Basic size validation
            }
            catch
            {
                return false;
            }
        }
        private void InitializeModels()
        {
            try
            {
                // Load configured models safely
                var configuredModels = GetConfiguredModels();
                foreach (var model in configuredModels.Where(m => !string.IsNullOrEmpty(m.FilePath)))
                {
                    AvailableModels.Add(model);
                }

                // Scan for GGUF models
                ScanForGGUFModels();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing models: {ex}");
            }
        }

        public List<LlmModel> GetConfiguredModels()
        {
            var models = new List<LlmModel>();

            try
            {
                // Safely get model paths with fallback to empty string
                models.Add(new LlmModel
                {
                    Name = "Gemma 2B",
                    FilePath = _config["ModelPaths:Gemma"] ?? string.Empty,
                    IsUserAdded = false
                });

                models.Add(new LlmModel
                {
                    Name = "Granite 7B",
                    FilePath = _config["ModelPaths:Granite"] ?? string.Empty,
                    IsUserAdded = false
                });

                models.Add(new LlmModel
                {
                    Name = "Llama 3 8B",
                    FilePath = _config["ModelPaths:Llama3"] ?? string.Empty,
                    IsUserAdded = false
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error getting configured models: {ex}");
            }

            return models.Where(m => !string.IsNullOrEmpty(m.FilePath)).ToList();
        }

        public string? GetModelPath(string modelName)
        {
            try
            {
                var path = _config[$"ModelPaths:{modelName}"];
                return string.IsNullOrWhiteSpace(path) ? null : path;
            }
            catch
            {
                return null;
            }
        }

        public void ScanForGGUFModels()
        {
            try
            {
                if (!Directory.Exists(_modelsDirectory))
                {
                    Directory.CreateDirectory(_modelsDirectory);
                    return;
                }

                var existingPaths = AvailableModels.Select(m => m.FilePath).ToList();

                foreach (var file in Directory.GetFiles(_modelsDirectory, "*.gguf"))
                {
                    if (!existingPaths.Contains(file, StringComparer.OrdinalIgnoreCase))
                    {
                        AvailableModels.Add(new LlmModel
                        {
                            Name = Path.GetFileNameWithoutExtension(file),
                            FilePath = file,
                            IsUserAdded = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error scanning for GGUF models: {ex}");
            }
        }

        public string? AddModelManually()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "GGUF Model Files (*.gguf)|*.gguf",
                    InitialDirectory = _modelsDirectory,
                    Title = "Select GGUF Model File"
                };

                return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding model manually: {ex}");
                return null;
            }
        }

        public LLamaWeights? LoadModel(string modelPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(modelPath) || !File.Exists(modelPath))
                {
                    Debug.WriteLine("Model path is invalid or file does not exist.");
                    return null;
                }

                if (!modelPath.EndsWith(".gguf", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.WriteLine("Unsupported model file format.");
                    return null;
                }

                var parameters = new ModelParams(modelPath)
                {
                    ContextSize = 2048,
                    GpuLayerCount = 0
                };

                return LLamaWeights.LoadFromFile(parameters);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load model: {ex.Message}");
                return null;
            }
        }

    }
}