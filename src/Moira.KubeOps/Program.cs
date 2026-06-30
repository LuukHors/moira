using KubeOps.Operator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Controllers;
using Moira.Common.Services;
using Moira.KubeOps;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .MinimumLevel.Override("System.Net.Http", LogEventLevel.Error)
    .MinimumLevel.Override("KubeOps.Operator", LogEventLevel.Error)    
    .MinimumLevel.Override("Moira", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

builder.Services
    .AddMoiraCommon()
    .AddMoiraAuthentikProvider()
    .AddMoiraKubeOps()
    .AddKubernetesOperator(s => s.Name = "Moira")
    .RegisterComponents();

builder.Logging.AddSerilog();

var host = builder.Build();
await host.RunAsync();
