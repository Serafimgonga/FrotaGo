# FrotaGo — Contexto do Projecto

## O que é
Plataforma web (e futuramente app móvel) para gestão completa da frota de
veículos de uma escola de condução em Angola: veículos, instrutores, alunos,
aulas práticas, manutenção, combustível, documentação, custos e relatórios.

Objectivo de longo prazo: evoluir de gestão de frota para um **ERP completo**
para escolas de condução (financeiro, alunos, instrutores, frota, comunicação,
facturação, relatórios, mobile).

## Perfis de utilizador (nunca inventar novos sem confirmar)
### MVP (Fase Inicial)
- **Administrador da Escola** — gestão total da escola, utilizadores, veículos, relatórios e permissões.
- **Gestor de Frota** — controlo de veículos, manutenções, abastecimentos e rastreamento.
- **Rececionista** — cadastro de alunos, marcações de aulas, disponibilidade e pagamentos/recibos.
- **Instrutor / Motorista** — agenda, alunos atribuídos, registo e controlo de aulas, tracking de GPS.

### Futuros Perfis (Pós-MVP)
- **Super Admin** — gestão global da plataforma SaaS, criação de novas escolas, subscrições e logs de sistema.
- **Mecânico** — controlo técnico de manutenção, atualização de estado de veículos e registo de peças.
- **Contabilista** — gestão financeira dedicada, relatórios e faturação.
- **Aluno** — consulta de agenda, progresso e histórico de pagamentos.

### Arquitetura de Acesso
- **Multi-Tenancy (Escolas):** Todo o sistema é isolado por escola (`SchoolId` / Tenant).
- **Perfis e Permissões:** Baseado em utilizador -> perfil (Role) -> permissões (Permissions) dinâmicas, permitindo flexibilidade futura de acessos.

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
