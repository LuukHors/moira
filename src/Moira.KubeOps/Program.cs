using KubeOps.Operator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moira.Authentik.Kubernetes;
using Moira.Common.Kubernetes;
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

var operatorBuilder = builder.Services.AddKubernetesOperator(s => s.Name = "Moira");

builder.Services
    .AddMoiraCommonKubernetes()
    .AddMoiraAuthentik(operatorBuilder);

builder.Logging.AddSerilog();

var host = builder.Build();
await host.RunAsync();