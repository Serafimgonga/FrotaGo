# FrotaGo

Plataforma web (e futuramente aplicação móvel) para gestão completa da
frota de veículos de uma escola de condução em Angola.

## O que o FrotaGo gere

- **Veículos** — matrícula, estado, quilometragem, documentação
- **Instrutores** — agenda, aulas realizadas, horas trabalhadas
- **Alunos** — cadastro, frequência, progresso nas aulas
- **Aulas práticas** — agendamento sem conflitos de horário
- **Manutenção** — histórico, alertas automáticos por quilometragem
- **Combustível** — consumo médio, custo por km
- **Documentação** — seguro, inspecção, DUA, licença, com alertas de expiração
- **Acidentes** — registo com fotos, custo estimado
- **Relatórios** — financeiro, frota, instrutores, alunos
- **Dashboard** — visão geral em tempo real da operação

Objectivo de longo prazo: evoluir para um **ERP completo** para escolas de
condução (financeiro, facturação, comunicação com alunos, app móvel).

## Perfis de utilizador

| Perfil | Principais acções |
|---|---|
| Administrador | Utilizadores, veículos, instrutores, alunos, relatórios, aprovação de despesas |
| Instrutor | Agenda, alunos, registo de aula, quilometragem, avarias |
| Rececionista | Cadastro de alunos, marcações, disponibilidade, recibos |
| Mecânico (opcional) | Manutenção, estado do veículo |

## Diferencial

Módulo de **conformidade legal angolana**: acompanhamento automático da
validade de seguros, inspecções, DUA e licenças, com alertas e relatórios
prontos para auditoria interna.

## Stack tecnológica

| Camada | Tecnologia |
|---|---|
| Frontend | Angular, Angular Material, TypeScript, RxJS |
| Backend | ASP.NET Core Web API, Entity Framework Core, JWT |
| Base de dados | SQL Server |
| Storage | Azure Blob Storage / MinIO |
| Mapas | Google Maps / Leaflet + OpenStreetMap |
| Notificações | Email, SMS, WhatsApp |
| Relatórios/PDF | QuestPDF / FastReport |
| GPS (Premium) | Localização, velocidade, percurso |

## Estrutura do repositório

```
FrotaGo/
├── AGENTS.md          # Contexto do projecto para agentes de IA (Antigravity)
├── .agents/
│   ├── rules/          # Regras sempre activas (arquitectura, estilo de código)
│   └── skills/          # Skills especializadas (UX, arquitectura, backend, frontend, QA, code review)
├── backend/            # ASP.NET Core Web API (Clean Architecture + CQRS)
├── frontend/           # Angular (standalone components + Angular Material)
├── mobile/             # Aplicação móvel (instrutores em campo)
└── docs/               # Documentação funcional e técnica
```

## Arquitectura (resumo)

- Backend em **Clean Architecture**: `Domain`, `Application`, `Infrastructure`, `API`
- **CQRS + MediatR** para casos de uso complexos
- Autenticação **JWT** com roles por perfil de utilizador
- Frontend Angular **standalone**, com signals para estado local

Detalhes completos em `.agents/rules/arquitetura.md`.

## Como correr o projecto localmente

### Backend
```bash
cd backend
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend
```bash
cd frontend
npm install
ng serve
```

> Ajusta os comandos acima consoante a estrutura final de pastas dentro de
> `backend/` e `frontend/` (ex: nome do projecto .csproj, workspace Angular).

## Desenvolvimento assistido por IA

Este projecto usa o [Google Antigravity](https://antigravity.google) com
contexto próprio definido em `AGENTS.md` e skills especializadas em
`.agents/skills/` (UX/UI, arquitectura de software, engenharia backend/
frontend, code review e QA). Qualquer agente de IA a trabalhar neste
repositório deve ler `AGENTS.md` primeiro.

## Roadmap

- [ ] Módulos base: veículos, instrutores, alunos, agendamento
- [ ] Manutenção e combustível com alertas automáticos
- [ ] Gestão documental com alertas de expiração
- [ ] Dashboard e relatórios
- [ ] QR Code por veículo
- [ ] Assinatura digital pós-aula
- [ ] Aplicação móvel para instrutores
- [ ] Integração GPS (Premium)
- [ ] Módulo de IA (previsão de manutenção, análise de custos, perguntas em linguagem natural)

## Licença

A definir.
