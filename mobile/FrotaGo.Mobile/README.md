
# FrotaGo Mobile (MAUI)

App móvel em .NET MAUI para instrutor/motorista com fluxo de login, aulas agendadas e rastreamento GPS em tempo real.

## 🎯 Características

- ✅ **Autenticação JWT** via backend
- ✅ **Lista de Aulas Agendadas** — sincronizadas com backend
- ✅ **Rastreamento GPS** — captura contínua de localização com offline resilience
- ✅ **Armazenamento Offline Encriptado** — cache de localizações mesmo sem internet
- ✅ **Design System** — cores e estilos alinhados com webapp (Material Design)
- ✅ **Testes Unitários** — 2/2 passing (AuthService, ApiService)

## 📋 Requisitos

- .NET 8+ SDK
- Workload MAUI instalado
- (Windows) Visual Studio 2022 ou posterior para compilação Android/iOS
- (macOS) Xcode 14+ para iOS
- ngrok para testar contra backend local

## 🚀 Executar Localmente

### Instalar Workload MAUI

```bash
dotnet workload install maui
```

### Compilar para Android

```bash
cd /home/sgonga/Transferências/FrotaGo
dotnet build mobile/FrotaGo.Mobile.csproj -f net8.0-android -c Release
```

### Compilar para iOS (macOS apenas)

```bash
dotnet build mobile/FrotaGo.Mobile.csproj -f net8.0-ios -c Release
```

### Executar Testes

```bash
dotnet test mobile/tests/FrotaGo.Mobile.Tests --logger "console;verbosity=minimal"
```

## 🎨 Design System

O design da app segue Material Design 3 e está alinhado com o webapp Angular.

**Paleta de Cores:**
- Primária: `#2196F3` (Azul)
- Primária Escura: `#1976D2`
- Sucesso: `#4CAF50` (Verde)
- Erro: `#F44336` (Vermelho)
- Aviso: `#FF9800` (Laranja)
- Fundo: `#FFFFFF` (Branco)
- Superfície: `#F5F5F5` (Cinzento Claro)

Para mais detalhes, consulte [docs/DESIGN_SYSTEM_MOBILE.md](../docs/DESIGN_SYSTEM_MOBILE.md).

## 🔧 Configuração

### Backend URL

Atualize `MobileConfig.BaseUrl` em `mobile/Services/MobileConfig.cs`:

```csharp
public static class MobileConfig
{
    public static string BaseUrl = "https://9be3-105-174-52-229.ngrok-free.app"; // ngrok URL
}
```

### Permissões (Android)

Adicione no `AndroidManifest.xml`:
```xml
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-permission android:name="android.permission.INTERNET" />
```

### Permissões (iOS)

Adicione no `Info.plist`:
```xml
<key>NSLocationWhenInUseUsageDescription</key>
<string>Necessário para rastreamento de aulas</string>
```

## 📁 Estrutura

```
mobile/
├── App.xaml                    # Estilos globais (cores, buttons, inputs)
├── App.xaml.cs
├── Views/
│   ├── LoginPage.xaml          # Autenticação
│   ├── LessonListPage.xaml     # Lista de aulas agendadas
│   └── LessonDetailPage.xaml   # Detalhe e controlo de aula
├── Services/
│   ├── AuthService.cs          # JWT, login, logout
│   ├── ApiService.cs           # Integração backend (lessons, tracking)
│   ├── TrackingService.cs      # Capture GPS, offline cache, sync
│   ├── EncryptionService.cs    # AES-GCM para cache local
│   └── MobileConfig.cs         # Configurações globais
├── Models/
│   ├── Lesson.cs               # Data model de aula
│   └── TrackingSession.cs      # Data model de rastreamento
└── tests/
    └── FrotaGo.Mobile.Tests/   # Testes unitários
```

## 🔐 Segurança

- **JWT Tokens:** Armazenados em `SecureStorage` (Keychain/Keystore)
- **Cache Offline:** Encriptado com AES-GCM antes de ser armazenado em ficheiro
- **HTTPS Only:** Todas as requisições usam HTTPS (ngrok fornece certificado válido)

## 🧪 Testes

### Executar Todos os Testes

```bash
dotnet test mobile/tests/FrotaGo.Mobile.Tests
```

### Testes de Integração E2E

Consulte [docs/TEST_PLAN.md](../docs/TEST_PLAN.md) para:
- E2E-001: Login Completo
- E2E-002: Listar Aulas
- E2E-003: Iniciar Aula com Rastreamento
- E2E-004: Parar Aula e Sincronizar
- E2E-005: Offline Resilience

## 📱 Deploying

### Android APK

```bash
dotnet publish mobile/FrotaGo.Mobile.csproj -f net8.0-android -c Release
# APK será gerado em: bin/Release/net8.0-android/com.frotago.mobile.apk
```

### iOS App

```bash
dotnet publish mobile/FrotaGo.Mobile.csproj -f net8.0-ios -c Release
# App será gerado em: bin/Release/net8.0-ios/
```

## 🐛 Troubleshooting

### Erro: "Failed to load API definition"
- Verificar Swagger em http://127.0.0.1:5073/swagger/index.html
- Reiniciar backend: `dotnet run --project backend/...`

### Erro: "Connection refused"
- Verificar se ngrok está ativo
- Atualizar URL em `MobileConfig.BaseUrl`
- Verificar firewall/proxy

### Erro: "Permission denied" (GPS)
- Conceder permissões no emulador/dispositivo
- Android: Settings → Apps → Permissions
- iOS: Settings → App → Location

## 📚 Referências

- [MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Design System](../docs/DESIGN_SYSTEM_MOBILE.md)
- [Test Plan](../docs/TEST_PLAN.md)
- [Backend API](../backend/FrotaGo.Backend/src/FrotaGo.Api/FrotaGo.Api.http)

---

**Próximo passo:** Testar no emulador/dispositivo com backend local via ngrok.

