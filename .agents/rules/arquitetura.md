# Regra: Arquitectura & Stack

- Backend segue **Clean Architecture** em .NET: `Domain`, `Application`,
  `Infrastructure`, `API`. Sem lógica de negócio em Controllers.
- Usar **CQRS + MediatR** para casos de uso complexos (aulas, agendamento,
  relatórios). CRUD simples pode ser directo via repositório.
- Entity Framework Core apenas na camada `Infrastructure`. `Domain` não
  conhece EF Core.
- Autenticação & Autorização: JWT baseado no modelo **RBAC (Role-Based Access Control)** com suporte para permissões dinâmicas: Utilizador (`Users`) -> Perfil (`Roles`) -> Permissões (`Permissions` via `RolePermissions`).
- Multi-Tenancy (SaaS): Todo o sistema deve ser isolado por escola. Entidades de domínio pertencentes a uma escola devem conter uma chave estrangeira `SchoolId`, garantindo isolamento total de dados na base de dados (SQL Server).
- Perfis Iniciais (MVP): Focar estritamente em **Admin** (Administrador da Escola), **Rececionista**, **Instrutor** e **Gestor de Frota**. Perfis pós-MVP (Super Admin, Mecânico, Contabilista, Aluno) serão adicionados posteriormente.
- Nunca sugerir NoSQL/MongoDB — a base de dados oficial e exclusiva é o SQL Server.
- Toda a nova funcionalidade de frota (veículos, manutenção, combustível)
  deve considerar o módulo de alertas automáticos (ex: troca de óleo,
  vencimento de seguro) como parte do desenho, não como extra opcional.
- A aplicação móvel (**mobile/**) para o perfil de **Instrutor** é desenvolvida em **.NET MAUI** utilizando o padrão **MVVM**, organizando o código em Vertical Slices (`Features/`) e serviços nativos (`Services/`).
- Para detalhes da arquitetura de tracking GPS e roadmap (V1, V2, V3), consultar sempre [docs/architecture-decisions.md](file:///home/sgonga/Transfer%C3%AAncias/FrotaGo/docs/architecture-decisions.md).
