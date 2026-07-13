---
name: backend-engineer-dotnet
description: Implementa código backend concreto (ASP.NET Core, EF Core, MediatR) para o FrotaGo. Usar quando o pedido for "implementar", "criar endpoint", "criar migration", "corrigir bug no backend", ou envolver C#/.NET directamente.
---

# Skill: Backend Engineer .NET — FrotaGo

## Missão
Implementar código backend robusto, testável e alinhado com Clean
Architecture + CQRS já definidos para o FrotaGo.

## Ao implementar um Command/Query
1. Criar o `Command`/`Query` em `Application/Features/<Modulo>/`
2. Criar o `Handler` correspondente (MediatR)
3. Validação com FluentValidation no `Validator` associado
4. Repositório/interface em `Application/Interfaces`, implementação em
   `Infrastructure/Repositories`
5. Migration EF Core só depois do modelo de domínio estar aprovado
6. Endpoint no Controller apenas chama `_mediator.Send(...)` — zero lógica
   de negócio no Controller
7. Testes unitários do Handler (xUnit + Moq/NSubstitute)

## Pontos de atenção específicos do FrotaGo
- Qualquer alteração de **quilometragem** de veículo deve validar que o
  novo valor é >= ao actual (nunca permitir recuar quilometragem)
- Qualquer alteração de **estado do veículo** deve verificar conflitos com
  agendamentos activos (não pode ficar "Em manutenção" com aula marcada)
- Datas de documentos (seguro, inspecção, DUA) alimentam o job de alertas —
  garantir que ficam em UTC e com índice na BD para consultas de expiração
- Nunca fazer commit de secrets/connection strings — usar
  `appsettings.Development.json` (gitignored) ou variáveis de ambiente

## Antes de terminar
- Correr os testes
- Confirmar que o endpoint respeita as roles (JWT) definidas em
  `.agents/rules/arquitetura.md`
