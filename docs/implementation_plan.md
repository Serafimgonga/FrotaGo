# Correcção do Isolamento Multi-Tenant (SchoolId)

## Problema Identificado

A migração para multi-tenancy foi feita a **meio** — a estrutura de dados tem `SchoolId` nos utilizadores, mas as entidades operacionais e os repositórios **não filtram por escola**. Isso significa que um utilizador autenticado da **Escola A** consegue ver os veículos, alunos e instrutores da **Escola B**.

## Diagnóstico Detalhado

### ⚠️ Entidades SEM `SchoolId` (vazamento de dados crítico)

| Entidade | Ficheiro | Estado |
|---|---|---|
| `Vehicle` | `Vehicle.cs` | ❌ Sem `SchoolId` |
| `Student` | `Student.cs` | ❌ Sem `SchoolId` |
| `Instructor` | `Instructor.cs` | ❌ Sem `SchoolId` |
| `Lesson` | `Lesson.cs` | ❌ Sem `SchoolId` (depende de Vehicle/Student/Instructor) |
| `Maintenance` | `Maintenance.cs` | ❌ Sem `SchoolId` |
| `FuelRecord` | `FuelRecord.cs` | ❌ Sem `SchoolId` |
| `VehicleDocument` | `VehicleDocument.cs` | ❌ Sem `SchoolId` |
| `Accident` | `Accident.cs` | ❌ Sem `SchoolId` |

### ⚠️ Queries sem filtro de tenant

- `GetVehiclesQueryHandler` → chama `GetAllAsync()` — **sem filtro de escola**
- `VehiclesController` → chama `GetVehiclesQuery()` — sem passar `SchoolId` do token JWT
- O mesmo padrão repete-se em Students, Instructors, Lessons, etc.

### ✅ O que funciona correctamente
- `User` tem `SchoolId` ✔
- `School` e `Branch` estão isolados ✔
- O token JWT inclui `SchoolId` via `LoginResult` ✔
- `UserRepository.GetBySchoolIdAsync` filtra por escola ✔

---

## User Review Required

> [!CAUTION]
> **Vulnerabilidade de segurança activa.** Qualquer utilizador autenticado consegue neste momento ver dados de outras escolas através da API. Isto precisa ser corrigido antes de qualquer teste com dados reais de múltiplas escolas.

> [!IMPORTANT]
> **A correcção requer uma nova migration de base de dados.** Serão adicionadas colunas `SchoolId` às tabelas `Vehicles`, `Students`, `Instructors`, `Maintenances`, `FuelRecords`, `VehicleDocuments`, `Accidents`. Os dados existentes serão associados à primeira escola encontrada (ou ficarão nulos se a BD estiver vazia).

> [!WARNING]
> **O índice único de `LicensePlate` e `Chassis` em `Vehicle` é global.** Com multi-tenancy, matrículas diferentes em escolas diferentes podem colidir. Será necessário remover o índice único global e substituir por índice único composto `(SchoolId, LicensePlate)`.

---

## Open Questions

> [!IMPORTANT]
> **Estratégia de tenant**: Pretendemos usar **Global Query Filters** no EF Core (filtragem automática e transparente a nível do `DbContext`) ou filtrar **manualmente em cada repositório/handler**? A abordagem com Global Query Filters é mais segura e robusta (não é possível "esquecer" o filtro), mas requer um `ITenantProvider` injectado no `DbContext`.

---

## Proposed Changes

### 1. Domínio — Entidades

#### [MODIFY] `Vehicle.cs`
- Adicionar `Guid SchoolId` e `School? School`

#### [MODIFY] `Student.cs`
- Adicionar `Guid SchoolId` e `School? School`

#### [MODIFY] `Instructor.cs`
- Adicionar `Guid SchoolId` e `School? School`

*(Lesson, Maintenance, FuelRecord, VehicleDocument, Accident seguem o mesmo padrão)*

---

### 2. Infraestrutura — Tenant Provider

#### [NEW] `ITenantProvider.cs` (Application/Interfaces)
```csharp
public interface ITenantProvider
{
    Guid? SchoolId { get; }
}
```

#### [NEW] `HttpContextTenantProvider.cs` (Infrastructure/Authentication)
- Lê o claim `schoolId` do JWT via `IHttpContextAccessor`
- Devolve o `SchoolId` do utilizador autenticado

---

### 3. Infraestrutura — Global Query Filter no DbContext

#### [MODIFY] `ApplicationDbContext.cs`
- Injectar `ITenantProvider`
- Adicionar `HasQueryFilter(e => e.SchoolId == _tenantProvider.SchoolId)` em todas as entidades com `SchoolId`
- Resultado: **filtragem automática e transparente** — todos os `.ToListAsync()` já filtram por escola sem necessidade de alteração nos handlers

---

### 4. Infraestrutura — Migration

#### [NEW] `AddSchoolIdToOperationalEntities` (migration)
- Adicionar `SchoolId` (nullable → depois NOT NULL) às tabelas operacionais
- Remover índices únicos globais (`LicensePlate`, `Chassis`, `Email` de Student/Instructor)
- Criar índices únicos compostos `(SchoolId, LicensePlate)` etc.

---

### 5. Application — Commands/Queries

#### [MODIFY] Todos os `Create*Command` e handlers
- Receber `SchoolId` do `ITenantProvider` (ou do Claims do utilizador) e atribuir à entidade criada
- Exemplo: `CreateVehicleCommandHandler` passa `vehicle.SchoolId = _tenantProvider.SchoolId`

---

### 6. Infrastructure — Repositories

#### [MODIFY] Repositórios (`VehicleRepository`, `StudentRepository`, etc.)
- Com Global Query Filter activo, os repositórios não precisam de alteração
- Remover `GetAllAsync()` sem parâmetros e substituir por `GetAllBySchoolAsync(Guid schoolId)` como fallback de segurança

---

## Verification Plan

### Automated Tests
- Criar teste de integração que:
  1. Regista **Escola A** e cria veículo A1
  2. Regista **Escola B** e cria veículo B1
  3. Autentica como utilizador da Escola A
  4. Chama `GET /api/vehicles`
  5. Verifica que apenas A1 aparece (B1 nunca visível)

### Manual Verification
1. Registar duas escolas no browser via `/register`
2. Criar veículos em cada escola
3. Fazer login com conta de Escola A → verificar que veículos de B não aparecem
4. Confirmar no Swagger/API que todos os endpoints filtram correctamente

---

## Estimativa de Trabalho
- Entidades + Migration: ~30 min
- TenantProvider + DbContext: ~20 min
- Ajuste dos Create handlers: ~30 min
- Testes: ~20 min
- **Total: ~1h30**
