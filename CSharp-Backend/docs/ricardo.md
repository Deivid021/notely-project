# Ricardo

Sou o assistente de desenvolvimento do projeto Notely. Este documento registra o contexto do que implementei e como penso sobre o código.

---

## O que eu fiz

### Feature: NoteGroups (grupos de notas)

Implementei a entidade `NoteGroup`, que permite ao usuário organizar suas notas em grupos nomeados (ex: "Trabalho", "Pessoal", "Estudos").

**Relação entre entidades:**

```
User (1) ──── (N) NoteGroup (1) ──── (N) Note
```

- Um usuário pode ter vários grupos.
- Uma nota pode pertencer a um grupo ou ficar solta (`NoteGroupId` é nullable).
- Deletar um grupo **não** deleta as notas — elas ficam sem grupo (`SetNull`).
- Deletar um usuário deleta todos os seus grupos e notas em cascata.

---

## Arquivos que criei ou modifiquei

### Novos

| Arquivo | O que é |
|---|---|
| `Models/NoteGroup.cs` | Entidade do domínio |
| `DTOs/NoteGroups/CreateNoteGroupRequest.cs` | DTO de criação |
| `DTOs/NoteGroups/UpdateNoteGroupRequest.cs` | DTO de atualização |
| `DTOs/NoteGroups/NoteGroupResponse.cs` | DTO de resposta (inclui `NoteCount`) |
| `Services/NoteGroupService.cs` | CRUD de grupos |
| `Controllers/NoteGroupsController.cs` | Endpoints REST |
| `Migrations/20260511000001_AddNoteGroups.cs` | Migration manual |

### Modificados

| Arquivo | O que mudou |
|---|---|
| `Models/Note.cs` | `NoteGroupId?` + navigation `NoteGroup?` |
| `Models/User.cs` | Coleção `NoteGroups` |
| `Data/AppDbContext.cs` | DbSet e configurações do novo relacionamento |
| `DTOs/Notes/NoteResponse.cs` | Campo `NoteGroupId?` |
| `DTOs/Notes/CreateNoteRequest.cs` | Campo `NoteGroupId?` |
| `DTOs/Notes/UpdateNoteRequest.cs` | Campo `NoteGroupId?` |
| `Services/NoteService.cs` | Filtro por grupo no `GetAll`, validação de ownership no Create/Update |
| `Controllers/NotesController.cs` | Query param `?groupId=` no GET, tratamento 422 no POST |
| `Program.cs` | Registro do `NoteGroupService` |
| `Migrations/AppDbContextModelSnapshot.cs` | Snapshot atualizado |

---

## Endpoints disponíveis

### NoteGroups

| Método | Rota | Status |
|---|---|---|
| `GET` | `/note-groups` | 200 |
| `GET` | `/note-groups/{id}` | 200 / 404 |
| `POST` | `/note-groups` | 201 |
| `PUT` | `/note-groups/{id}` | 200 / 404 |
| `DELETE` | `/note-groups/{id}` | 204 / 404 |

### Notes (alterações)

| Método | Rota | Mudança |
|---|---|---|
| `GET` | `/notes?groupId={id}` | Filtro opcional por grupo |
| `POST` | `/notes` | Aceita `NoteGroupId?`, retorna 422 se o grupo não pertencer ao usuário |
| `PUT` | `/notes/{id}` | Aceita `NoteGroupId?`, valida ownership do grupo |

---

## Decisões que tomei

**`NoteGroupId` nullable nas notas** — notas existentes não quebram. A feature é aditiva.

**`SetNull` ao deletar grupo** — notas não são perdidas junto com o grupo. O usuário não perde conteúdo.

**`NoteCount` na resposta do grupo** — calculado via subquery do EF Core diretamente no `Select`, sem carregar as notas em memória.

**Validação de ownership do grupo no service** — antes de associar uma nota a um grupo, verifico se `g.UserId == userId`. Impede que um usuário associe suas notas a grupos de outro usuário.

**Migration manual** — o `dotnet-ef` é instável neste ambiente WSL, então a migration foi escrita à mão seguindo o padrão da `InitialCreate`.
