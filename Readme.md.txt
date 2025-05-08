# 🧠 Local LLM QA Application

## 📘 1. Technical Documentation

### 1.1 Overview
The **Local LLM QA** application allows users to interact with **locally hosted Large Language Models (LLMs)** in a question-answering format. It supports loading GGUF model files, processing user queries, and displaying responses in a clean UI.

---

### 1.2 Key Features

- **Model Management:** Load pre-configured or custom GGUF models.  
- **Question Answering:** Submit questions and receive AI-generated responses.  
- **Response Handling:** Copy answers, clear inputs, and view processing status.  
- **Error Handling:** Graceful handling of model loading and inference errors.  

---

### 1.3 Architecture

The application follows the **MVVM (Model-View-ViewModel)** pattern:

| Component   | Description                                       |
|-------------|---------------------------------------------------|
| **Models**  | Data structures (`LlmModel`, `ChatMessage`, `ModelResponse`) |
| **Views**   | XAML-based UI (`MainWindow.xaml`)                |
| **ViewModels** | Business logic (`MainViewModel.cs`)           |
| **Services**   | Backend operations (`ChatService.cs`, `ModelLoaderService.cs`) |
| **Converters** | UI logic (`BooleanToVisibilityConverter`, `StringToVisibilityConverter`) |

---

### 1.4 File Descriptions

#### Core Files

| File               | Purpose                                                  |
|--------------------|----------------------------------------------------------|
| `MainWindow.xaml`  | Main UI: model selection, input, and output display      |
| `MainViewModel.cs` | Handles UI interaction, model loading, question processing |
| `ChatService.cs`   | Manages LLM inference and response generation            |
| `ModelLoaderService.cs` | Loads and manages GGUF model files              |
| `LlmModel.cs`      | Defines model attributes (`Name`, `FilePath`, `IsUserAdded`) |
| `ChatMessage.cs`   | Stores messages and roles (`user` / `assistant`)         |
| `appsettings.json` | Stores default model paths (Gemma, Granite, Llama3)      |

#### Supporting Files

| File                  | Purpose                                 |
|-----------------------|-----------------------------------------|
| `VisibilityConverters.cs` | UI visibility control via converters |
| `Converters.xaml`     | Resource dictionary for converters      |
| `App.xaml.cs`         | App entry point and exception handling  |

---

## ⚙️ 2. Setup Instructions

### 2.1 Prerequisites

- [.NET 6.0+](https://dotnet.microsoft.com/) (or compatible runtime)  
- GGUF Model Files (e.g., `gemma-2b.gguf`, `llama3-8b.gguf`)

---

### 2.2 Model Configuration

#### Default Models (Pre-Configured)

Edit the `appsettings.json` file:

```json
{
  "ModelPaths": {
    "Gemma": "C:\\models\\gemma-2b.gguf",
    "Granite": "C:\\models\\granite-7b.gguf",
    "Llama3": "C:\\models\\llama3-8b.gguf"
  }
}
```

> If paths are invalid or missing, they will be ignored gracefully.

#### Custom Models (Manual Addition)

- Click **"Add Model"** in the UI.
- Browse and select a `.gguf` model file.
- The model will be copied to the `./models/` directory.

---

### 2.3 Running the Application

#### 🔧 Build & Launch

- Compile and run the application.
- It auto-loads models from `appsettings.json` and `./models/`.

#### 🚀 Usage Workflow

1. Select a model from the dropdown.
2. Type a question in the input box.
3. Click **"Ask Question"** or press **Ctrl+Enter**.
4. View the AI-generated answer.
5. Use **"Copy"** or **"Clear"** as needed.

---

## 🛠️ 3. Troubleshooting

| Issue                        | Solution                                                                 |
|-----------------------------|--------------------------------------------------------------------------|
| Model not loading            | Check file existence and path in `appsettings.json`.                     |
| Short/truncated answers      | Increase `maxTokens` in `ChatService.cs`.                                |
| UI freezing during processing| Adjust `GpuLayerCount` and model compatibility.                          |
| "Add Model" not working      | Check permissions and ensure `.gguf` file is selected.                   |

---

## 🧳 4. Migration Guide (For QE Team)

### 📁 Files to Migrate

| File Type    | Files                         | Destination             |
|--------------|-------------------------------|--------------------------|
| Executable   | `LocalLLMQA.exe`              | Target deployment folder |
| Config       | `appsettings.json`            | `/Config/` (editable)    |
| Models       | `*.gguf`                      | `/models/` or custom path|
| Dependencies | `LLamaSharp.dll` (if needed)  | `/Libs/`                 |

---

### ✅ Post-Migration Checks

- Confirm paths in `appsettings.json` match the target environment.
- Test model loading via UI and API (if used).
- Verify question/response flow without errors.

---

## 📝 Final Notes

- The application is **offline-first** and does **not require an internet connection**.
- For performance tuning:
  - Adjust `ContextSize` and `GpuLayerCount` in `ModelLoaderService.cs`.

---
