# API Reference

Base URL: `http://localhost:8080`

Swagger UI disponível em `/swagger` (apenas em ambiente de desenvolvimento).

---

## Auth

### POST /auth/register

Cria um novo usuário e retorna um par de tokens.

**Body**
```json
{
  "email": "user@example.com",
  "password": "minimo8chars"
}
```

**Respostas**
| Status | Descrição |
|---|---|
| 200 | `{ "accessToken": "eyJ...", "refreshToken": "..." }` |
| 400 | Validação falhou (email inválido, senha curta) |
| 409 | Email já cadastrado |

---

### POST /auth/login

Autentica um usuário existente e retorna um par de tokens.

**Body**
```json
{
  "email": "user@example.com",
  "password": "minimo8chars"
}
```

**Respostas**
| Status | Descrição |
|---|---|
| 200 | `{ "accessToken": "eyJ...", "refreshToken": "..." }` |
| 400 | Validação falhou |
| 401 | Credenciais inválidas |

---

### POST /auth/refresh

Troca um refresh token válido por um novo par de tokens. O token enviado é imediatamente revogado (token rotation).

**Body**
```json
{
  "refreshToken": "..."
}
```

**Respostas**
| Status | Descrição |
|---|---|
| 200 | `{ "accessToken": "eyJ...", "refreshToken": "..." }` |
| 401 | Token inválido, expirado ou já revogado |

---

### POST /auth/logout

Revoga o refresh token, encerrando a sessão. Sempre retorna 204 independente de o token existir.

**Body**
```json
{
  "refreshToken": "..."
}
```

**Respostas**
| Status | Descrição |
|---|---|
| 204 | Sessão encerrada |

---

## Notes

Todos os endpoints abaixo exigem o header:
```
Authorization: Bearer <accessToken>
```

---

### GET /notes

Lista todas as notas do usuário autenticado, ordenadas por `updatedAt` decrescente.

**Query params**

| Param | Tipo | Descrição |
|---|---|---|
| `groupId` | Guid (opcional) | Filtra notas de um grupo específico |

**Resposta 200**
```json
[
  {
    "id": "uuid",
    "title": "Título",
    "content": "Conteúdo",
    "noteGroupId": "uuid ou null",
    "createdAt": "2026-05-11T00:00:00Z",
    "updatedAt": "2026-05-11T00:00:00Z"
  }
]
```

---

### GET /notes/{id}

Retorna uma nota específica do usuário autenticado.

**Respostas**
| Status | Descrição |
|---|---|
| 200 | Objeto da nota |
| 401 | Sem token |
| 404 | Nota não encontrada ou pertence a outro usuário |

---

### POST /notes

Cria uma nova nota.

**Body**
```json
{
  "title": "Título",
  "content": "Conteúdo",
  "noteGroupId": "uuid (opcional)"
}
```

**Respostas**
| Status | Descrição |
|---|---|
| 201 | Nota criada, com header `Location: /notes/{id}` |
| 400 | Validação falhou |
| 401 | Sem token |
| 422 | `noteGroupId` informado não pertence ao usuário |

---

### PUT /notes/{id}

Atualiza título, conteúdo e grupo de uma nota existente.

**Body**
```json
{
  "title": "Novo título",
  "content": "Novo conteúdo",
  "noteGroupId": "uuid ou null"
}
```

**Respostas**
| Status | Descrição |
|---|---|
| 200 | Nota atualizada |
| 400 | Validação falhou |
| 401 | Sem token |
| 404 | Nota não encontrada ou pertence a outro usuário |
| 422 | `noteGroupId` informado não pertence ao usuário |

---

### DELETE /notes/{id}

Remove uma nota.

**Respostas**
| Status | Descrição |
|---|---|
| 204 | Removida com sucesso |
| 401 | Sem token |
| 404 | Nota não encontrada ou pertence a outro usuário |

---

## NoteGroups

Todos os endpoints abaixo exigem o header:
```
Authorization: Bearer <accessToken>
```

---

### GET /note-groups

Lista todos os grupos do usuário autenticado.

**Resposta 200**
```json
[
  {
    "id": "uuid",
    "name": "Trabalho",
    "description": "...",
    "noteCount": 3,
    "createdAt": "2026-05-11T00:00:00Z",
    "updatedAt": "2026-05-11T00:00:00Z"
  }
]
```

---

### GET /note-groups/{id}

Retorna um grupo específico.

**Respostas**
| Status | Descrição |
|---|---|
| 200 | Objeto do grupo |
| 404 | Grupo não encontrado ou pertence a outro usuário |

---

### POST /note-groups

Cria um novo grupo.

**Body**
```json
{
  "name": "Trabalho",
  "description": "Opcional"
}
```

**Respostas**
| Status | Descrição |
|---|---|
| 201 | Grupo criado, com header `Location: /note-groups/{id}` |
| 400 | Validação falhou |
| 401 | Sem token |

---

### PUT /note-groups/{id}

Atualiza nome e descrição de um grupo.

**Respostas**
| Status | Descrição |
|---|---|
| 200 | Grupo atualizado |
| 404 | Grupo não encontrado ou pertence a outro usuário |

---

### DELETE /note-groups/{id}

Remove um grupo. As notas do grupo **não** são deletadas — ficam sem grupo.

**Respostas**
| Status | Descrição |
|---|---|
| 204 | Removido com sucesso |
| 404 | Grupo não encontrado ou pertence a outro usuário |
