# FrotaGo — Especificação da API Mobile (Instrutor)

Este documento descreve todos os endpoints da API mobile consumidos pela
aplicação .NET MAUI do Instrutor.

---

## Arquitectura

```
        Mobile MAUI / Android
              │ HTTPS
              ▼
        ASP.NET Core API
              │
     SQL Server + SignalR
              │
        Painel Web Angular
```

O painel administrativo consegue acompanhar tudo em tempo real.

---

## Fluxo Completo do Instrutor

```
Login
   │
   ▼
Dashboard
   │
   ▼
Aulas de hoje
   │
   ▼
Selecionar aula
   │
   ▼
Ver detalhes
   │
   ├── Aluno
   ├── Veículo
   ├── Hora
   ├── Local de encontro
   ▼
Iniciar aula
   │
   ├── Verificar GPS
   ├── Verificar Internet
   ├── Verificar permissões
   ▼
Partilhar localização em tempo real
   │
   ▼
Durante a aula
   │
   ├── Atualizar posição GPS
   ├── Mostrar percurso
   ├── Comunicar com SignalR
   ▼
Terminar aula
   │
   ├── Quilómetros percorridos
   ├── Observações
   ├── Avaliação
   ├── Estado do aluno
   ▼
Enviar relatório
```

---

## Endpoints

### 1. Autenticação

#### Login
```
POST /api/auth/login
```

**Request:**
```json
{
  "email": "instrutor@icsn.co.ao",
  "password": "********"
}
```

**Response:**
```json
{
  "token": "...",
  "refreshToken": "...",
  "user": {
    "id": "...",
    "name": "João",
    "role": "Instructor"
  }
}
```

#### Logout
```
POST /api/mobile/logout
```

---

### 2. Perfil

#### Obter Perfil
```
GET /api/mobile/profile
```

**Response:**
```json
{
  "id": "...",
  "nome": "João",
  "telefone": "...",
  "fotografia": "...",
  "escola": "ICSN"
}
```

#### Alterar Palavra-passe
```
PUT /api/mobile/profile/password
```

---

### 3. Dashboard

#### Resumo do Dia
```
GET /api/mobile/dashboard
```

**Response:**
```json
{
  "lessonsToday": 4,
  "nextLesson": "09:00",
  "notifications": 2
}
```

---

### 4. Aulas

#### Aulas de Hoje
```
GET /api/mobile/lessons/today
```

**Response:**
```json
[
  {
    "lessonId": 1,
    "student": "Carlos",
    "vehicle": "Toyota Corolla",
    "start": "09:00",
    "status": "Scheduled"
  }
]
```

#### Detalhes da Aula
```
GET /api/mobile/lessons/{id}
```

#### Recursos Disponíveis (antes de iniciar)
```
GET /api/mobile/lessons/{id}/resources
```

**Response:**
```json
{
  "student": {},
  "vehicle": {},
  "route": {},
  "documents": []
}
```

#### Iniciar Aula
```
POST /api/mobile/lessons/{id}/start
```

O backend deve:
- Validar o instrutor
- Validar o aluno
- Validar o veículo
- Mudar estado da aula para `InProgress`
- Guardar hora inicial

#### Terminar Aula
```
POST /api/mobile/lessons/{id}/finish
```

**Request:**
```json
{
  "evaluation": "Good",
  "notes": "Boa evolução.",
  "odometer": 50342
}
```

O backend:
- Grava hora final
- Calcula duração
- Calcula distância
- Altera estado da aula para `Completed`

#### Histórico de Aulas
```
GET /api/mobile/lessons/history
```

---

### 5. Tracking GPS / Localização

#### Enviar Localização
```
POST /api/mobile/tracking/location
```

**Request (chamado automaticamente a cada poucos segundos):**
```json
{
  "lessonId": 10,
  "latitude": -8.814,
  "longitude": 13.230,
  "speed": 32,
  "heading": 180,
  "accuracy": 5,
  "timestamp": "..."
}
```

#### Pausar Partilha
```
POST /api/mobile/tracking/pause
```

#### Retomar Partilha
```
POST /api/mobile/tracking/resume
```

---

### 6. SignalR (Tempo Real)

#### Hub URL
```
/hubs/tracking
```

#### Eventos
| Evento           | Direcção       | Descrição                                   |
|------------------|----------------|---------------------------------------------|
| `JoinLesson`     | Client → Server | Instrutor entra na sessão da aula           |
| `LeaveLesson`    | Client → Server | Instrutor sai da sessão                     |
| `UpdateLocation` | Client → Server | Envio de coordenadas GPS em tempo real      |
| `FinishLesson`   | Client → Server | Sinaliza fim da aula                        |
| `LocationUpdate` | Server → Client | Painel Angular recebe posição actualizada   |

O painel Angular vê o veículo mover-se em tempo real no mapa.

---

### 7. Operações de Frota (Instrutor)

#### Registar Acidente
```
POST /api/mobile/accidents
```

Enviar:
- Fotografias
- Localização
- Descrição

#### Registar Abastecimento
```
POST /api/mobile/fuel
```

#### Registar Manutenção
```
POST /api/mobile/maintenance
```

---

### 8. Notificações

#### Listar Notificações
```
GET /api/mobile/notifications
```

---

## Resumo dos Status de Aula

| Status        | Descrição                        |
|---------------|----------------------------------|
| `Scheduled`   | Aula agendada, aguarda início    |
| `InProgress`  | Aula em curso, GPS activo        |
| `Paused`      | Partilha GPS pausada             |
| `Completed`   | Aula terminada com sucesso       |
| `Cancelled`   | Aula cancelada                   |

---

## Notas de Implementação

- Todos os endpoints `/api/mobile/*` requerem JWT Bearer token no header `Authorization`.
- O `SchoolId` é extraído do token JWT (multi-tenancy automático).
- Prefixo `/api/mobile/` separa claramente os endpoints mobile dos endpoints web.
- O SignalR Hub em `/hubs/tracking` também requer autenticação JWT.
