using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LLama;
using LocalLLMQA.Models;
using LocalLLMQA.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Linq;
using System.IO;
using System.Windows.Threading;

namespace LocalLLMQA.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ModelLoaderService _modelLoader;
        private readonly ChatService _chatService;
        private LLamaWeights? _currentModel;
        private string? _currentModelPath;
        private CancellationTokenSource _loadModelsCts = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AskQuestionCommand))]
        private string _question = string.Empty;

        [ObservableProperty]
        private string _answer = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AskQuestionCommand))]
        private LlmModel? _selectedModel;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AskQuestionCommand))]
        private bool _isProcessing;

        [ObservableProperty]
        private bool _isLoadingModels;

        public ObservableCollection<LlmModel> AvailableModels => _modelLoader.AvailableModels;

        public MainViewModel()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            _modelLoader = new ModelLoaderService(config);
            _chatService = new ChatService();

            _ = LoadAvailableModelsAsync();

            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Question) ||
                   e.PropertyName == nameof(SelectedModel) ||
                   e.PropertyName == nameof(IsProcessing))
                {
                    AskQuestionCommand.NotifyCanExecuteChanged();
                }
            };
        }

        private async Task LoadAvailableModelsAsync()
        {
            try
            {
                IsLoadingModels = true;
                _modelLoader.AvailableModels.Clear();

                // Load both configured and discovered models
                var configuredModels = await Task.Run(() => _modelLoader.GetConfiguredModels());
                foreach (var model in configuredModels)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _modelLoader.AvailableModels.Add(model);
                    });
                }

                await Task.Run(() => _modelLoader.ScanForGGUFModels());

                Application.Current.Dispatcher.Invoke(() =>
                {
                    SelectedModel = _modelLoader.AvailableModels.FirstOrDefault();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading models: {ex}");
                Answer = $"Error loading models: {ex.Message}";
            }
            finally
            {
                IsLoadingModels = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanAskQuestion))]
        private async Task AskQuestion()
        {
            if (!CanAskQuestion() || SelectedModel == null) return;

            try
            {
                IsProcessing = true;
                Answer = "Processing...";
                CommandManager.InvalidateRequerySuggested();

                // Store current model path to prevent race conditions
                var currentModelPath = SelectedModel.FilePath;
                var userQuestion = Question;
                Question = string.Empty;

                _currentModelPath = currentModelPath;
                _currentModel = await Task.Run(() => _modelLoader.LoadModel(currentModelPath));

                // Verify we're still using the same model
                if (SelectedModel?.FilePath != currentModelPath)
                {
                    Answer = "Model changed during processing. Please try again.";
                    return;
                }

                var response = await _chatService.GetResponse(
                    _currentModel,
                    currentModelPath,
                    userQuestion);

                Answer = response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during inference: {ex}");
                Answer = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool CanAskQuestion()
        {
            return !IsProcessing &&
                   !string.IsNullOrWhiteSpace(Question) &&
                   SelectedModel != null;
        }

        [RelayCommand]
        private void CancelLoading()
        {
            _loadModelsCts.Cancel();
            _loadModelsCts = new CancellationTokenSource();
        }

        [RelayCommand]
        private async Task AddModel()
        {
            try
            {
                IsLoadingModels = true;
                var modelPath = await Task.Run(() => _modelLoader.AddModelManually());

                if (!string.IsNullOrEmpty(modelPath))
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var existingModel = _modelLoader.AvailableModels
                            .FirstOrDefault(m => m.FilePath.Equals(modelPath, StringComparison.OrdinalIgnoreCase));

                        if (existingModel == null)
                        {
                            var newModel = new LlmModel
                            {
                                Name = Path.GetFileNameWithoutExtension(modelPath),
                                FilePath = modelPath,
                                IsUserAdded = true
                            };

                            _modelLoader.AvailableModels.Add(newModel);
                            SelectedModel = newModel;
                            Answer = $"Model added: {newModel.Name}";
                        }
                        else
                        {
                            SelectedModel = existingModel;
                            Answer = $"Model already exists: {existingModel.Name}";
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error adding model: {ex}");
                Answer = $"Error adding model: {ex.Message}";
            }
            finally
            {
                IsLoadingModels = false;
            }
        }

        [RelayCommand]
        private void ClearQuestion()
        {
            Question = string.Empty;
        }

        [RelayCommand]
        private void CopyAnswer()
        {
            if (!string.IsNullOrWhiteSpace(Answer))
            {
                Clipboard.SetText(Answer);
            }
        }
    }
}