# FrotaGo — Contexto do Projecto

## O que é
Plataforma web (e futuramente app móvel) para gestão completa da frota de
veículos de uma escola de condução em Angola: veículos, instrutores, alunos,
aulas práticas, manutenção, combustível, documentação, custos e relatórios.

Objectivo de longo prazo: evoluir de gestão de frota para um **ERP completo**
para escolas de condução (financeiro, alunos, instrutores, frota, comunicação,
facturação, relatórios, mobile).

## Perfis de utilizador (nunca inventar novos sem confirmar)
- **Administrador** — utilizadores, veículos, instrutores, alunos, relatórios, aprovação de despesas
- **Instrutor** — agenda, alunos, registo de aula, quilometragem, avarias
- **Rececionista** — cadastro de alunos, marcações, disponibilidade, recibos
- **Mecânico (opcional)** — manutenção, estado do veículo

## Domínio — entidades-chave
Veículo (matrícula, marca, modelo, chassis, combustível, câmbio, estado:
Disponível/Em aula/Em manutenção/Acidentado/Fora de serviço), Instrutor,
Aluno, Aula, Manutenção, Abastecimento, Documento (seguro, inspecção, DUA,
licença — com alertas de expiração), Acidente, Agendamento.

## Stack oficial — NÃO sugerir alternativas sem pedido explícito
- Frontend: Angular + Angular Material + TypeScript + RxJS
- Backend: ASP.NET Core Web API + Entity Framework Core + JWT
- BD: SQL Server
- Storage: Azure Blob Storage ou MinIO (documentos/fotos)
- Mapas: Google Maps ou Leaflet + OpenStreetMap
- Notificações: Email, SMS, WhatsApp
- Relatórios/PDF: QuestPDF ou FastReport
- GPS: versão Premium apenas

## Diferencial competitivo (não esquecer ao propor features)
Módulo de **conformidade legal angolana**: validade automática de seguros,
inspecções, DUA e licenças, alertas e relatórios prontos para auditoria.

## Convenções gerais
- Idioma da UI e da documentação de negócio: **Português (Angola)**
- Nomes de variáveis/código: inglês (convenção internacional de código)
- Nunca propor tecnologias fora da stack sem justificar e pedir aprovação
- Este ficheiro é o contexto permanente. Regras detalhadas estão em
  `.agents/rules/`. Procedimentos especializados estão em `.agents/skills/`.
