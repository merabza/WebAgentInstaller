using System;
using System.IO;
using System.Reflection;
using Figgle.Fonts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SystemTools.SystemToolsShared.DependencyInjection;
using WebAgentShared.LibProjectsApi;
using WebAgentShared.LibProjectsApi.DependencyInjection;
using WebSystemTools.ApiKeyIdentity.DependencyInjection;
using WebSystemTools.MediatorTools.DependencyInjection;
using WebSystemTools.SerilogLogger;
using WebSystemTools.SignalRMessages.DependencyInjection;
using WebSystemTools.SignalRMessages.Endpoints.V1;
using WebSystemTools.SwaggerTools.DependencyInjection;
using WebSystemTools.TestToolsApi.DependencyInjection;
using WebSystemTools.WindowsServiceTools;

try
{
    Console.WriteLine("Loading...");

    const string appName = "WebAgentInstaller";
    const int versionCount = 1;

    string header = $"{appName} {Assembly.GetEntryAssembly()?.GetName().Version}";
    Console.WriteLine(FiggleFonts.Standard.Render(header));

    WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        ContentRootPath = AppContext.BaseDirectory, Args = args
    });

    bool debugMode = builder.Environment.IsDevelopment();

    ILogger logger = builder.Host.UseSerilogLogger(debugMode, builder.Configuration);
    ILogger? debugLogger = debugMode ? logger : null;

    builder.Host.UseWindowsServiceOnWindows(debugLogger, args);

    // @formatter:off
    builder.Services.AddHttpClient()
        .AddSwagger(debugLogger, true, versionCount, appName) //+
        .AddApiKeyIdentity(debugLogger)
        .AddSignalRMessages(debugLogger)
        .AddMediator(debugLogger,
            builder.Configuration,
            AssemblyReference.Assembly)
        .AddApplication(x =>
        {
            x.AppName = appName;
        });

    // ReSharper disable once using
    await using WebApplication app = builder.Build();

    // ReSharper disable once RedundantArgumentDefaultValue
    app.UseSwaggerServices(debugLogger, versionCount);
    app.UseTestToolsApiEndpoints(debugLogger);
    app.UseSignalRMessagesHub(debugLogger);

    app.UseLibProjectsApi(debugLogger);

    Log.Information("Directory.GetCurrentDirectory() = {0}", Directory.GetCurrentDirectory());
    Log.Information("AppContext.BaseDirectory = {0}", AppContext.BaseDirectory);

    await app.RunAsync();

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
    await Log.CloseAndFlushAsync();
}
