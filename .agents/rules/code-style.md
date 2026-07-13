# Regra: Estilo de Código

## Backend (.NET)
- C# 12, nullable reference types activado
- Nomes de classes/métodos em inglês; nomes de campos de negócio (matrícula,
  aluno, instrutor) podem manter-se em português quando reflectem o domínio
- Um DTO por caso de uso — não reutilizar entidades de domínio como resposta de API
- Validação com FluentValidation nos comandos CQRS

## Frontend (Angular)
- Standalone components, sem NgModules novos
- Signals para estado local; RxJS para streams assíncronos (HTTP, sockets)
- Angular Material para todos os componentes de UI — não misturar com outras libs de UI
- Um serviço por domínio (VeiculoService, AulaService, etc.), nunca um "GodService"

## Geral
- Commits em Conventional Commits (feat:, fix:, chore:, docs:)
- Nenhum "número mágico" — usar constantes nomeadas (ex: dias de alerta antes
  de expirar documento)
