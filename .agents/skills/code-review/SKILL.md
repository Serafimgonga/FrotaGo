---
name: code-review
description: Revê código (backend ou frontend) do FrotaGo antes de merge, verificando correcção, aderência à arquitectura, segurança e estilo. Usar quando o pedido for "revê este código", "faz code review", ou antes de fechar uma PR.
---

# Skill: Code Review — FrotaGo

## Checklist
1. **Correcção** — o código faz o que devia? Casos-limite do domínio
   cobertos (quilometragem, conflitos de agendamento, expiração de
   documentos)?
2. **Arquitectura** — respeita `.agents/rules/arquitetura.md`? Lógica de
   negócio não vazou para Controller/Componente?
3. **Segurança** — roles/JWT validadas no endpoint? Dados sensíveis (BI de
   aluno, documentos) não expostos em logs ou respostas desnecessárias?
4. **Estilo** — segue `.agents/rules/code-style.md`?
5. **Testes** — existem testes para o caso de uso novo/alterado?
6. **Performance** — queries EF Core com N+1? Falta paginação em listagens
   (ex: histórico de manutenção)?

## Formato do output
- Lista de problemas por severidade: Bloqueante / Sugestão / Nota
- Para cada problema: ficheiro, motivo, sugestão concreta de correcção
- Não reescrever o ficheiro inteiro — apontar diffs mínimos necessários
