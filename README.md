# MinimalApiOrg

Projeto de estudos de **ASP.NET Core Minimal APIs** (.NET 10). O foco não é um domínio de produção, e sim organizar uma API pequena de forma que `Program.cs` continue curto e cada responsabilidade fique em um lugar claro.

A API expõe um CRUD de todos em memória, um endpoint de health e um recurso de categorias usado só para demonstrar versionamento.

## Stack

- ASP.NET Core Minimal APIs (`net10.0`)
- [Asp.Versioning.Http](https://github.com/dotnet/aspnet-api-versioning) para versionar rotas
- [Microsoft.AspNetCore.OpenApi](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi) para gerar o contrato OpenAPI
- [Scalar](https://scalar.com/) como UI de documentação em Development

Não há banco de dados. Os todos vivem em um `ConcurrentDictionary` na memória do processo e somem quando a API reinicia.

## Arquitetura

O desenho segue uma **composição por extension methods** em cima do host ASP.NET Core. Não é Clean Architecture completa, DDD nem camadas de repositório: é um recorte didático para estudar Minimal API organizada.

```
HTTP
  → Program.cs (orquestra o host)
    → Extensions (pipeline, DI, mapa de rotas)
      → Endpoints (handlers HTTP)
        → Services (regras e estado em memória)
          → Models / Configurations
```

### `Program.cs` como compositor

O arquivo de entrada só monta o host. Não declara rota, não registra serviço concreto e não configura middleware:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder
    .AddArchitectures()
    .AddServices();

var app = builder.Build();

app.UseArchitectures().MapEndpoints();

app.Run();
```

Essa separação deixa o bootstrap legível e empurra detalhes para pastas com responsabilidade única.

### Pastas e papéis

| Pasta | Papel |
| --- | --- |
| `Extensions/` | Encaixa peças no `WebApplicationBuilder` e no `WebApplication`. |
| `Endpoints/` | Agrupa rotas por recurso (health, categories, todos). |
| `Services/` | Estado e operações de domínio simples (`TodoStore`). |
| `Models/` | Entidade e records de request. |
| `Configurations/` | Options bindadas de `appsettings.json`. |

`Usings.cs` importa `MinimalApiOrg.Extensions` de forma global, então `Program.cs` chama os métodos de extensão sem `using` extra.

### Composition root (`Extensions`)

Três arquivos montam o host:

1. **`BuilderExtensions.AddArchitectures`**  
   Registra infraestrutura transversal: controllers (ainda presentes no pipeline) e OpenAPI.

2. **`BuilderExtensions.AddServices`**  
   Registra o que a aplicação usa de fato:
   - `AppSettings` via Options Pattern (`IOptions<AppSettings>`)
   - versionamento de API (versão padrão `1.0`, leitor no header `api-version`)
   - `TodoStore` como **singleton** (necessário porque o dicionário vive no processo)

3. **`AppExtensions.UseArchitectures`**  
   Liga o pipeline HTTP. Em Development mapeia OpenAPI e o Scalar.

4. **`EndpointExtensions.MapEndpoints`**  
   Único lugar que conhece os prefixes das rotas e chama cada módulo:

   - `/` — health
   - `/api/categories` — categorias (v1 e v2)
   - `/api/todos` — CRUD de todos

### Endpoints como fatias verticais

Cada recurso vive em uma classe estática com `Map*Endpoints`. O padrão se repete:

1. `app.NewVersionedApi("Nome")` cria o grupo versionado.
2. `MapGroup(path).HasApiVersion(...)` amarra a versão.
3. Handlers privados recebem dependências por **parameter binding** (`TodoStore`, `IOptions<AppSettings>`).

Isso substitui controllers com actions: a rota aponta direto para um método estático que devolve `IResult` (`Results.Ok`, `NotFound`, `BadRequest`, `Created`).

**Todos (v1)** — CRUD em memória:

| Método | Rota | Ação |
| --- | --- | --- |
| `GET` | `/api/todos` | Lista todos |
| `GET` | `/api/todos/{id}` | Busca por id |
| `POST` | `/api/todos` | Cria (title obrigatório) |
| `PUT` | `/api/todos/{id}` | Atualiza title e `IsDone` |
| `POST` | `/api/todos/{id}/complete` | Marca como concluído |
| `DELETE` | `/api/todos/{id}` | Remove |

**Categories** — mesmo path, duas versões de contrato:

- v1 devolve um array de strings.
- v2 devolve objetos `{ id, name }`.

Serve para treinar o cliente a escolher a versão sem mudar a URL.

### Versionamento

A versão **não** vai na URL (`/v1/...`). O leitor configurado é `HeaderApiVersionReader("api-version")`.

Exemplos:

```bash
curl http://localhost:5151/api/categories
curl -H "api-version: 1.0" http://localhost:5151/api/categories
curl -H "api-version: 2.0" http://localhost:5151/api/categories
```

Se o header não vier, a API assume `1.0` (`AssumeDefaultVersionWhenUnspecified`). A resposta também reporta as versões suportadas (`ReportApiVersions`).

### Persistência em memória

`TodoStore` encapsula um `ConcurrentDictionary<Guid, TodoModels>`. Fica singleton no DI para todas as requests compartilharem a mesma coleção.

Escolha didática: não há EF Core, transação nem persistência. O ponto de estudo é o fluxo HTTP → handler → serviço, não o banco.

### Configuração

`appsettings.json` traz a seção `AppSettings` (`ApplicationName`, `Version`). O health em `/` lê esses valores via `IOptions<AppSettings>` e devolve também o ambiente (`Development`, `Production`, etc.).

## Como rodar

Pré-requisito: SDK .NET 10.

```bash
dotnet run
```

Por padrão o perfil HTTP sobe em `http://localhost:5151`.

Em Development:

- OpenAPI: `http://localhost:5151/openapi/v1.json`
- Scalar: `http://localhost:5151/scalar`

## Exemplo rápido

```bash
# health
curl http://localhost:5151/

# criar todo
curl -X POST http://localhost:5151/api/todos \
  -H "Content-Type: application/json" \
  -d '{"title":"Estudar Minimal APIs"}'

# listar
curl http://localhost:5151/api/todos
```

## O que este recorte não cobre (de propósito)

- banco, migrations, autenticação
- testes automatizados
- camadas de repositório / Unit of Work
- CQRS, MediatR ou vertical slice completo com pastas por feature

O próximo passo natural de estudo seria extrair persistência real e testes dos endpoints, sem inflar o `Program.cs`.
