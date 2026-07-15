# Plano de Teste — FrotaGo Mobile & Backend

**Data:** 15 de julho de 2026  
**Versão:** 1.0  
**Escopo:** MVP Instrutor/Motorista — Login, Aulas, Rastreamento GPS  

---

## 1. Estrutura de Testes

### 1.1 Níveis de Teste

| Nível | Escopo | Ferramenta | Status |
|-------|--------|-----------|--------|
| **Unitário** | Lógica isolada (services, handlers) | xUnit (.NET), xUnit (MAUI) | ✅ Em execução |
| **Integração** | Backend API + BD | xUnit + TestServer | ✅ Em execução |
| **E2E** | App Mobile + Backend (via ngrok) | Manual + Appium (futuro) | 🔄 Em andamento |
| **Manual** | Fluxos de utilizador reais | Emulador/Dispositivo | 🔄 Em andamento |

---

## 2. Testes Unitários

### 2.1 Backend (.NET)

**Projeto:** `FrotaGo.Application.Tests`

#### Casos de teste — Autenticação

| ID | Descrição | Input | Esperado | Status |
|----|-----------|-------|----------|--------|
| AUTH-001 | Login com credenciais válidas | Email: `teste.mobile@frotago.com`, Password: `123456` | Retorna JWT válido | ✅ |
| AUTH-002 | Login com email inválido | Email: `invalid@test.com`, Password: `123456` | Retorna 401 Unauthorized | ⏳ |
| AUTH-003 | Login com password vazia | Email: `teste@frotago.com`, Password: `` | Retorna 400 Bad Request | ⏳ |
| AUTH-004 | Registro com novo utilizador | Email: `novo@frotago.com`, Password: `Sec@re123` | Retorna novo utilizador com ID | ⏳ |

#### Casos de teste — Aulas (Lessons)

| ID | Descrição | Input | Esperado | Status |
|----|-----------|-------|----------|--------|
| LESSON-001 | Listar aulas do instrutor autenticado | JWT válido | Retorna lista com 2+ aulas | ✅ |
| LESSON-002 | Iniciar aula válida | LessonId: `6864b051-...`, VehicleId: `25ff2f97-...` | Cria TrackingSession com status `Starting` | ⏳ |
| LESSON-003 | Iniciar aula inexistente | LessonId: `invalid-guid` | Retorna 404 Not Found | ⏳ |
| LESSON-004 | Parar aula ativa | TrackingSessionId: `<valid>` | Atualiza status para `Stopped` e define `EndedAt` | ⏳ |

#### Casos de teste — Rastreamento (Tracking)

| ID | Descrição | Input | Esperado | Status |
|----|-----------|-------|----------|--------|
| TRACK-001 | Enviar localização válida | Lat: `-8.8383`, Lng: `13.2344`, Speed: `45.5` | Cria `VehicleLocation` e retorna 200 | ⏳ |
| TRACK-002 | Enviar localização para sessão inexistente | TrackingSessionId: `invalid` | Retorna 404 Not Found | ⏳ |
| TRACK-003 | Enviar localização para sessão stopped | Sessão com status `Stopped` | Retorna 400 Bad Request | ⏳ |
| TRACK-004 | Atualizar status de sessão para `Active` na primeira localização | Status inicial: `Starting` | Status muda para `Active` | ⏳ |

### 2.2 App Móvel (MAUI)

**Projeto:** `mobile/tests/FrotaGo.Mobile.Tests`

#### Casos de teste — AuthService

| ID | Descrição | Input | Esperado | Status |
|----|-----------|-------|----------|--------|
| MAUTH-001 | Login com credenciais válidas | Email, Password | Armazena JWT em SecureStorage | ✅ |
| MAUTH-002 | Login falha com credenciais inválidas | Email inválido, Password errada | Lança Exception com mensagem de erro | ⏳ |
| MAUTH-003 | Recuperar token armazenado | JWT previamente armazenado | Retorna token válido | ⏳ |

#### Casos de teste — ApiService

| ID | Descrição | Input | Esperado | Status |
|----|-----------|-------|----------|--------|
| MAPI-001 | Buscar aulas com JWT | Token válido | Retorna lista de aulas | ✅ |
| MAPI-002 | Iniciar rastreamento | VehicleId, InstructorId, LessonId | Retorna TrackingSession com ID | ⏳ |
| MAPI-003 | Enviar localização para backend | Lat, Lng, Speed | Retorna 200 OK | ⏳ |
| MAPI-004 | Parar rastreamento | TrackingSessionId | Retorna status Stopped | ⏳ |

#### Casos de teste — TrackingService

| ID | Descrição | Input | Esperado | Status |
|----|-----------|-------|----------|--------|
| MTRACK-001 | Capturar localização GPS | GPS ativo | Armazena (Lat, Lng, Speed) | ⏳ |
| MTRACK-002 | Armazenar offline quando sem rede | GPS válido, sem internet | Encripta e armazena em ficheiro local | ⏳ |
| MTRACK-003 | Sincronizar cache offline | Cache encriptado, internet disponível | Envia todas as localizações ao backend | ⏳ |
| MTRACK-004 | Manusear erro de rede graciosamente | Envio falha | Mantém cache e tenta novamente | ⏳ |

---

## 3. Testes de Integração

### 3.1 Backend + Base de Dados

**Comando:**
```bash
dotnet test backend/FrotaGo.Backend/tests/FrotaGo.Application.Tests --logger "console;verbosity=normal"
```

#### Cenários

| Cenário | Passos | Esperado | Status |
|---------|--------|----------|--------|
| **Login → Aulas → Iniciar Tracking** | 1. POST `/api/auth/login`<br>2. GET `/api/lessons` com JWT<br>3. POST `/api/tracking/start` | 1. JWT retornado<br>2. Lista com aulas<br>3. TrackingSession criada | ⏳ |
| **Rastreamento com múltiplas localizações** | 1. POST `/api/tracking/start`<br>2. POST `/api/tracking/location` (5x)<br>3. POST `/api/tracking/stop` | 5 VehicleLocation registadas, sessão finalizada | ⏳ |
| **Isolamento de dados por Tenant (Escola)** | Dois utilizadores de escolas diferentes | Cada um vê apenas seus dados | ⏳ |

---

## 4. Testes End-to-End (E2E) — App Mobile + Backend

### 4.1 Configuração do Ambiente

**Pre-requisitos:**
- Backend rodando em `http://127.0.0.1:5073`
- ngrok expondo em `https://9be3-105-174-52-229.ngrok-free.app`
- App móvel compilada com MobileConfig apontando para ngrok URL
- Emulador Android/iOS com GPS simulado ou dispositivo físico

### 4.2 Fluxo de Teste — Login até Parar Aula

#### Teste E2E-001: Login Completo

**Objetivo:** Validar autenticação de ponta a ponta

| Passo | Ação | Esperado |
|-------|------|----------|
| 1 | Abrir app → Tela de Login | Login form visível |
| 2 | Inserir email: `teste.mobile@frotago.com` | Campo preenchido |
| 3 | Inserir password: `123456` | Campo preenchido (masked) |
| 4 | Tap "Entrar" | Spinner de carregamento aparece |
| 5 | Aguardar resposta do backend via ngrok | JWT recebido e armazenado |
| 6 | Verificar SecureStorage | Token JWT presente e válido |
| 7 | Navigationnavega para LessonListPage | Lista de aulas visível |

**Resultado esperado:** ✅ Utilizador autenticado, lista de aulas carregada

---

#### Teste E2E-002: Listar Aulas Agendadas

**Objetivo:** Validar recuperação de aulas do backend

| Passo | Ação | Esperado |
|-------|------|----------|
| 1 | Estar na LessonListPage | Lista carregada com aulas |
| 2 | Verificar número de aulas | Mínimo 2 aulas visíveis |
| 3 | Verificar dados de cada aula | Student, Vehicle, Date, Topic visíveis |
| 4 | Tap em uma aula | Navega para LessonDetailPage |
| 5 | Verificar detalhe da aula | Todos os campos da aula (aluno, viatura, instrutor, etc.) |

**Resultado esperado:** ✅ Aulas listadas e detalhe acessível

---

#### Teste E2E-003: Iniciar Aula com Rastreamento

**Objetivo:** Validar fluxo completo de início de aula e tracking

| Passo | Ação | Esperado |
|-------|------|----------|
| 1 | Na LessonDetailPage, tap "Iniciar Aula" | Modal de confirmação aparece |
| 2 | Verificar dados: Aluno, Viatura, Instrutor | Dados corretos do backend |
| 3 | Tap "Confirmar" | Requisição POST `/api/tracking/start` enviada |
| 4 | Aguardar resposta | TrackingSession recebida com ID |
| 5 | Verificar estado da UI | Muda para "Aula em curso" com botão "Parar Aula" |
| 6 | Permitir acesso ao GPS | Permissões concedidas (simulado no emulador) |
| 7 | Verificar se GPS está ativo | Localizações começam a ser capturadas |
| 8 | Aguardar 3-5 localizações (5 segundos) | Localizações armazenadas offline (encriptadas) |

**Resultado esperado:** ✅ Aula iniciada, tracking ativo, localizações capturadas

---

#### Teste E2E-004: Parar Aula e Sincronizar Rastreamento

**Objetivo:** Validar finalização de aula e sincronização de dados

| Passo | Ação | Esperado |
|-------|------|----------|
| 1 | Durante aula ativa, tap "Parar Aula" | Modal de confirmação |
| 2 | Tap "Confirmar Parada" | POST `/api/tracking/stop` enviado |
| 3 | Aguardar resposta | TrackingSession finalizada (status: Stopped) |
| 4 | Verificar se localizações são sincronizadas | POST `/api/tracking/location` (múltiplos) enviados |
| 5 | Verificar cache offline | Cache limpado após sincronização |
| 6 | Voltar a LessonListPage | Aula já não aparece (se foi finalizada no backend) |

**Resultado esperado:** ✅ Aula finalizada, dados sincronizados, cache limpo

---

#### Teste E2E-005: Offline Resilience

**Objetivo:** Validar comportamento sem internet

| Passo | Ação | Esperado |
|-------|------|----------|
| 1 | Iniciar aula normalmente | TrackingSession criada |
| 2 | Desativar internet (Airplane Mode) | App desconectado da rede |
| 3 | Capturar 5 localizações | Localizações armazenadas offline (encriptadas) |
| 4 | Ativar internet novamente | Conexão restaurada |
| 5 | Aguardar sincronização automática | Cache é sincronizado ao backend |
| 6 | Verificar backend | 5 VehicleLocations presentes no banco de dados |

**Resultado esperado:** ✅ Dados offline sincronizados corretamente

---

#### Teste E2E-006: Validação de Campos Obrigatórios

**Objetivo:** Testar erro handling para inputs inválidos

| Passo | Ação | Esperado |
|-------|------|----------|
| 1 | Na tela de login, deixar email vazio | Campo marcado como obrigatório (validação local) |
| 2 | Tap "Entrar" | Não envia requisição, mostra mensagem de erro |
| 3 | Preencher email com valor inválido (ex: `abc`) | Campo marcado como inválido (validação local) |
| 4 | Preencher com email válido mas credencial errada | POST enviado |
| 5 | Backend retorna 401 | App mostra mensagem de erro: "Credenciais inválidas" |

**Resultado esperado:** ✅ Validações funcionando, erros tratados graciosamente

---

### 4.3 Testes de Cenários de Erro

#### Cenário E2E-007: Backend Offline

| Passo | Ação | Esperado |
|-------|------|----------|
| 1 | Parar o backend (Ctrl+C no terminal) | Backend desconectado |
| 2 | Na app, tentar fazer Login | Requisição falha |
| 3 | Verificar UI | Mostra erro: "Servidor indisponível. Tente novamente." |
| 4 | Reiniciar backend | Backend online novamente |
| 5 | Tentar Login novamente | Funciona normalmente |

**Resultado esperado:** ✅ Erro tratado, utilizador pode tentar novamente

---

#### Cenário E2E-008: Token JWT Expirado

| Passo | Ação | Esperado |
|-------|------|----------|
| 1 | Fazer login | JWT válido armazenado |
| 2 | Deixar app em background > 1 hora (ou simular expiração) | Token expira |
| 3 | Voltar a app, tentar GET `/api/lessons` | API retorna 401 Unauthorized |
| 4 | App valida erro 401 | Limpa token, redireciona para Login |
| 5 | Fazer login novamente | Novo JWT, acesso restaurado |

**Resultado esperado:** ✅ Refresh/re-login automático

---

### 4.4 Testes de Performance

| Teste | Métrica | Alvo | Método |
|-------|---------|------|--------|
| **Tempo de Login** | Seg | < 3 seg | Cronómetro manual |
| **Tempo de Listagem de Aulas** | Seg | < 2 seg | Cronómetro manual |
| **Consumo de Bateria (rastreamento 10min)** | % | < 15% | Verificar % bateria |
| **Tamanho Cache Offline** | MB | < 50 MB | Verificar AppDataDirectory |

---

## 5. Testes Manuais — Checklist

### 5.1 Instalação e Setup

- [ ] MAUI workload instalado: `dotnet workload list | grep maui`
- [ ] Backend compila sem erros: `dotnet build backend/`
- [ ] App mobile compila sem erros: `dotnet build mobile/FrotaGo.Mobile.csproj`
- [ ] ngrok conectado e expondo corretamente
- [ ] MobileConfig.BaseUrl apontando para ngrok URL válida
- [ ] Banco de dados seeded com dados de teste (instrutor, alunos, aulas, veículos)

### 5.2 Funcionalidade

- [ ] **Login:** Email e password válidos → Acesso concedido
- [ ] **Login:** Email/password inválido → Erro exibido
- [ ] **Aulas:** Lista carrega rapidamente (<2 seg)
- [ ] **Aulas:** Detalhe mostra todos os campos corretos
- [ ] **Aula Início:** Confirmação exibe dados corretos do backend
- [ ] **GPS:** Permissões concedidas → GPS ativo
- [ ] **Rastreamento:** Localizações capturadas a cada ~5-10 seg
- [ ] **Offline:** Sem internet, cache armazena localizações
- [ ] **Sincronização:** Com internet, cache sincronizado
- [ ] **Aula Parada:** Status finalizado no backend
- [ ] **Segurança:** JWT não exposto em logs/UI
- [ ] **Encriptação:** Cache offline encriptado (não legível em ficheiro)

### 5.3 UI/UX

- [ ] Layout responsivo em diferentes tamanhos de ecrã
- [ ] Cores e tipografia consistentes com Material Design
- [ ] Mensagens de erro claras em Português
- [ ] Spinners de carregamento aparecem durante requisições
- [ ] Botões desativados durante operações em curso
- [ ] Navegação intuitiva (volta, forward, home)

---

## 6. Resultados de Testes Registados

### 6.1 Testes Executados até Agora

| Data | Teste | Resultado | Notas |
|------|-------|-----------|-------|
| 15/07/2026 | `dotnet test mobile/tests/FrotaGo.Mobile.Tests` | ✅ PASS (2/2) | AuthService e ApiService básico |
| 15/07/2026 | POST `/api/auth/login` via curl + ngrok | ✅ PASS | JWT válido retornado |
| 15/07/2026 | GET `/api/lessons` com JWT via ngrok | ✅ PASS | 2 aulas retornadas |
| 15/07/2026 | Swagger JSON generation | ✅ PASS (após correção) | `TrackVehicleLocationRequest` renomeado |

### 6.2 Testes Pendentes (Próximas Fases)

- [ ] **E2E Completo:** App → Login → Aulas → Iniciar → Parar (Emulador/Dispositivo)
- [ ] **Offline Resilience:** Cache e sincronização
- [ ] **Performance:** Consumo bateria, tempo resposta
- [ ] **Segurança:** Testes de JWT, encriptação cache
- [ ] **Integração CI/CD:** Testes automatizados em pipeline

---

## 7. Procedimento de Execução de Testes

### 7.1 Testes Unitários

```bash
# Backend
cd /home/sgonga/Transferências/FrotaGo
dotnet test backend/FrotaGo.Backend/tests/FrotaGo.Application.Tests --logger "console;verbosity=normal"

# Mobile
dotnet test mobile/tests/FrotaGo.Mobile.Tests --logger "console;verbosity=minimal"
```

### 7.2 Testes de Integração

```bash
# Backend + BD (via TestServer)
dotnet test backend/FrotaGo.Backend/tests/FrotaGo.Application.Tests --filter "Category=Integration" --logger "console;verbosity=normal"
```

### 7.3 Testes E2E Manual

1. Iniciar backend:
   ```bash
   cd /home/sgonga/Transferências/FrotaGo
   dotnet run --project backend/FrotaGo.Backend/src/FrotaGo.Api/FrotaGo.Api.csproj
   ```

2. Garantir ngrok ativo (URL em MobileConfig.BaseUrl)

3. Compilar app mobile:
   ```bash
   dotnet build mobile/FrotaGo.Mobile.csproj -f net8.0-android
   ```

4. Executar em emulador Android:
   ```bash
   # Via Android Studio ou CLI
   dotnet build -f net8.0-android -c Release
   adb install -r bin/Release/net8.0-android/apk/FrotaGo.Mobile.apk
   adb shell am start -n com.frotago.mobile/.MainActivity
   ```

5. Seguir os passos dos testes E2E-001 a E2E-006

---

## 8. Critérios de Aceitação (DoD - Definition of Done)

- ✅ Todos os testes unitários passam (100% coverage em services críticos)
- ✅ Integração backend + BD validada
- ✅ Fluxo E2E completo testado (login → aulas → rastreamento → parada)
- ✅ Offline resilience confirmada
- ✅ Sem erros críticos ou crashes na app
- ✅ JWT e encriptação funcionando
- ✅ Documentação atualizada
- ✅ PR reviewado e aprovado

---

## 9. Contacts e Recursos

- **Backend:** [/home/sgonga/Transferências/FrotaGo/backend/FrotaGo.Backend](../backend/FrotaGo.Backend)
- **Mobile:** [/home/sgonga/Transferências/FrotaGo/mobile](../mobile)
- **Swagger:** http://127.0.0.1:5073/swagger/index.html
- **ngrok Tunnel:** https://9be3-105-174-52-229.ngrok-free.app (verificar URL atual)

---

**Próxima Revisão:** 16 de julho de 2026  
**Responsável:** @Serafimgonga
