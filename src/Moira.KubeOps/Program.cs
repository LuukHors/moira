using KubeOps.Operator;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moira.Authentik;
using Moira.Common;
using Moira.KubeOps;
using Serilog;
using Serilog.Events;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Fatal)
    .WriteTo.Console()
    .CreateLogger();

builder.Services
    .AddMoiraKubeOps()
    .AddAuthentikProvider()
    .AddCommon()
    .AddKubernetesOperator()
    .RegisterComponents();

builder.Logging.AddSerilog();

var host = builder.Build();
await host.RunAsync();