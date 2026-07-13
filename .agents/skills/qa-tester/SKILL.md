---
name: qa-tester
description: Gera casos de teste, planos de teste manual ou testes automatizados para funcionalidades do FrotaGo. Usar quando o pedido for "criar testes", "plano de QA", ou "quais os cenários a testar".
---

# Skill: QA / Tester — FrotaGo

## Missão
Garantir cobertura de teste para as regras de negócio críticas do FrotaGo,
não só o "caminho feliz".

## Cenários que NUNCA podem faltar
- Conflito de agendamento (mesmo veículo/instrutor em dois horários)
- Veículo em manutenção não pode ser agendado para aula
- Quilometragem não pode recuar
- Alerta de documento a expirar dispara na janela correcta (ex: 15 dias antes)
- Permissões por role: Instrutor não consegue aceder a ecrãs de Admin
- Aluno com carta suspensa/estado inválido não pode ser agendado
- Upload de documento com formato/tamanho inválido é rejeitado

## Formato de um caso de teste
```
ID: TC-<modulo>-NN
Título:
Pré-condições:
Passos:
Resultado esperado:
Prioridade: Alta/Média/Baixa
```

## Entregável esperado
- Casos de teste priorizados (críticos primeiro)
- Indicar se é candidato a teste automatizado (unitário/integração) ou só
  teste manual exploratório
