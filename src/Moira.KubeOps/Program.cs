using KubeOps.Operator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moira.Authentik;
using Moira.Common;
using Moira.KubeOps;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("System.Net.Http", LogEventLevel.Information)
    .MinimumLevel.Override("KubeOps.Operator", LogEventLevel.Information)    
    .MinimumLevel.Override("Moira", LogEventLevel.Debug)
    .Enrich.FromLogContext()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();

builder.Services
    .AddMoiraCommon()
    .AddMoiraAuthentikProvider()
    .AddMoiraKubeOps()
    .AddKubernetesOperator()
    .RegisterComponents();

builder.Logging.AddSerilog();

var host = builder.Build();
await host.RunAsync();
