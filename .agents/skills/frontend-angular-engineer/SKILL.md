---
name: frontend-angular-engineer
description: Implementa componentes, serviços e telas Angular para o FrotaGo. Usar quando o pedido for "criar componente", "criar tela", "consumir API", ou envolver TypeScript/Angular directamente.
---

# Skill: Frontend Angular Engineer — FrotaGo

## Missão
Implementar UI Angular standalone, consumindo a API .NET, seguindo
Angular Material e as decisões de UX já tomadas.

## Ao implementar uma tela/componente
1. Componente standalone em `src/app/features/<modulo>/`
2. Serviço dedicado ao domínio (`VeiculoService`, `AulaService`...) usando
   `HttpClient` + tipos gerados/alinhados com os DTOs do backend
3. Estado local com `signal()`; chamadas HTTP com RxJS + `takeUntilDestroyed`
4. Formulários com `ReactiveFormsModule` + validações espelhando as regras
   do backend (ex: quilometragem não pode diminuir)
5. Guardas de rota por role (Admin/Instrutor/Rececionista/Mecânico) usando
   `CanActivateFn`
6. Loading states e mensagens de erro (mat-snackbar) em toda chamada HTTP

## Pontos de atenção específicos do FrotaGo
- Ecrãs de Instrutor (agenda, quilometragem, avarias) devem funcionar bem
  em ecrã de telemóvel — testar em viewport estreito
- Badges de estado do veículo devem usar as cores definidas na skill de UX
  (`.agents/skills/ux-ui-designer/SKILL.md`)
- Alertas de documentos a expirar aparecem como notificação visível no
  dashboard, não escondidos numa tab secundária

## Antes de terminar
- Confirmar que segue `.agents/rules/code-style.md`
- Sem `any` implícito em TypeScript
- Componente testável isoladamente (inputs/outputs claros)
