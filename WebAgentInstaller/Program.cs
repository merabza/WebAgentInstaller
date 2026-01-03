using System;
using System.IO;
using System.Reflection;
using ApiKeyIdentity.DependencyInjection;
using ConfigurationEncrypt;
using Figgle.Fonts;
using LibProjectsApi;
using LibProjectsApi.DependencyInjection;
using MediatorTools.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SerilogLogger;
using SignalRMessages.DependencyInjection;
using SwaggerTools.DependencyInjection;
using TestToolsApi.DependencyInjection;
using WindowsServiceTools;

//using AssemblyReference = ApiExceptionHandler.AssemblyReference;

try
{
    Console.WriteLine("Loading...");

    const string appName = "WebAgentInstaller";
    const int versionCount = 1;

    var header = $"{appName} {Assembly.GetEntryAssembly()?.GetName().Version}";
    Console.WriteLine(FiggleFonts.Standard.Render(header));

    //var parameters = new Dictionary<string, string>
    //{
    //    //{ SignalRMessagesInstaller.SignalRReCounterKey, string.Empty },//Allow SignalRReCounterKey
    //    { ConfigurationEncryptInstaller.AppKeyKey, appKey },
    //    { SwaggerInstaller.AppNameKey, appName },
    //    { SwaggerInstaller.VersionCountKey, 1.ToString() }
    //    //{ SwaggerInstaller.UseSwaggerWithJwtBearerKey, string.Empty },//Allow Swagger
    //};

    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        ContentRootPath = AppContext.BaseDirectory, Args = args
    });

    var debugMode = builder.Environment.IsDevelopment();

    builder.Host.UseSerilogLogger(builder.Configuration, debugMode); //+
    builder.Host.UseWindowsServiceOnWindows(debugMode, args); //+

    builder.Configuration.AddConfigurationEncryption(debugMode, "C0090D12-DC4F-4A15-A7F8-A024E241DB1A"); //+

    // @formatter:off
    builder.Services.AddHttpClient()
        .AddSwagger(debugMode, true, versionCount, appName) //+
        .AddApiKeyIdentity(debugMode)
        .AddSignalRMessages(debugMode)
        //.AddSupportToolsServerRepositories(debugMode)
        //.AddSupportToolsServerPersistence(builder.Configuration, debugMode)
        .AddMediator(
            builder.Configuration,
            debugMode,
            AssemblyReference.Assembly);
    //.AddSupportToolsServerApiKeyIdentity(debugMode)
    //.AddAllScopedServiceSupportToolsServerApplication()
    //.AddSupportToolsServerQueryRepositories(debugMode)
    //.AddSupportToolsServerCommandRepositories(debugMode)
    //.AddSupportToolsServerForCommandsDatabase(builder.Configuration, debugMode)
    //.AddSupportToolsServer_Repositories(debugMode)
    //.AddSupportToolsServerDb(builder.Configuration, debugMode);
    // @formatter:on

    //if (!builder.InstallServices(debugMode, args, parameters,

    //        // @formatter:off

    //        //WebSystemTools
    //        AssemblyReference.Assembly, 
    //        ApiKeyIdentity.AssemblyReference.Assembly,
    //        ConfigurationEncrypt.AssemblyReference.Assembly, 
    //        FluentValidationInstaller.AssemblyReference.Assembly,
    //        HttpClientInstaller.AssemblyReference.Assembly, 
    //        SerilogLogger.AssemblyReference.Assembly,
    //        SignalRMessages.AssemblyReference.Assembly, 
    //        SwaggerTools.AssemblyReference.Assembly,
    //        TestToolsApi.AssemblyReference.Assembly, 
    //        WindowsServiceTools.AssemblyReference.Assembly,

    //        //WebAgentShared
    //        LibProjectsApi.AssemblyReference.Assembly,

    //        //WebAgent
    //        LibDatabasesApi.AssemblyReference.Assembly))
    //    return 2;

    //var mediatRSettings = builder.Configuration.GetSection("MediatRLicenseKey");

    //var mediatRLicenseKey = mediatRSettings.Get<string>();

    //builder.Services.AddMediatR(cfg =>
    //{
    //    cfg.LicenseKey = mediatRLicenseKey;
    //    cfg.RegisterServicesFromAssembly(LibProjectsApi.AssemblyReference.Assembly);
    //    cfg.RegisterServicesFromAssembly(LibDatabasesApi.AssemblyReference.Assembly);
    //});

    //// FluentValidationInstaller
    //builder.Services.InstallValidation(LibProjectsApi.AssemblyReference.Assembly);

    // ReSharper disable once using
    using var app = builder.Build();

    // ReSharper disable once RedundantArgumentDefaultValue
    app.UseSwaggerServices(debugMode, versionCount);
    app.UseTestToolsApiEndpoints(debugMode);
    //app.UseSignalRRecounterMessages(debugMode);

    app.UseLibProjectsApi(debugMode);

    //if (!app.UseServices(debugMode))
    //    return 3;

    Log.Information("Directory.GetCurrentDirectory() = {0}", Directory.GetCurrentDirectory());
    Log.Information("AppContext.BaseDirectory = {0}", AppContext.BaseDirectory);

    app.Run();

    Log.Information("Finish");
    return 0;
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}