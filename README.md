# WebAgentInstaller

**Languages / ენა:** [English](#english) · [ქართული](#ქართული)

---

## English

### Overview

**WebAgentInstaller** is an ASP.NET Core (.NET 10) Web API that runs as a remote **deployment agent** on a target server. Through an API-key-protected HTTP API it can:

- install or update a program from a shared file storage (e.g. a Synology NAS) and register it as a **Windows service**;
- **start**, **stop** and **remove** that service;
- report the installed **program version** and **app-settings version**;
- stream the progress of long-running operations back to the caller over **SignalR**.

The host itself can be installed and run as a Windows Service.

### Requirements

- **.NET SDK 10** (required by the `.slnx` solution format and the `net10.0` target framework).
- Windows is recommended, as service installation uses Windows Service hosting.

### Getting the full source (multi-repository)

This repository is the thin web host of a multi-repo system and references its sibling repositories by relative path. **It does not build on its own** — clone every repository into a common parent folder:

```bash
mkdir WebAgentInstaller
cd WebAgentInstaller
git clone git@github.com:merabza/ConnectionTools.git        ConnectionTools
git clone git@github.com:merabza/DatabaseTools.git          DatabaseTools
git clone git@github.com:merabza/ParametersManagement.git   ParametersManagement
git clone git@github.com:merabza/SystemTools.git            SystemTools
git clone git@github.com:merabza/ToolsManagement.git        ToolsManagement
git clone git@github.com:merabza/WebAgentContracts.git      WebAgentContracts
git clone git@github.com:merabza/WebAgentShared.git         WebAgentShared
git clone git@github.com:merabza/WebSystemTools.git         WebSystemTools
git clone git@github.com:merabza/WebAgentInstaller.git      WebAgentInstaller
cd ..
```

The actual deployment engine lives in the sibling repositories (mainly `ToolsManagement` and `WebAgentShared`), while this repository only hosts and wires them together.

### Build & run

```bash
# build the whole solution
dotnet build WebAgentInstaller.slnx

# run the agent (Swagger UI opens at http://localhost:5032/swagger)
dotnet run --project WebAgentInstaller/WebAgentInstaller/WebAgentInstaller.csproj
```

By default Kestrel listens on **http://*:5032**. Builds are strict: warnings are treated as errors and SonarAnalyzer plus `.editorconfig` code style are enforced, so a clean build means no analyzer or style violations.

### Configuration

Configuration is provided through `appsettings.json`; there is no separate database for the agent's own state. Key sections:

| Section | Purpose |
| --- | --- |
| `Kestrel:Endpoints` | HTTP listening address/port. |
| `ApiKeys:AppSettingsByApiKey` | API keys (and optional source IP) that may call the API. |
| `InstallerSettings` | Work/install folders, archive date masks & extensions, and `ProgramExchangeFileStorageName` (which file storage to pull program archives from). |
| `AppSettings:FileStorages` | Named file storages (path/user/password), e.g. `Synology`, used for program and parameter exchange. |
| `AppSettings:SmartSchemas` | Backup retention schemas (`PreserveCount` per `PeriodType`). |

### API

All endpoints require a valid API key and are grouped under `api/v1/projects`:

| Method | Route | Description |
| --- | --- | --- |
| `POST` | `/update` | Update a project. |
| `POST` | `/updateservice` | Install/update a project as a Windows service. |
| `POST` | `/updatesettings` | Update a project's settings file. |
| `POST` | `/startservice/{projectName}/{environmentName}` | Start a service. |
| `POST` | `/stop/{projectName}/{environmentName}` | Stop a service. |
| `DELETE` | `/removeprojectservice/{projectName}/{environmentName}/{isService}` | Remove a project/service. |
| `GET` | `/getversion/{serverSidePort}/{apiVersionId}` | Get the installed program version. |
| `GET` | `/getappsettingsversion/{serverSidePort}/{apiVersionId}` | Get the installed app-settings version. |

### License

See [LICENSE](LICENSE).

---

## ქართული

### მიმოხილვა

**WebAgentInstaller** არის ASP.NET Core (.NET 10) ვების API, რომელიც სამიზნე სერვერზე მუშაობს როგორც დისტანციური **განთავსების აგენტი** (deployment agent). API-გასაღებით დაცული HTTP API-ის საშუალებით მას შეუძლია:

- პროგრამის ინსტალაცია ან განახლება საერთო ფაილსაცავიდან (მაგ. Synology NAS) და მისი რეგისტრაცია **Windows-სერვისად**;
- სერვისის **გაშვება**, **გაჩერება** და **წაშლა**;
- დაყენებული **პროგრამის ვერსიის** და **პარამეტრების (app-settings) ვერსიის** დაბრუნება;
- ხანგრძლივი ოპერაციების მიმდინარეობის რეალურ დროში დაბრუნება გამომძახებელთან **SignalR**-ის გავლით.

თავად ჰოსტი შესაძლებელია დაყენდეს და გაეშვას როგორც Windows-სერვისი.

### მოთხოვნები

- **.NET SDK 10** (საჭიროა `.slnx` სოლუშენის ფორმატისა და `net10.0` სამიზნე ფრეიმვორქისთვის).
- რეკომენდებულია Windows, ვინაიდან სერვისის ინსტალაცია Windows Service-ის ჰოსტინგს იყენებს.

### სრული კოდის მიღება (მრავალ-რეპოზიტორიანი)

ეს რეპოზიტორი არის მრავალ-რეპოზიტორიანი სისტემის მსუბუქი ვებ-ჰოსტი და მეზობელ რეპოზიტორებს მიმართავს ფარდობითი მისამართებით. **ის დამოუკიდებლად არ აგებულდება (build)** — დააკლონირეთ ყველა რეპოზიტორი ერთ საერთო საქაღალდეში:

```bash
mkdir WebAgentInstaller
cd WebAgentInstaller
git clone git@github.com:merabza/ConnectionTools.git        ConnectionTools
git clone git@github.com:merabza/DatabaseTools.git          DatabaseTools
git clone git@github.com:merabza/ParametersManagement.git   ParametersManagement
git clone git@github.com:merabza/SystemTools.git            SystemTools
git clone git@github.com:merabza/ToolsManagement.git        ToolsManagement
git clone git@github.com:merabza/WebAgentContracts.git      WebAgentContracts
git clone git@github.com:merabza/WebAgentShared.git         WebAgentShared
git clone git@github.com:merabza/WebSystemTools.git         WebSystemTools
git clone git@github.com:merabza/WebAgentInstaller.git      WebAgentInstaller
cd ..
```

განთავსების რეალური ლოგიკა მეზობელ რეპოზიტორებშია (ძირითადად `ToolsManagement` და `WebAgentShared`), ეს რეპოზიტორი კი მხოლოდ მათ ჰოსტინგსა და ერთმანეთთან დაკავშირებას ემსახურება.

### აგება და გაშვება

```bash
# მთლიანი სოლუშენის აგება
dotnet build WebAgentInstaller.slnx

# აგენტის გაშვება (Swagger UI გაიხსნება მისამართზე http://localhost:5032/swagger)
dotnet run --project WebAgentInstaller/WebAgentInstaller/WebAgentInstaller.csproj
```

ნაგულისხმევად Kestrel უსმენს მისამართს **http://*:5032**. აგება მკაცრია: გაფრთხილებები (warnings) განიხილება შეცდომებად და გააქტიურებულია SonarAnalyzer და `.editorconfig`-ის კოდის სტილი, ამიტომ წარმატებული აგება ნიშნავს, რომ ანალიზატორის ან სტილის დარღვევები არ არსებობს.

### კონფიგურაცია

კონფიგურაცია მიეწოდება `appsettings.json`-ის საშუალებით; აგენტს საკუთარი მდგომარეობისთვის ცალკე ბაზა არ აქვს. ძირითადი სექციები:

| სექცია | დანიშნულება |
| --- | --- |
| `Kestrel:Endpoints` | HTTP-ის მოსმენის მისამართი/პორტი. |
| `ApiKeys:AppSettingsByApiKey` | API-გასაღებები (და სურვილისამებრ წყაროს IP), რომლებსაც API-ის გამოძახება შეუძლიათ. |
| `InstallerSettings` | სამუშაო/ინსტალაციის საქაღალდეები, არქივის თარიღის მასკები და გაფართოებები, და `ProgramExchangeFileStorageName` (რომელი ფაილსაცავიდან წამოიღება პროგრამის არქივები). |
| `AppSettings:FileStorages` | სახელდებული ფაილსაცავები (მისამართი/მომხმარებელი/პაროლი), მაგ. `Synology`, პროგრამისა და პარამეტრების გაცვლისთვის. |
| `AppSettings:SmartSchemas` | ბექაფის შენახვის სქემები (`PreserveCount` თითო `PeriodType`-ზე). |

### API

ყველა ენდფოინთი მოითხოვს მოქმედ API-გასაღებს და გაერთიანებულია მისამართის ქვეშ `api/v1/projects`:

| მეთოდი | მისამართი | აღწერა |
| --- | --- | --- |
| `POST` | `/update` | პროექტის განახლება. |
| `POST` | `/updateservice` | პროექტის ინსტალაცია/განახლება Windows-სერვისად. |
| `POST` | `/updatesettings` | პროექტის პარამეტრების ფაილის განახლება. |
| `POST` | `/startservice/{projectName}/{environmentName}` | სერვისის გაშვება. |
| `POST` | `/stop/{projectName}/{environmentName}` | სერვისის გაჩერება. |
| `DELETE` | `/removeprojectservice/{projectName}/{environmentName}/{isService}` | პროექტის/სერვისის წაშლა. |
| `GET` | `/getversion/{serverSidePort}/{apiVersionId}` | დაყენებული პროგრამის ვერსიის მიღება. |
| `GET` | `/getappsettingsversion/{serverSidePort}/{apiVersionId}` | დაყენებული პარამეტრების ვერსიის მიღება. |

### ლიცენზია

იხ. [LICENSE](LICENSE).
