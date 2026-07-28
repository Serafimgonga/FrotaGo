# Regra: Arquitectura & Stack

- Backend segue **Clean Architecture** em .NET: `Domain`, `Application`,
  `Infrastructure`, `API`. Sem lógica de negócio em Controllers.
- Usar **CQRS + MediatR** para casos de uso complexos (aulas, agendamento,
  relatórios). CRUD simples pode ser directo via repositório.
- Entity Framework Core apenas na camada `Infrastructure`. `Domain` não
  conhece EF Core.
- Autenticação & Autorização: JWT baseado no modelo **RBAC (Role-Based Access Control)** com suporte para permissões dinâmicas: Utilizador (`Users`) -> Perfil (`Roles`) -> Permissões (`Permissions` via `RolePermissions`).
- Multi-Tenancy (SaaS) & Onboarding:
  - Todo o sistema é isolado por escola via `SchoolId` (SQL Server com EF Core Global Query Filters).
  - **Apenas o Proprietário da Escola (Owner)** faz registo público inicial.
  - **Instrutores, Rececionistas, Admins e Alunos entram exclusivamente por convite** (`/invite/{token}`) ou criação direta via painel. Nunca selecionam a escola manualmente no registo ou login.
  - O JWT contém `schoolId` e `role`. Login usa apenas `email` e `password`.
  - Ver especificação em [`docs/onboarding-and-tenancy.md`](file:///home/sgonga/Transferências/FrotaGo/docs/onboarding-and-tenancy.md).
- Perfis Iniciais (MVP): Focar estritamente em **Admin** (Administrador da Escola), **Rececionista**, **Instrutor** e **Gestor de Frota**. Perfis pós-MVP (Super Admin, Mecânico, Contabilista, Aluno) serão adicionados posteriormente.
- Nunca sugerir NoSQL/MongoDB — a base de dados oficial e exclusiva é o SQL Server.
- Toda a nova funcionalidade de frota (veículos, manutenção, combustível)
  deve considerar o módulo de alertas automáticos (ex: troca de óleo,
  vencimento de seguro) como parte do desenho, não como extra opcional.

## Aplicação Mobile (Instrutor)

- A aplicação móvel (**mobile/**) para o perfil de **Instrutor** é desenvolvida em **.NET MAUI** (Android) utilizando o padrão **MVVM**, organizando o código em Vertical Slices (`Features/`) e serviços nativos (`Services/`).
- Endpoints mobile usam o prefixo `/api/mobile/*` para separação clara da API web.
- Fluxo principal: **Login → Dashboard → Aulas de Hoje → Selecionar Aula → Detalhes → Iniciar Aula → GPS/SignalR → Terminar Aula → Relatório**.
- A comunicação em tempo real usa **SignalR** (Hub `/hubs/tracking`) com autenticação JWT.
- Para a especificação completa dos endpoints e contratos, ver [`docs/mobile-api-spec.md`](file:///home/sgonga/Transferências/FrotaGo/docs/mobile-api-spec.md).
- Para detalhes da arquitectura de tracking GPS e roadmap (V1, V2, V3), consultar sempre [`docs/architecture-decisions.md`](file:///home/sgonga/Transferências/FrotaGo/docs/architecture-decisions.md).
