# Arquitetura Móvel Cross-Platform — Estratégia de Desenvolvimento (Debian Linux / Windows)

Este documento especifica a estratégia de desenvolvimento cross-platform da aplicação móvel do **FrotaGo** para instrutores.

---

## 1. O Desafio do Ambiente Heterogéneo

* **Debian Linux:** Ambiente principal de desenvolvimento para o Backend (ASP.NET Core) e Frontend Web (Angular). O SDK/Workload do .NET MAUI não está tipicamente instalado ou suportado nativamente para empacotamento de UI móvel (Android/iOS).
* **Windows (VS Code / Visual Studio):** Ambiente onde o SDK do .NET MAUI e simuladores Android/iOS estão configurados para compilação visual e testes nativos.

---

## 2. Solução Arquitetural: Divisão em `Core` e `UI`

Para maximizar a produtividade e permitir que **80–90% do código móvel seja escrito e testado no Linux**, o ecossistema móvel é estruturado em duas bibliotecas sob a pasta `mobile/`:

```
FrotaGo/
├── mobile/
│   ├── FrotaGo.Mobile.Core/      ← Biblioteca C# Standard (.net8.0) — DESENVOLVIDO NO LINUX
│   │   ├── DTOs/                 # Objetos de transferência de dados (API REST)
│   │   ├── Enums/                # Enumerações de domínio (LessonStatus, VehicleStatus, etc.)
│   │   ├── Helpers/              # Utilitários e constantes de endpoints
│   │   ├── Interfaces/           # Contratos de serviços (IAuthService, ITrackingService, IGpsService)
│   │   ├── Models/               # Entidades e modelos de domínio móvel
│   │   ├── Services/             # Lógica de negócio, clientes HTTP e clientes SignalR
│   │   └── ViewModels/           # Lógica de apresentação MVVM (CommunityToolkit.Mvvm)
│   │
│   └── FrotaGo.Mobile/           ← Projeto .NET MAUI (.net8.0-android / ios) — DESENVOLVIDO NO WINDOWS
│       ├── App.xaml / App.xaml.cs
│       ├── MauiProgram.cs        # Injeção de dependências e configuração do app
│       ├── Views/                # Interfaces gráficas XAML (LoginPage, DashboardPage, etc.)
│       ├── Resources/            # Estilos, ícones e fontes nativas
│       └── Platforms/            # Código nativo (Services em background Android, Capabilities iOS)
```

---

## 3. Matriz de Responsabilidades por Ambiente

| Componente | Linguagem / Framework | Pode ser compilado/testado no Linux? | Onde é desenvolvido? |
| :--- | :--- | :---: | :--- |
| **`FrotaGo.Mobile.Core`** | C# (.net8.0 Standard) | ✅ Sim (`dotnet build` / `dotnet test`) | **Debian Linux** (ou Windows) |
| **Modelos & DTOs** | C# PURO | ✅ Sim | **Debian Linux** |
| **Serviços HTTP & SignalR** | C# + System.Text.Json / SignalR.Client | ✅ Sim | **Debian Linux** |
| **ViewModels (MVVM)** | C# + CommunityToolkit.Mvvm | ✅ Sim | **Debian Linux** |
| **Páginas XAML & Views** | XAML / .NET MAUI | ❌ Não (Requer MAUI SDK) | **Windows** |
| **Serviços Nativo GPS (Background)** | Android Foreground Service / iOS Location | ❌ Não | **Windows** |

---

## 4. Workflow de Trabalho

1. **Desenvolvimento no Linux (Debian):**
   - Criação de novos serviços móveis, ViewModels, consumo de endpoints REST da API e hubs SignalR.
   - Validação contínua através de `dotnet build mobile/FrotaGo.Mobile.Core/FrotaGo.Mobile.Core.csproj` e testes unitários.

2. **Transição para o Windows:**
   - Abrir o repositório no VS Code / Visual Studio no Windows.
   - Desenhar as Views em XAML ligando os bindings (`BindingContext`) às `ViewModels` criadas no `.Core`.
   - Testar a aplicação no Android Emulator ou dispositivo físico.

---

## 5. Injeção de Dependências (Exemplo de Configuração no Windows)

No `MauiProgram.cs` (em `FrotaGo.Mobile`):

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        // Registar Serviços do FrotaGo.Mobile.Core
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<ITrackingService, TrackingService>();
        builder.Services.AddSingleton<ILessonService, LessonService>();

        // Registar ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();

        // Registar Views (MAUI)
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<DashboardPage>();

        return builder.Build();
    }
}
```
