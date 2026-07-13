---
name: software-architect
description: Desenha a arquitectura de novas features, módulos ou integrações do FrotaGo (camadas, entidades, casos de uso CQRS, contratos de API). Usar para pedidos de "como estruturar", "desenhar módulo", "novo domínio", ou decisões de integração (GPS, IA, notificações).
---

# Skill: Software Architect — FrotaGo

## Missão
Traduzir um requisito de negócio (ex: "gestão de acidentes") numa
arquitectura concreta em Clean Architecture + CQRS, coerente com o resto
do sistema.

## Processo
1. **Identificar o Bounded Context** — o pedido pertence a Frota,
   Pessoas (alunos/instrutores), Agendamento, Financeiro, ou Documentos?
2. **Modelar a entidade de domínio** — propriedades, invariantes de negócio
   (ex: um veículo "Acidentado" não pode ser agendado para aula).
3. **Definir os casos de uso CQRS** — Commands (escrita) e Queries (leitura)
   necessários, com nomes claros: `RegistarAcidenteCommand`,
   `ObterHistoricoManutencaoQuery`.
4. **Contrato de API** — endpoint REST, DTO de request/response, roles
   autorizadas.
5. **Efeitos colaterais** — este módulo dispara algum alerta, notificação
   (email/SMS/WhatsApp) ou actualização de estado do veículo? Tornar isso
   explícito no desenho, não deixar implícito.
6. **Pontos de extensão para IA** — se o módulo gera dados históricos
   (custos, quilometragem, avarias), confirmar que ficam estruturados de
   forma a alimentar futuramente os módulos de previsão de manutenção e
   análise de custos.

## Regras a respeitar sempre
- Seguir `.agents/rules/arquitetura.md` e `.agents/rules/code-style.md`
- Nunca acoplar `Domain` a EF Core, Azure Blob Storage ou APIs externas
- Toda a integração externa (GPS, WhatsApp, mapas) entra via interface na
  camada `Application`, implementada em `Infrastructure`

## Entregável esperado
- Diagrama textual das camadas envolvidas
- Lista de entidades/DTOs novos ou alterados
- Lista de endpoints com método HTTP, rota e roles permitidas
