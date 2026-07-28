# FrotaGo — Contexto do Projecto

## O que é
Plataforma web e **app móvel** para gestão completa da frota de veículos de
uma escola de condução em Angola: veículos, instrutores, alunos, aulas
práticas, manutenção, combustível, documentação, custos e relatórios.

Objectivo de longo prazo: evoluir de gestão de frota para um **ERP completo**
para escolas de condução (financeiro, alunos, instrutores, frota, comunicação,
facturação, relatórios, mobile).

## Perfis de utilizador (nunca inventar novos sem confirmar)
### MVP (Fase Inicial)
- **Administrador da Escola / Proprietário (Owner)** — gestão total da escola, utilizadores, veículos, relatórios e permissões.
- **Gestor de Frota** — controlo de veículos, manutenções, abastecimentos e rastreamento.
- **Rececionista** — cadastro de alunos, marcações de aulas, disponibilidade e pagamentos/recibos.
- **Instrutor / Motorista** — agenda, alunos atribuídos, registo e controlo de aulas, tracking de GPS.

### Futuros Perfis (Pós-MVP)
- **Super Admin** — gestão global da plataforma SaaS, criação de novas escolas, subscrições e logs de sistema.
- **Mecânico** — controlo técnico de manutenção, atualização de estado de veículos e registo de peças.
- **Contabilista** — gestão financeira dedicada, relatórios e faturação.
- **Aluno** — consulta de agenda, progresso e histórico de pagamentos.

### Arquitetura de Acesso e Onboarding
- **Multi-Tenancy (Escolas):** Todo o sistema é isolado por escola (`SchoolId` / Tenant).
- **Registo Público vs. Convites:**
  - **Apenas o Proprietário** efetua o registo público inicial da escola.
  - **Todos os outros utilizadores** (Instrutores, Rececionistas, Admins, Alunos) são **convidados** pelo Proprietário/Admin via e-mail (`https://app.frotago.com/invite/{token}`) para definirem a sua palavra-passe. Nenhum utilizador escolhe a escola no registo.
- **Perfis e Permissões:** Baseado em utilizador -> perfil (Role) -> permissões (`Permissions`) dinâmicas.

Para a especificação detalhada de onboarding e convites, ver:
[`docs/onboarding-and-tenancy.md`](file:///home/sgonga/Transferências/FrotaGo/docs/onboarding-and-tenancy.md)

## Domínio — entidades-chave
Veículo (matrícula, marca, modelo, chassis, combustível, câmbio, estado:
Disponível/Em aula/Em manutenção/Acidentado/Fora de serviço), Instrutor,
Aluno, Aula, Manutenção, Abastecimento, Documento (seguro, inspecção, DUA,
licença — com alertas de expiração), Acidente, Agendamento.

## Arquitectura do Sistema

```
        Mobile .NET MAUI (Instrutor)
              │ HTTPS + SignalR
              ▼
        ASP.NET Core API
              │
     SQL Server + SignalR Hub
              │
        Painel Web Angular (Admin/Gestor/Recepcionista)
```

O painel administrativo (Angular) consegue acompanhar tudo em **tempo real** via
SignalR. A app mobile (.NET MAUI) é exclusiva para Instrutores.

## Stack oficial — NÃO sugerir alternativas sem pedido explícito
- **Frontend Web:** Angular + Angular Material + TypeScript + RxJS
- **Backend:** ASP.NET Core Web API + Entity Framework Core + JWT
- **Mobile:** .NET MAUI (Android) — MVVM — para perfil Instrutor
- **BD:** SQL Server
- **Real-time:** SignalR (Hub `/hubs/tracking`)
- **Storage:** Azure Blob Storage ou MinIO (documentos/fotos)
- **Mapas:** Google Maps ou Leaflet + OpenStreetMap
- **Notificações:** Email, SMS, WhatsApp
- **Relatórios/PDF:** QuestPDF ou FastReport
- **GPS:** Transmissão via app mobile, consumo no painel web

## Fluxo Mobile do Instrutor

```
Login → Dashboard → Aulas de Hoje → Selecionar Aula → Ver Detalhes
  → Iniciar Aula (validar GPS/Internet/permissões)
  → Partilhar Localização em Tempo Real (SignalR)
  → Durante a Aula (atualizar GPS, mostrar percurso)
  → Terminar Aula (km, observações, avaliação)
  → Enviar Relatório
```

Para detalhes completos dos endpoints e contratos da API Mobile, ver:
[`docs/mobile-api-spec.md`](file:///home/sgonga/Transferências/FrotaGo/docs/mobile-api-spec.md)

## Diferencial competitivo (não esquecer ao propor features)
Módulo de **conformidade legal angolana**: validade automática de seguros,
inspecções, DUA e licenças, alertas e relatórios prontos para auditoria.

## Convenções gerais
- Idioma da UI e da documentação de negócio: **Português (Angola)**
- Nomes de variáveis/código: inglês (convenção internacional de código)
- Nunca propor tecnologias fora da stack sem justificar e pedir aprovação
- Este ficheiro é o contexto permanente. Regras detalhadas estão em
  `.agents/rules/`. Procedimentos especializados estão em `.agents/skills/`.
- Especificação de Onboarding & Multi-Tenancy está em `docs/onboarding-and-tenancy.md`.
- Especificação da API mobile está em `docs/mobile-api-spec.md`.
- Decisões de arquitectura de tracking/GPS estão em `docs/architecture-decisions.md`.
- Arquitectura cross-platform e desenvolvimento móvel Linux/Windows estão em `docs/mobile-architecture-crossplatform.md`.

