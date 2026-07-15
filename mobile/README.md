
FrotaGo Mobile (MAUI) - Scaffold

Este diretório contém um scaffold mínimo para a app mobile em .NET MAUI, com páginas de `Login`, `Aulas` e o fluxo básico de iniciar/terminar aulas com rastreamento GPS.

Requisitos locais:
- .NET 8+ SDK
- Workloads MAUI: `dotnet workload install maui`

Executar localmente (exemplo Android):

```bash
# instalar workloads (se necessário)
dotnet workload install maui

# compilar
dotnet build mobile/FrotaGo.Mobile.csproj -f net8.0-android

# executar (usar Visual Studio / CLI para targets específicos)
dotnet build -f net8.0-android
``` 

Testes:

```bash
# executar os testes do projecto mobile/tests
dotnet test mobile/tests/FrotaGo.Mobile.Tests
```

Notas importantes:
- A `ApiService` usa endpoints do backend (`api/auth/login`, `api/lessons`, `api/tracking/*`). Atualize `baseUrl` nas páginas para apontar para o backend real.
 - A `ApiService` usa endpoints do backend (`api/auth/login`, `api/lessons`, `api/tracking/*`). Atualize `MobileConfig.BaseUrl` em [mobile/Services/MobileConfig.cs](mobile/Services/MobileConfig.cs) para apontar para o backend real, por exemplo `https://localhost:5001/`.
- O armazenamento offline do rastreamento usa um ficheiro encriptado em `FileSystem.AppDataDirectory` e uma chave guardada em `SecureStorage`.
- Em ambiente de desenvolvimento, alguns APIs (SecureStorage, Geolocation) exigem permissões e configuração de plataforma.

Este scaffold é uma base para o MVP do app motorista/instrutor. Para integração end-to-end, configure as rotas no backend e teste em dispositivo ou emulador com GPS.
