# Decisões de Arquitetura — Rastreamento GPS e Aplicação Mobile (.NET MAUI)

Este documento documenta o plano arquitetural para o rastreamento em tempo real do FrotaGo, o papel da aplicação mobile (.NET MAUI) para instrutores e o roadmap de evolução tecnológica.

---

## 1. Visão Geral da Arquitetura

```
                         FrotaGo System
                         
                     ┌─────────────────────┐
                     │     Angular Web     │
                     │    Painel Admin     │
                     └──────────┬──────────┘
                                │
                                │ HTTPS + SignalR (Real-time update)
                                │
                     ┌──────────▼──────────┐
                     │   ASP.NET Core API  │
                     │   Tracking Service  │
                     │      Auth JWT       │
                     │    SignalR Hub      │
                     └──────────┬──────────┘
                                │
           ┌────────────────────┴────────────────────┐
           │                                         │
   ┌───────▼────────┐                       ┌────────▼────────┐
   │    MAUI App    │                       │   GPS Device    │
   │   Instrutor    │                       │     (Futuro)    │
   │ (FrotaGo.Driver)│                       │  Teltonika, etc │
   └────────────────┘                       └─────────────────┘
```

A arquitetura do FrotaGo divide as responsabilidades por perfil de utilizador e plataforma para maximizar a eficiência e a experiência do utilizador:
* **Administrador (Angular Web Dashboard):** Consome dados em tempo real enviados via SignalR e monitoriza no mapa. Usado via computador, tablet ou browser.
* **Instrutor (Mobile App - .NET MAUI):** Aplicação dedicada instalada no dispositivo móvel do instrutor, com foco em persistência de localização em segundo plano (background).

---

## 2. Fluxo da Aplicação Mobile (FrotaGo Driver)

O fluxo principal do instrutor ao utilizar a aplicação mobile é o seguinte:

```
┌─────────────────┐     ┌───────────┐     ┌───────────────────┐     ┌──────────────────┐
│ Instrutor abre  ├────►│   Login   ├────►│ Seleciona Veículo ├────►│ Seleciona Aluno  │
│   a aplicação   │     │   (JWT)   │     │                   │     │                  │
└─────────────────┘     └───────────┘     └─────────┬─────────┘     └────────┬─────────┘
                                                    ▲                        │
                                                    │                        ▼
┌─────────────────┐     ┌───────────────────┐       │               ┌──────────────────┐
│ Admin acompanha │─────┤ Envia localização │───────┴───────────────┤  Iniciar Aula &  │
│ no mapa (Web)   │     │  (em background)  │                       │  Permitir GPS    │
└─────────────────┘     └───────────────────┘                       └──────────────────┘
```

---

## 3. Benefícios do .NET MAUI vs Browser-based GPS

| Característica | Browser GPS (V1) | .NET MAUI Mobile App (V2) |
| :--- | :--- | :--- |
| **Persistência em Background** | Cessa o envio se a página/browser for fechado ou o ecrã bloqueado. | Mantém o rastreamento ativo através de serviços nativos em segundo plano. |
| **Consumo de Bateria** | Menos otimizado, dependente do motor do browser. | Altamente otimizado utilizando APIs nativas do sistema operativo. |
| **Modo Offline** | Limitado (depende de Service Workers/Local Storage complexo). | Nativo (base de dados SQLite local com sincronização posterior). |
| **Integração de Hardware** | Acesso básico a sensores via WebAPIs. | Acesso completo e nativo a GPS, câmara, e notificações push. |

---

## 4. Estrutura do Projeto MAUI (`mobile/`)

A aplicação móvel será organizada sob o diretório `/mobile` da seguinte forma:

```
mobile/
└── FrotaGo.Driver/
    ├── Features/                  # Funcionalidades verticais (Vertical Slices)
    │   ├── Authentication/        # Login, Logout e gestão de tokens JWT
    │   ├── Tracking/              # Gestão da sessão de rastreamento e GPS
    │   ├── Lessons/               # Listagem e controlo de aulas práticas
    │   └── Vehicles/              # Seleção de veículos da escola
    ├── Services/                  # Serviços transversais da aplicação
    │   ├── LocationService.cs     # Obtenção de coordenadas nativas GPS
    │   ├── TrackingService.cs     # Controlo lógico da sessão de envio
    │   ├── SignalRService.cs      # Conexão em tempo real com o Hub da API
    │   └── ApiService.cs          # Cliente HTTP comum para a API do FrotaGo
    ├── Models/                    # Entidades e DTOs locais
    ├── ViewModels/                # Lógica de apresentação (MVVM)
    ├── Views/                     # Páginas XAML da aplicação
    ├── Resources/                 # Imagens, fontes, estilos globais
    └── Platforms/                 # Configurações específicas de plataforma
        ├── Android/               # Serviços em Background, Manifest e Permissões
        └── iOS/                   # Info.plist e Background Capabilities
```

---

## 5. Detalhes de Implementação

### 5.1. Serviço de Localização (`LocationService.cs`)
Utiliza as APIs do Essentials do .NET MAUI para aceder ao GPS de forma nativa com alta precisão:

```csharp
public class LocationService
{
    public async Task<Location?> GetLocationAsync()
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5));
            var location = await Geolocation.Default.GetLocationAsync(request);
            return location;
        }
        catch (Exception ex)
        {
            // Tratar falta de permissão ou GPS desativado
            return null;
        }
    }
}
```

### 5.2. Payload de Envio (Intervalo de 5 Segundos)
A cada intervalo de tempo parametrizável, a aplicação envia para a API o seguinte payload via HTTPS ou SignalR:

```json
{
  "latitude": -8.8368,
  "longitude": 13.2331,
  "speed": 40.5,
  "timestamp": "2026-07-15T14:04:00Z"
}
```

### 5.3. Background Tracking
A grande vantagem do MAUI é a capacidade de correr um serviço em segundo plano nativo:
* **Android:** `Foreground Service` com uma notificação persistente para evitar que o SO mate o processo.
* **iOS:** `Background Location Updates` ativado nas Capabilities do projeto.

### 5.4. Autenticação Unificada (JWT)
O MAUI reutilizará os mesmos endpoints de autenticação e os mesmos tokens JWT já implementados para o ecossistema Web (Angular):

```
[Angular Admin]  ──► [JWT Token] ──► [ASP.NET Core API]
[MAUI Driver]    ──► [JWT Token] ──► [ASP.NET Core API]
```

A base de dados (SQL Server) mantém-se unificada, rastreando através do `DeviceType` na entidade `TrackingSession` se a origem é `Mobile` (MAUI) ou `Browser` (Angular).

---

## 6. Roadmap de Evolução (Fases)

### V1 — Atual (Fase Presente)
* **Angular + Browser GPS:** Validação rápida de negócio, testes de usabilidade com escolas parceiras e validação de UX.
* **Foco:** Estabilização do Admin Dashboard e simulação/uso do rastreamento pelo browser.

### V2 — Próxima Fase (Foco Mobile)
* **MAUI Driver App:** Construção da aplicação nativa para Android/iOS do instrutor.
* **Foco:** GPS persistente em segundo plano, gestão robusta de bateria, notificações push locais e modo offline (armazenamento local de rota caso a internet móvel caia).

### V3 — Futuro (Integração de Hardware)
* **GPS Físico no Veículo:** Integração de hardware OBD-II/Teltonika direto no carro.
* **Foco:** Transmissão via protocolo de telemetria direto para a API do FrotaGo, tornando o rastreamento independente do telemóvel do instrutor.
