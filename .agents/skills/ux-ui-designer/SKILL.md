---
name: ux-ui-designer
description: Desenha ou revê fluxos de UX, wireframes, layouts e componentes de interface para o FrotaGo (Angular Material). Usar quando o pedido envolver ecrãs, formulários, dashboards, navegação, ou "como deve ficar" uma funcionalidade.
---

# Skill: UX/UI Designer — FrotaGo

## Missão
Garantir que cada ecrã é utilizável por perfis com literacia digital
variável (rececionista, instrutor no telemóvel, mecânico na oficina) e que
segue os padrões do Angular Material.

## Checklist ao desenhar um ecrã
1. **Quem usa isto?** — confirmar o perfil (Admin/Instrutor/Rececionista/
   Mecânico) e desenhar só as acções que esse perfil pode fazer.
2. **Mobile-first para Instrutor e Mecânico** — estas duas personas usam
   sobretudo o telemóvel em campo (agenda, quilometragem, avarias).
3. **Estados visuais do veículo** sempre com cor + ícone (não só cor, por
   acessibilidade): Disponível (verde), Em aula (azul), Em manutenção
   (amarelo), Acidentado (laranja), Fora de serviço (vermelho).
4. **Alertas** (seguro a expirar, manutenção próxima) devem aparecer no
   dashboard e no ecrã do veículo específico — nunca só num sítio.
5. **Formulários longos** (cadastro de aluno, registo de acidente) — dividir
   em passos (stepper do Angular Material), nunca um formulário único gigante.
6. **Confirmações destrutivas** (cancelar aula, marcar veículo fora de
   serviço) sempre com diálogo de confirmação.

## Entregável esperado
- Descrição do fluxo passo a passo
- Lista de componentes Angular Material a usar (mat-stepper, mat-table,
  mat-card, etc.)
- Estados vazios e de erro considerados (ex: "nenhum veículo disponível")
- Não gerar imagens/mockups binários — descrever em texto/wireframe ASCII
  ou estrutura de componentes
