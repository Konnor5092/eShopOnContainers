using System.Net;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using Ordering.API;
using Ordering.API.Infrastructure;
using Ordering.Infrastructure;
using Platform.IntegrationEventLogEF;
using Serilog;

var configuration = GetConfiguration();

Log.Logger = CreateSerilogLogger(configuration);

var loggerFactory = new LoggerFactory()
    .AddSerilog(CreateSerilogLogger(configuration));

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);

startup.ConfigureServices(builder.Services);

// Using a custom DI container.
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(startup.ConfigureContainer);

var app = builder.Build();

startup.Configure(app, app.Environment, loggerFactory);

app.Run();

// try
// {
//     Log.Information("Configuring web host ({ApplicationContext})...", Program.AppName);
//     var host = BuildWebHost(configuration, args);
//
//     Log.Information("Applying migrations ({ApplicationContext})...", Program.AppName);
//     // host.MigrateDbContext<OrderingContext>((context, services) =>
//     // {
//     //     var env = services.GetService<IWebHostEnvironment>();
//     //     var settings = services.GetService<IOptions<OrderingSettings>>();
//     //     var logger = services.GetService<ILogger<OrderingContextSeed>>();
//     //
//     //     new OrderingContextSeed()
//     //         .SeedAsync(context, env, settings, logger)
//     //         .Wait();
//     // })
//     // .MigrateDbContext<IntegrationEventLogContext>((_, __) => { });
//
//     Log.Information("Starting web host ({ApplicationContext})...", Program.AppName);
//     host.Build().Run();
//
//     return 0;
// }
// catch (Exception ex)
// {
//     Log.Fatal(ex, "Program terminated unexpectedly ({ApplicationContext})!", Program.AppName);
//     return 1;
// }
// finally
// {
//     Log.CloseAndFlush();
// }

// IHostBuilder BuildWebHost(IConfiguration configuration, string[] args)
// {
//     return Host.CreateDefaultBuilder(args)
//         .UseServiceProviderFactory(new AutofacServiceProviderFactory())
//         .ConfigureWebHostDefaults(webBuilder =>
//         {
//             webBuilder
//                 // .CaptureStartupErrors(false)
//                 // .ConfigureKestrel(options =>
//                 // {
//                 //     var ports = GetDefinedPorts(configuration);
//                 //     options.Listen(IPAddress.Any, ports.httpPort, listenOptions =>
//                 //     {
//                 //         listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
//                 //     });
//                 //
//                 //     options.Listen(IPAddress.Any, ports.grpcPort, listenOptions =>
//                 //     {
//                 //         listenOptions.Protocols = HttpProtocols.Http2;
//                 //     });
//                 //
//                 // })
//                 // .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
//                 .UseStartup<Startup>();
//             //.UseSerilog();
//         });
// }


Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
{
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];
    var logstashUrl = configuration["Serilog:LogstashgUrl"];
    return new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.WithProperty("ApplicationContext", Program.AppName)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
        .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl, null)
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

IConfiguration GetConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    var config = builder.Build();

    if (config.GetValue<bool>("UseVault", false))
    {
        TokenCredential credential = new ClientSecretCredential(
            config["Vault:TenantId"],
            config["Vault:ClientId"],
            config["Vault:ClientSecret"]);
        builder.AddAzureKeyVault(new Uri($"https://{config["Vault:Name"]}.vault.azure.net/"), credential);
    }

    return builder.Build();
}

public partial class Program
{
    public static string Namespace = typeof(Startup).Namespace;
    public static string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
}