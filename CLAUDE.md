# CLAUDE.md

## Project overview
Kubernetes operator written in C# using KubeOps. Exposes generic CRDs
(e.g. OidcApplication, Group) and syncs them into external Identity
Providers (IDPs). Currently targets Authentik; additional IDPs will be
added over time.

## Build
dotnet build

## Code style
- File-scoped namespaces on every file.
- Prefer primary constructors over constructor injection boilerplate.
- Use records for DTOs and value objects; classes for services and
  controllers.
- Async/await everywhere. Never use .Result, .Wait(), or
  .GetAwaiter().GetResult().
- Raise errors explicitly. No silent catch blocks, no swallowed
  exceptions. If a call can fail, let it fail loudly.

## Diff discipline
- Make the smallest diff that satisfies the request.
- Do not refactor, rename, or reformat code outside the scope of the
  task.
- Do not add comments, logging, or error handling that was not asked for.

## Architecture
The project uses hexagonal (ports & adapters) architecture. Each IDP lives
in its own set of projects; the operator core never imports IDP-specific code.

```
Moira.KubeOps                  — operator entry point: KubeOps controllers,
                                  reconciliation, secret management, validators
Moira.Common                   — shared utilities and DI extensions;
                                  no IDP-specific code here
Moira.Authentik.Domain         — pure domain entities (no external deps)
Moira.Authentik.Application    — use-case handlers, port interfaces, mappers
Moira.Authentik.Infrastructure — HTTP clients, Authentik API communication,
                                  authentication, health checks
Moira.Authentik.Controllers    — adapters that translate Kubernetes CRDs
                                  to domain models and dispatch to handlers

Dependency direction: KubeOps → Controllers → Application/Domain ← Infrastructure
```

Naming conventions (already used throughout; follow them):
- `*Handler`  — application-layer orchestration (e.g. `AuthentikGroupHandler`)
- `*Port` / `I*Service` — interfaces for infrastructure dependencies
- `*Adapter`  — translates Kubernetes CRDs to domain commands
- `*Mapper`   — data transformation between layers

## Dependency injection
Each project registers its own services in a `DependencyInjectionExtensions.cs`
file via explicit `AddScoped<>` / `AddSingleton<>` calls (not Scrutor assembly
scanning). When adding a new service, add it to the appropriate layer's
extension method. Wire a new IDP module into `Program.cs` with a single
`AddNewIdpProvider()` call.

## Validation
Validators use FluentValidation and live in
`Moira.KubeOps/Entities/Validators/`. Register new validators in
`Moira.KubeOps/DependencyInjectionExtensions.cs` as
`AddScoped<AbstractValidator<TEntity>, TValidator>()`. They are invoked
automatically by the pre-reconcile webhook step via `IValidatorExecutor<T>`.

## Logging
Use Serilog message templates with named properties. Never use string
interpolation in log calls.

```csharp
// correct
logger.LogInformation("Syncing group {DisplayName}", group.Name);

// wrong
logger.LogInformation($"Syncing group {group.Name}");
```

## Adding a new IDP
Mirror the Authentik module structure: Domain → Application (ports +
handlers + mappers) → Infrastructure → Controllers, each with its own
`DependencyInjectionExtensions.cs`. The Controllers layer must implement
`IProviderAdapter<T>` for each entity type it handles. Wire the new module
into `Program.cs` alongside the existing `AddMoiraAuthentikProvider()` call.

## IDP abstraction
All IDP-specific logic must sit behind the existing abstraction layer
already present in this project. Do not add Authentik-specific code
directly into operator reconciliation logic or shared services.
Before introducing a new interface or abstraction, check whether one
already exists for the purpose.

## KubeOps and IDP APIs
If you are unsure how a KubeOps API or an IDP API behaves, stop and ask
before proceeding. If my answer does not resolve the uncertainty,
search the web for the official documentation.

## No tests
Do not generate unit tests or integration tests unless explicitly asked.