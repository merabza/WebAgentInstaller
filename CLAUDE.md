# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

WebAgentInstaller is an ASP.NET Core (.NET 10) Web API that acts as a remote **deployment agent** installed on a target server. Over an API-key-protected HTTP API it installs/updates a program from a file storage (e.g. a Synology NAS), registers it as a Windows service, starts/stops/removes that service, and reports the installed app/settings versions. The host itself can run as a Windows Service. Progress of long-running operations is streamed back to the caller over SignalR.

## Multi-repository build — read first

This repository does **not** build on its own. It is the thin web-host front-end of a multi-repo system and references sibling repos by **relative path** (`..\..\WebAgentShared\...`, `..\..\WebSystemTools\...`, etc. — see `WebAgentInstaller.slnx` and the `.csproj`). All sibling repos must be cloned next to this one under a common parent folder:

```
<parent>/
  ConnectionTools/      DatabaseTools/        ParametersManagement/
  SystemTools/          ToolsManagement/      WebAgentContracts/
  WebAgentShared/       WebSystemTools/       WebAgentInstaller/   <-- this repo
```

Clone them all (note: the README omits `DatabaseTools`, but the solution references it):

```bash
git clone git@github.com:merabza/ConnectionTools.git
git clone git@github.com:merabza/DatabaseTools.git
git clone git@github.com:merabza/ParametersManagement.git
git clone git@github.com:merabza/SystemTools.git
git clone git@github.com:merabza/ToolsManagement.git
git clone git@github.com:merabza/WebAgentContracts.git
git clone git@github.com:merabza/WebAgentShared.git
git clone git@github.com:merabza/WebSystemTools.git
git clone git@github.com:merabza/WebAgentInstaller.git
```

The actual deployment engine lives in the siblings (chiefly `ToolsManagement.Installer` and `WebAgentShared.LibProjectsApi`), not in this host project.

## Commands

Requires the **.NET SDK 10** (the `.slnx` solution format and the `net10.0` target framework both need it; `dotnet --list-sdks` should show a 10.x).

- Build everything: `dotnet build WebAgentInstaller.slnx`
- Build/run the host: `dotnet run --project WebAgentInstaller\WebAgentInstaller\WebAgentInstaller.csproj`
- Restore only: `dotnet restore WebAgentInstaller.slnx`
- Run from a published service: the host honors `--console` / Windows Service hosting via `UseWindowsServiceOnWindows`.

There are **no automated test projects** in this solution — "verifying" a change means it builds clean and the API behaves as expected against Swagger.

When running, Kestrel listens on **http://*:5032** (see `appsettings.json`) and Swagger UI is served at **`/swagger`** (the launch profile opens it automatically).

### Builds are strict

`Directory.Build.props` turns on `TreatWarningsAsErrors`, SonarAnalyzer.CSharp with `AnalysisMode=All`, and `EnforceCodeStyleInBuild`. **Analyzer and code-style violations fail the build.** Match the existing style enforced by `.editorconfig`: file-scoped namespaces, `using` directives outside the namespace with `System.*` sorted first, explicit accessibility modifiers, language keywords over BCL type names (`string` not `String`), and required braces. NuGet package versions are managed centrally in `Directory.Packages.props` (`ManagePackageVersionsCentrally`) — add versions there, not in individual `.csproj` files.

## Architecture

The host (`WebAgentInstaller/WebAgentInstaller/Program.cs`) is intentionally thin: it composes feature "installers" exposed as DI extension methods by the sibling libraries (`AddSwagger`, `AddApiKeyIdentity`, `AddSignalRMessages`, `AddMediator`, `AddApplication`, then `UseSwaggerServices`, `UseSignalRMessagesHub`, `UseLibProjectsApi`). To change what the agent does, you almost always edit a sibling library, not `Program.cs`.

Request flow (CQRS over MediatR):

```
HTTP request (API key required)
  → ProjectsEndpoints (WebAgentShared.LibProjectsApi/Endpoints/V1)   minimal-API handlers
  → request DTO .AdaptTo(userName) → Command/Query  (Mapster mappers)
  → MediatR Send → *CommandHandler / *QueryHandler  (FluentValidation runs in the pipeline)
  → ToolsManagement.Installer engine: ProjectManagersFactory.CreateAgentClientWithFileStorage(...)
                                      → agentClient.InstallService / Start / Stop / Remove
  → returns OneOf<TResult, Error[]>  → TypedResults.Ok / BadRequest(Error[])
```

Key conventions and cross-cutting pieces:

- **Result type:** handlers return `OneOf<T, Error[]>` (the OneOf library), never throw for expected failures. Endpoints `.Match(...)` it into `Ok`/`BadRequest`. Domain errors are `Error` values defined in `ProjectsErrors`.
- **Auth:** every endpoint group is `.RequireAuthorization()` and the caller is resolved from the API key via `ICurrentUserByApiKey` (`WebSystemTools.ApiKeyIdentity`). Keys are configured under `ApiKeys:AppSettingsByApiKey` in `appsettings.json`.
- **Progress streaming:** handlers/endpoints push human-readable progress with `IMessagesDataManager.SendMessage(userName, ...)`, delivered to clients over the SignalR hub.
- **Configuration is the database.** There is no DB for the agent's own state; it reads typed settings from `IConfiguration` at request time: `InstallerSettings.Create(_config)` and `AppSettings.Create(_config)` (models live in `ParametersManagement`). Important sections in `appsettings.json`:
  - `InstallerSettings` — work/install folders, archive date masks & extensions, and `ProgramExchangeFileStorageName` (which `FileStorages` entry to pull program archives from).
  - `AppSettings:FileStorages` — named file storages (e.g. `Synology` with path/user/password) used as the program/parameters exchange.
  - `AppSettings:SmartSchemas` — backup retention schemas (`PreserveCount` per `PeriodType`).
- **API routes** are constants in `WebAgentContracts.WebAgentProjectsApiContracts/V1/Routes/ProjectsApiRoutes.cs` (base `api/v1/projects`). Never hardcode route strings in endpoints — reference the contract constants so client and server stay in sync.

### Adding or changing an endpoint

A single agent operation is spread across the contract repo and `LibProjectsApi`. To add one, follow the existing pattern (e.g. `UpdateService`) end to end:

1. **Contract** (`WebAgentContracts.WebAgentProjectsApiContracts`): add the route constant in `ProjectsApiRoutes` and the request DTO under `V1/Requests`.
2. **Command/Query** (`LibProjectsApi/CommandRequests` or `QueryRequests`): a record implementing `ICommandHandler<,>`/query marker, usually with a `Create(...)` factory or a Mapster mapper from the DTO (`*CommandRequestMapper` / `.AdaptTo`).
3. **Validator** (`LibProjectsApi/Validators`): FluentValidation rules — it runs automatically in the MediatR pipeline.
4. **Handler** (`LibProjectsApi/Handlers`): the logic; return `OneOf<T, Error[]>`, add new failure reasons to `ProjectsErrors`.
5. **Endpoint** (`LibProjectsApi/Endpoints/V1/ProjectsEndpoints.cs`): map the route in `UseProjectsEndpoints` and write the minimal-API method (resolve `ICurrentUserByApiKey`, send the message, `mediator.Send`, `.Match` the result).
