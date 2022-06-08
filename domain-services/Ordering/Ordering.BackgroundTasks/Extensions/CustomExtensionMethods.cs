using Autofac;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Platform.EventBus;
using Platform.EventBus.Abstractions;
using Platform.EventBusServiceBus;
using Serilog;

namespace Ordering.BackgroundTasks.Extensions;

public static class CustomExtensionMethods
{
    public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
    {
        var hcBuilder = services.AddHealthChecks();

        hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

        hcBuilder.AddSqlServer(
            configuration["ConnectionString"],
            name: "OrderingTaskDB-check",
            tags: new string[] {"orderingtaskdb"});

        if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
        {
            hcBuilder.AddAzureServiceBusTopic(
                configuration["EventBusConnection"],
                topicName: "eshop_event_bus",
                name: "orderingtask-servicebus-check",
                tags: new string[] {"servicebus"});
        }

        return services;
    }

    public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        var subscriptionClientName = configuration["SubscriptionClientName"];

        if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
        {
            services.AddSingleton<IServiceBusPersisterConnection>(sp =>
            {
                var serviceBusConnectionString = configuration["EventBusConnection"];

                return new DefaultServiceBusPersisterConnection(serviceBusConnectionString);
            });

            services.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
            {
                var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<EventBusServiceBus>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
                string subscriptionName = configuration["SubscriptionClientName"];

                return new EventBusServiceBus(serviceBusPersisterConnection, logger, eventBusSubcriptionsManager,
                    iLifetimeScope, subscriptionName);
            });
        }
        
        services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

        return services;
    }

    public static ILoggingBuilder UseSerilog(this ILoggingBuilder builder, IConfiguration configuration)
    {
        var seqServerUrl = configuration["Serilog:SeqServerUrl"];
        var logstashUrl = configuration["Serilog:LogstashgUrl"];

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .Enrich.WithProperty("ApplicationContext", Program.AppName)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
            .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl, null)
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        return builder;
    }
}