# Arquitetura

## Stack

| Camada | Tecnologia |
|---|---|
| Framework | ASP.NET Core 8 (Web API) |
| ORM | Entity Framework Core 8 |
| Banco de dados | PostgreSQL 17 |
| Auth | JWT Bearer (access token) + Refresh Token |
| Hash de senha | BCrypt |
| Hash de token | SHA-256 |
| Documentação | Swagger (Swashbuckle) |
| Container | Docker + Docker Compose |

## Estrutura de pastas

```
src/Notely.Api/
├── Controllers/      # Parsing de request e respostas HTTP
├── Services/         # Lógica de negócio
├── Data/
│   ├── AppDbContext.cs
│   └── Migrations/
├── Models/           # Entidades do banco
└── DTOs/             # Contratos de entrada e saída da API
    ├── Auth/
    ├── Notes/
    └── NoteGroups/
```

## Fluxo de dados

```
Request → Controller → Service → AppDbContext → PostgreSQL
```

## Modelos

### User
| Campo | Tipo | Observação |
|---|---|---|
| Id | Guid | PK |
| Email | string(256) | unique, lowercase |
| PasswordHash | string | BCrypt |
| CreatedAt | DateTime | UTC |

### Note
| Campo | Tipo | Observação |
|---|---|---|
| Id | Guid | PK |
| UserId | Guid | FK → User, cascade delete |
| NoteGroupId | Guid? | FK → NoteGroup, set null on delete |
| Title | string(255) | |
| Content | string | |
| CreatedAt | DateTime | UTC |
| UpdatedAt | DateTime | UTC |

### NoteGroup
| Campo | Tipo | Observação |
|---|---|---|
| Id | Guid | PK |
| UserId | Guid | FK → User, cascade delete |
| Name | string(100) | |
| Description | string(500)? | |
| CreatedAt | DateTime | UTC |
| UpdatedAt | DateTime | UTC |

### RefreshToken
| Campo | Tipo | Observação |
|---|---|---|
| Id | Guid | PK |
| UserId | Guid | FK → User, cascade delete |
| TokenHash | string(64) | SHA-256 do token bruto, unique |
| ExpiresAt | DateTime | UTC |
| CreatedAt | DateTime | UTC |
| IsRevoked | bool | |

## Autenticação

- Login e registro retornam `{ accessToken, refreshToken }`
- **Access token**: JWT de 1h — carrega claims `sub` (userId) e `email`
- **Refresh token**: 7 dias de validade, armazenado como hash SHA-256 no banco
- A cada `/auth/refresh` o token antigo é revogado e um novo par é emitido (token rotation)
- Todos os endpoints de `/notes` e `/note-groups` exigem `Authorization: Bearer <accessToken>`
- Dados são sempre filtrados pelo `UserId` extraído do token — um usuário nunca enxerga dados de outro

---

## Funcionalidades e responsáveis

| Funcionalidade | Responsável | Detalhes |
|---|---|---|
| API base (auth, notes, modelos iniciais) | Equipe | — |
| Grupos de notas (`NoteGroups`) | Ricardo | [ricardo.md](ricardo.md) |
| Infraestrutura de testes | Marcio | [marcio.md](marcio.md) |
| Refresh token | Yeat | [yeat.md](yeat.md) |
