# Arquitetura Multi-Tenant & Fluxo de Convites (Onboarding)

 Este documento descreve as decisões arquiteturais sobre a gestão de tenants (escolas), autenticação e onboarding de utilizadores no **FrotaGo**.

---

## 1. Princípios do Modelo Multi-Tenant SaaS

No FrotaGo, cada escola de condução é um **Tenant** totalmente isolado na base de dados (SQL Server via `SchoolId` e EF Core Global Query Filters).

```
               FrotaGo SaaS

     ┌─────────────────────────────────────┐
     │                                     │
     │  Tenant: Escola Alfa                │
     │    ├── Proprietário (Owner)         │
     │    ├── Recepcionistas               │
     │    ├── Instrutores                  │
     │    └── Financeiro                   │
     │                                     │
     ├─────────────────────────────────────┤
     │ Tenant: Escola Beta                 │
     │    ├── Proprietário (Owner)         │
     │    ├── Instrutores                  │
     │    └── Alunos                       │
     ├─────────────────────────────────────┤
     │ Tenant: ICSN                        │
     │    ├── Proprietário (Owner)         │
     │    ├── Instrutores                  │
     │    ├── Motoristas                   │
     │    └── Recepção                     │
     └─────────────────────────────────────┘
```

---

## 2. Quem se regista vs. Quem é convidado

Apenas o **Proprietário da Escola** efetua o registo público inicial na plataforma. Todos os restantes papéis entram exclusivamente por **convite ou criação direta**.

| Papel | Quem cria? | Registo Público? | Método de Entrada |
| :--- | :--- | :---: | :--- |
| **Proprietário (Owner)** | Ele próprio | ✅ **Sim** | Efetua o registo da escola no portal SaaS |
| **Administrador** | Proprietário | ❌ **Não** | Convite por e-mail enviado via Painel Web |
| **Gestor de Frota** | Proprietário / Admin | ❌ **Não** | Convite por e-mail enviado via Painel Web |
| **Rececionista** | Proprietário / Admin | ❌ **Não** | Convite por e-mail enviado via Painel Web |
| **Financeiro** | Proprietário / Admin | ❌ **Não** | Convite por e-mail enviado via Painel Web |
| **Instrutor** | Proprietário / Admin | ❌ **Não** | Convite por e-mail; acede à App Mobile (.NET MAUI) |
| **Aluno** | Receção / Admin | ❌ **Não** | Registo pela receção; aceita convite para definir palavra-passe |

---

## 3. Fluxo de Registo & Convite

### 3.1. Registo da Escola (Proprietário)
```
Registo no Portal Web
   │
   ▼
Criação da Entidade "School" (Name, NIF, Slug, Plan, Status)
   │
   ▼
Criação da Conta "User" (Role = Owner, SchoolId = X)
   │
   ▼
Acesso ao Dashboard Principal
```

### 3.2. Fluxo de Convite para Utilizadores (Instrutores, Rececionistas, etc.)
```
Painel Web (Admin/Owner) ──► Enviar Convite (Nome, Email, Telefone, Role)
                                    │
                                    ▼
                      Backend gera registro "Invitation"
                      Envia e-mail: https://app.frotago.com/invite/{token}
                                    │
                                    ▼
                      Utilizador clica no link
                                    │
                                    ▼
                      Definir Palavra-passe & Confirmar
                                    │
                                    ▼
                      Conta ativada e associada à Escola (SchoolId predefinido)
```

> **Por que evitar registo público para instrutores/alunos?**
> Evita erros de digitação de nomes de escola, criação de duplicados, acessos indevidos a tenants errados e tentativas de fraude. O utilizador nunca precisa escolher a escola no momento de registo/login.

---

## 4. Autenticação & Resolução de Tenant (JWT)

### 4.1. Login Simplificado
O login requer apenas `Email` e `Password`. A associação à escola e as permissões são resolvidas automaticamente no backend.

```
POST /api/auth/login
{
  "email": "instrutor@icsn.co.ao",
  "password": "********"
}
```

### 4.2. Claims do JWT Token
```json
{
  "sub": "user_id_15",
  "schoolId": "4",
  "role": "Instructor",
  "email": "instrutor@icsn.co.ao"
}
```
Todas as consultas no backend aplicam um filtro automático por `SchoolId`.

### 4.3. Login na App Mobile (Instrutor)
A aplicação móvel (.NET MAUI) envia apenas e-mail e palavra-passe. A API responde com a informação da escola e o token JWT:
```json
{
  "school": "ICSN",
  "role": "Instructor",
  "token": "..."
}
```
A App Mobile configura o estado interno automaticamente sem exigir qualquer seleção de escola ao utilizador.

---

## 5. Evolução Futura (Suporte a Múltiplas Escolas por Utilizador)

Para suportar escala no futuro onde um mesmo profissional (ex.: instrutor ou consultor) trabalha para mais do que uma escola:
- Uma conta única (`User` com e-mail único).
- Tabela de associação `UserSchool` (`UserId`, `SchoolId`, `RoleId`).
- Ao iniciar sessão com múltiplas associações ativas, o sistema apresenta um ecrã de seleção:
  ```
  Selecione a escola para aceder:
  ( ) Escola ICSN - Instrutor
  ( ) Escola Alfa - Administrador
  ```
Este desenho de base de dados deve ser mantido em mente para fácil migração sem breaking changes na autenticação core.
