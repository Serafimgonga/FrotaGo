# Regra: Arquitectura & Stack

- Backend segue **Clean Architecture** em .NET: `Domain`, `Application`,
  `Infrastructure`, `API`. Sem lógica de negócio em Controllers.
- Usar **CQRS + MediatR** para casos de uso complexos (aulas, agendamento,
  relatórios). CRUD simples pode ser directo via repositório.
- Entity Framework Core apenas na camada `Infrastructure`. `Domain` não
  conhece EF Core.
- Autenticação: JWT com roles = Administrador, Instrutor, Rececionista,
  Mecânico. Nunca misturar permissões entre roles sem confirmação.
- Nunca sugerir NoSQL/MongoDB — a base de dados é SQL Server.
- Toda a nova funcionalidade de frota (veículos, manutenção, combustível)
  deve considerar o módulo de alertas automáticos (ex: troca de óleo,
  vencimento de seguro) como parte do desenho, não como extra opcional.
