# Setup local

## Pré-requisitos

- Docker e Docker Compose

## Subir o ambiente

```bash
docker compose up --build
```

A API estará disponível em `http://localhost:8080`.

## Variáveis de ambiente

Copie `.env.example` para `.env` e ajuste os valores antes de subir em produção:

```bash
cp .env.example .env
```

| Variável | Padrão | Descrição |
|---|---|---|
| `POSTGRES_USER` | `notely` | Usuário do banco |
| `POSTGRES_PASSWORD` | `notely` | Senha do banco |
| `POSTGRES_DB` | `notely` | Nome do banco |
| `JWT_SECRET` | *(ver .env.example)* | Chave de assinatura JWT — **trocar em produção** |
| `JWT_ISSUER` | `notely-api` | Issuer do token |
| `JWT_AUDIENCE` | `notely-client` | Audience do token |
| `JWT_EXPIRES_HOURS` | `1` | Duração do access token em horas |
| `JWT_REFRESH_TOKEN_EXPIRY_DAYS` | `7` | Duração do refresh token em dias |

## Migrations

As migrations são aplicadas automaticamente no startup da API via `db.Database.Migrate()`. Não é necessário nenhum comando manual.

## Swagger

Disponível em `http://localhost:8080/swagger` apenas quando `ASPNETCORE_ENVIRONMENT=Development`.
