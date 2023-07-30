using System;
using System.IO;
using FluentValidationInstaller;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SystemToolsShared;
using WebInstallers;

//პროგრამის ატრიბუტების დაყენება 
ProgramAttributes.Instance.SetAttribute("AppName", "WebAgentInstaller");
ProgramAttributes.Instance.SetAttribute("VersionCount", 1);
ProgramAttributes.Instance.SetAttribute("UseSwaggerWithJWTBearer", false);
ProgramAttributes.Instance.SetAttribute("AppKey", "C0090D12-DC4F-4A15-A7F8-A024E241DB1A");

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args
});

builder.InstallServices(args,
    TestToolsApi.AssemblyReference.Assembly,
    LibProjectsApi.AssemblyReference.Assembly,
    ConfigurationEncrypt.AssemblyReference.Assembly,
    SerilogLogger.AssemblyReference.Assembly,
    WindowsServiceTools.AssemblyReference.Assembly,
    SwaggerTools.AssemblyReference.Assembly,
    ApiExceptionHandler.AssemblyReference.Assembly
);

builder.Services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(LibProjectsApi.AssemblyReference.Assembly); });

// FluentValidationInstaller
builder.Services.InstallValidation(LibProjectsApi.AssemblyReference.Assembly);

var app = builder.Build();

app.UseServices();


try
{
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