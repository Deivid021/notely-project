# Yeat

Sou o assistente de desenvolvimento do projeto Notely. Este documento registra o que implementei e as decisões que tomei.

---

## O que eu fiz

### Feature: Refresh Token (autenticação de longa duração)

Implementei o sistema de refresh token, permitindo que o cliente obtenha novos access tokens sem precisar fazer login novamente. O access token passou de 24h para 1h de duração; o refresh token dura 7 dias.

**Fluxo:**

```
Login / Register
    └── retorna { accessToken (1h), refreshToken (7d) }

POST /auth/refresh
    └── valida refreshToken → revoga o antigo → emite novo par de tokens

POST /auth/logout
    └── revoga o refreshToken → sessão encerrada
```

---

## Arquivos que criei ou modifiquei

### Novos

| Arquivo | O que é |
|---|---|
| `Models/RefreshToken.cs` | Entidade do domínio |
| `DTOs/Auth/RefreshRequest.cs` | DTO com o campo `RefreshToken` |
| `Migrations/20260511000002_AddRefreshTokens.cs` | Migration manual |

### Modificados

| Arquivo | O que mudou |
|---|---|
| `Models/User.cs` | Coleção `RefreshTokens` |
| `Data/AppDbContext.cs` | DbSet + Fluent API (PK, índice único em `TokenHash`, FK cascade) |
| `appsettings.json` | `ExpiresHours` de 24 → 1; adicionado `RefreshTokenExpiryDays: 7` |
| `DTOs/Auth/AuthResponse.cs` | Campo `Token` renomeado para `AccessToken`; adicionado `RefreshToken` |
| `Services/AuthService.cs` | `RegisterAsync`/`LoginAsync` emitem par de tokens; novos métodos `RefreshAsync` e `RevokeAsync` |
| `Controllers/AuthController.cs` | Novos endpoints `POST /auth/refresh` e `POST /auth/logout` |
| `Migrations/AppDbContextModelSnapshot.cs` | Snapshot atualizado |
| `tests/.../AuthHelper.cs` | `body.Token` → `body.AccessToken` |
| `tests/.../AuthServiceTests.cs` | Asserções atualizadas + config `RefreshTokenExpiryDays` |
| `tests/.../AuthControllerTests.cs` | Asserções atualizadas |

---

## Endpoints disponíveis

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/auth/refresh` | Troca refresh token por novo par de tokens |
| `POST` | `/auth/logout` | Revoga o refresh token |

---

## Decisões que tomei

**Token bruto nunca vai ao banco** — apenas o hash SHA-256 é persistido em `TokenHash`. Se o banco for comprometido, os tokens não podem ser usados.

**Token gerado com `RandomNumberGenerator`** — criptograficamente seguro, 32 bytes (256 bits de entropia), codificado em Base64.

**Token rotation** — a cada `/auth/refresh` o token antigo é revogado e um novo é emitido. Isso invalida tokens roubados que tentem ser reutilizados.

**`/auth/logout` sempre retorna 204** — não revela se o token existia ou não no banco, evitando enumeração.

**Migration manual** — o `dotnet-ef` é instável neste ambiente WSL, então a migration foi escrita à mão seguindo o padrão das anteriores.

**Correção de bug no `RegisterAsync`** — a checagem de email duplicado comparava `u.Email == req.Email` mas o armazenamento usava `req.Email.ToLowerInvariant()`. Corrigido para `u.Email == req.Email.ToLowerInvariant()`, alinhando a busca com o dado persistido.
