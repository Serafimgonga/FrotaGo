# Guia de Deploy e Migração: Render (Backend) & Vercel (Frontend)

Este guia orienta o processo passo a passo para migrar o **FrotaGo** da AWS para o **Render** (Backend API + Base de Dados) e a **Vercel** (Frontend Angular), aproveitando os planos gratuitos de ambas as plataformas.

---

## 1. Visão Geral da Arquitetura

```
  ┌─────────────────────────┐               ┌─────────────────────────┐
  │   Vercel (Frontend)     │               │     Render (Backend)    │
  │   https://frotago.vercel.app            │   https://frotago-api.onrender.com
  │                         │  HTTPS API    │                         │
  │   - Angular 22 (SPA)    ├──────────────►│   - ASP.NET Core Web API│
  │   - CDN Global Ultrarrápida             │   - .NET 8 (Docker)     │
  │                         │  WebSocket    │   - Hub SignalR / GPS   │
  │   - Tracking em Tempo Real├─────────────►│                         │
  └─────────────────────────┘               └───────────┬─────────────┘
                                                        │
                                                        │ EF Core (Npgsql)
                                                        ▼
                                            ┌─────────────────────────┐
                                            │  Render PostgreSQL DB   │
                                            │  (ou Supabase Grátis)   │
                                            └─────────────────────────┘
```

---

### 2.1 Criar a Base de Dados no Supabase (Grátis e Permanente)

1. Aceda a [supabase.com](https://supabase.com) e crie uma conta gratuita (ou faça login com GitHub).
2. Clique em **"New Project"**.
3. Preencha:
   - **Name:** `frotago-db`
   - **Database Password:** Escolha uma senha segura (guarde-a bem).
   - **Region:** Escolha uma região próxima (ex: `Central EU (Frankfurt)` ou `West EU (London)`).
4. Clique em **"Create new project"** e aguarde 1 a 2 minutos até o banco inicializar.
5. No painel do seu projeto no Supabase:
   - Vá ao menu lateral **Project Settings** (ícone de engrenagem) → **Database**.
   - Role até **Connection String** e selecione a aba **URI** (ou **ADO.NET**).
   - Copie a string de conexão (substituindo `[YOUR-PASSWORD]` pela senha que você criou).
   - Exemplo: `postgresql://postgres.[REF]:[SENHA]@aws-0-eu-central-1.pooler.supabase.com:6543/postgres`

---

### 2.2 Deploy do Web Service no Render (via Blueprint)

1. Aceda ao seu painel em [dashboard.render.com](https://dashboard.render.com).
2. No Blueprint existente ou em **"New +"** → **"Blueprint"**, conecte o repositório do FrotaGo.
3. O Render irá ler o [`render.yaml`](file:///home/sgonga/Transferências/FrotaGo/render.yaml) atualizado.
4. No campo **`ConnectionStrings__DefaultConnection`**, cole a Connection String copiada do Supabase.
5. Clique em **"Apply"**. O Render iniciará a compilação do container Docker e a API estará no ar em instantes!

---

### Opção B: Criação Manual no Render

Caso prefira criar os serviços individualmente pelo dashboard:

#### 1. Criar o Banco de Dados PostgreSQL:
1. No Render, clique em **"New +"** → **"PostgreSQL"**.
2. Defina o Nome: `frotago-db` e Database: `frotago`.
3. Escolha a região (ex: `Frankfurt` para menor latência com Angola ou `Oregon`).
4. Selecione o plano **Free**.
5. Clique em **"Create Database"**.
6. Após a criação, copie o valor do campo **"Internal Database URL"** (se a API estiver na mesma conta Render) ou **"External Database URL"**.

#### 2. Criar o Web Service da API:
1. No Render, clique em **"New +"** → **"Web Service"**.
2. Conecte o repositório GitHub do FrotaGo.
3. Preencha as configurações:
   - **Name:** `frotago-api`
   - **Region:** A mesma do banco de dados.
   - **Root Directory:** `backend/FrotaGo.Backend`
   - **Runtime:** `Docker` (usará o Dockerfile automaticamente).
   - **Instance Type:** `Free`.
4. Em **Environment Variables**, adicione as seguintes chaves:
   - `ASPNETCORE_ENVIRONMENT` = `Production`
   - `ConnectionStrings__DefaultConnection` = *(cole a URL do PostgreSQL copiada acima)*
   - `JwtSettings__Secret` = *(digite uma chave segura com mais de 32 caracteres, ex: `frotago_super_secret_jwt_token_key_2026`)*
   - `JwtSettings__Issuer` = `FrotaGoApi`
   - `JwtSettings__Audience` = `FrotaGoApp`
   - `EnableSwagger` = `true` *(opcional, permite testar os endpoints pelo Swagger)*
5. Clique em **"Create Web Service"**.

---

### Testar a API no Render:
Assim que o build terminar e o status for **Live**, aceda ao link fornecido pelo Render (ex: `https://frotago-api.onrender.com`):
- `https://frotago-api.onrender.com/healthz` -> Deve retornar `Healthy` (HTTP 200).
- `https://frotago-api.onrender.com/` -> Retorna `{"service": "FrotaGo API", "status": "running"}`.
- `https://frotago-api.onrender.com/swagger` -> Interface interativa da API.

> [!NOTE]
> **Migrações Automáticas:** O backend executa `db.Database.Migrate()` automaticamente no arranque. Portanto, todas as tabelas do FrotaGo (escolas, utilizadores, veículos, aulas, etc.) são criadas logo no primeiro arranque sem necessidade de comandos manuais.

---

## 3. Passo 2: Deploy do Frontend na Vercel

1. Aceda ao painel da Vercel em [vercel.com](https://vercel.com) e inicie sessão com o GitHub.
2. Clique em **"Add New..."** → **"Project"**.
3. Importe o repositório **FrotaGo**.
4. Configure as opções do projeto na tela de importação:
   - **Framework Preset:** `Angular`
   - **Root Directory:** Clique em *Edit* e selecione a pasta `frontend`.
   - **Build and Output Settings:**
     - **Build Command:** `npm run build`
     - **Output Directory:** `dist/frontend/browser` *(certifique-se de preencher `dist/frontend/browser`)*
     - **Install Command:** `npm install`
5. *(Opcional)* Se quiser que o frontend se conecte diretamente à URL do Render em vez de usar caminhos relativos:
   - Abra o ficheiro [`frontend/src/environments/environment.prod.ts`](file:///home/sgonga/Transferências/FrotaGo/frontend/src/environments/environment.prod.ts) e insira a URL do Render:
     ```typescript
     export const environment = {
       production: true,
       apiUrl: 'https://frotago-api.onrender.com',
       hubUrl: 'https://frotago-api.onrender.com/hubs/gps'
     };
     ```
6. Clique no botão **"Deploy"**.

Em menos de 1 minuto o site estará disponível no ar sob um domínio como `https://frotago.vercel.app` (ou o nome do seu projeto).

---

## 4. Passo 3: Verificação de Ponta a Ponta

1. **Aceder ao Frontend:** Abra a URL gerada pela Vercel (`https://frotago.vercel.app`).
2. **Registo da Escola / Login:**
   - Efetue o primeiro registo na landing page / registo de escola (`/register-school`).
   - Verifique que o utilizador proprietário e o tenant da escola são criados com sucesso.
3. **Módulo de Veículos:** Adicione um veículo de teste com a matrícula padrão angolana (ex: `LD-01-23-AA`).
4. **Rastreamento em Tempo Real (SignalR):**
   - Aceda à página **Tracking** (`/tracking`).
   - Verifique que o status de conexão exibe `Online` ou `Connected` conectado ao Hub `/hubs/gps` do Render.
5. **Navegação SPA:**
   - Navegue para `/vehicles`, `/maintenance`, `/fuel` e recarregue a página (`F5`).
   - O ficheiro [`vercel.json`](file:///home/sgonga/Transferências/FrotaGo/frontend/vercel.json) garante que o servidor Vercel devolva o `index.html` e a rota não apresente erro 404.

---

## 5. Dicas Importantes sobre o Plano Gratuito do Render

> [!TIP]
> **Cold Start (Suspensão por inatividade no Render Free):**
> No plano gratuito do Render, se a API não receber tráfego durante 15 minutos, ela entra em suspensão ("hibernação"). A primeira requisição após a suspensão pode demorar cerca de 30 a 50 segundos para acordar o container.
> 
> **Como contornar gratuitamente:**
> Pode configurar um monitor gratuito em serviços como [UptimeRobot](https://uptimerobot.com) ou [Cron-Job.org](https://cron-job.org) para disparar uma requisição `GET` para `https://frotago-api.onrender.com/healthz` a cada 10 minutos. Isso mantém a API sempre acordada e rápida!
