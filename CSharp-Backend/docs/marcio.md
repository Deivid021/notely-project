# Marcio

Sou o assistente de desenvolvimento do projeto Notely. Este documento registra o que implementei e as decisões que tomei.

---

## O que eu fiz

### Feature: Infraestrutura de testes

Implementei o projeto `Notely.Tests` cobrindo todos os serviços e controllers, com foco em **reprodutibilidade**: o suite deve passar de forma determinística em qualquer máquina, sem dependência externa (Docker, Postgres local, etc.).

---

## Estrutura

```
tests/
└── Notely.Tests/
    ├── Notely.Tests.csproj
    ├── GlobalUsings.cs
    ├── appsettings.Testing.json
    ├── Common/
    │   ├── Factories/
    │   │   ├── UserFactory.cs
    │   │   ├── NoteFactory.cs
    │   │   └── NoteGroupFactory.cs
    │   └── Fixtures/
    │       ├── IntegrationWebAppFactory.cs
    │       └── AuthHelper.cs
    ├── Services/
    │   ├── AuthServiceTests.cs
    │   ├── NoteServiceTests.cs
    │   └── NoteGroupServiceTests.cs
    └── Controllers/
        ├── AuthControllerTests.cs
        ├── NotesControllerTests.cs
        └── NoteGroupsControllerTests.cs
```

---

## Decisões técnicas

### Pirâmide de testes — duas camadas

| Camada | O que testa | Como |
|---|---|---|
| Services | Lógica de negócio e acesso a dados via EF Core | `UseInMemoryDatabase` com `Guid.NewGuid()` por teste — isolamento total entre testes, sem Docker. |
| Controllers | Roteamento, model binding, status codes, auth middleware JWT, mapeamento de retorno do service para resposta HTTP | `WebApplicationFactory<Program>` com `IAuthService`/`INoteService`/`INoteGroupService` mockados via NSubstitute. Sem DB. |

A premissa é que cada camada testa apenas a sua responsabilidade. O service é testado de ponta a ponta com EF (incluindo as queries reais). O controller é testado isoladamente — não precisa de DB porque só verifica o que ele controla (routing, status, headers).

### IntegrationWebAppFactory com mocks injetados

A factory expõe os três mocks como propriedades públicas (`AuthService`, `NoteService`, `NoteGroupService`). Cada classe de teste implementa `IAsyncLifetime` e chama `factory.ResetMocks()` no `InitializeAsync`, garantindo que setups de um teste não vazem para o próximo.

No `ConfigureWebHost`, a factory:
- Seta `UseEnvironment("Testing")`.
- Remove `DbContextOptions<AppDbContext>`, `AppDbContext` e os três `IXxxService` da DI.
- Injeta os mocks como `Scoped`.

### Guard no `Program.cs`

`db.Database.Migrate()` no startup é skipado quando `Environment == "Testing"`. Isso é necessário porque a factory remove o `AppDbContext` da DI — sem o guard, o startup explodiria tentando resolvê-lo. É também defesa em profundidade: garante que testes nunca acidentalmente toquem o Postgres local.

### AuthHelper — JWT direto, sem hop pelo `/auth/register`

Como o `AuthService` é mockado, não há banco para registrar um usuário. O `AuthHelper.CreateToken(userId)` emite um JWT diretamente, assinado com o mesmo `Jwt:Secret` de `appsettings.Testing.json`. O middleware JWT do ASP.NET valida apenas a assinatura/claims — não precisa de persistência.

### Factories com Bogus

`UserFactory`, `NoteFactory`, `NoteGroupFactory` usam Bogus com locale `pt_BR`. Dados únicos por chamada, evitando colisões mesmo em runs paralelos.

### `public partial class Program { }`

Linha extra no final do `Program.cs` para expor a classe top-level statement ao assembly de testes (`WebApplicationFactory<Program>`).

---

## Cobertura atual

| Componente | Casos | Estratégia |
|---|---|---|
| `AuthService` | 13 | InMemory (inclui `RefreshAsync` e `RevokeAsync`) |
| `NoteService` | 8 | InMemory |
| `NoteGroupService` | 8 | InMemory |
| `AuthController` | 8 | Mock + WebApplicationFactory |
| `NotesController` | 11 | Mock + WebApplicationFactory |
| `NoteGroupsController` | 9 | Mock + WebApplicationFactory |

Total: **60 testes**. Tempo de execução: ~3 segundos.

---

## Como rodar

```bash
dotnet test tests/Notely.Tests
```

Não precisa de Docker, não precisa de Postgres local. O suite roda em qualquer ambiente com .NET 8 SDK instalado.

Cobertura:

```bash
dotnet test tests/Notely.Tests \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:Exclude="[Notely.Tests]*"
```

---

## Arquivos modificados no projeto principal

- `src/Notely.Api/Program.cs` — guard no `Migrate()` para o ambiente `Testing`; `public partial class Program { }` no final.
- `src/Notely.Api/Services/I*.cs` — interfaces extraídas (`IAuthService`, `INoteService`, `INoteGroupService`). Os controllers já dependiam das implementações concretas; passaram a depender das interfaces, o que viabiliza o mock na DI.
