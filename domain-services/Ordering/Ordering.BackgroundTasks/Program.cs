using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Ordering.BackgroundTasks.Extensions;

namespace Ordering.BackgroundTasks;

public class Program
{
    public static readonly string AppName = typeof(Program).Assembly.GetName().Name;
    
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddCustomHealthCheck(builder.Configuration)
            .Configure<BackgroundTaskSettings>(builder.Configuration)
            .AddOptions()
            ;

        var app = builder.Build();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
            {
                Predicate = r => r.Name.Contains("self")
            });
        });

        app.Run();
    }
}