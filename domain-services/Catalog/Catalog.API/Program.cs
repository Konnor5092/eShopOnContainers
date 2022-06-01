using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Linq;
using Autofac.Extensions.DependencyInjection;
using Catalog.API.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Platform.IntegrationEventLogEF;

namespace Catalog.API
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var catalogContext = scope.ServiceProvider.GetRequiredService<CatalogContext>();
                var integrationEventLogContext = scope.ServiceProvider.GetRequiredService<IntegrationEventLogContext>();
                var settings = scope.ServiceProvider.GetService<IOptions<CatalogSettings>>();
                var env = scope.ServiceProvider.GetService<IWebHostEnvironment>();
                var logger = scope.ServiceProvider.GetService<ILogger<CatalogContextSeed>>();

                if (catalogContext.Database.GetPendingMigrations().Any() &&
                    integrationEventLogContext.Database.GetPendingMigrations().Any())
                {
                    catalogContext.Database.Migrate();
                    integrationEventLogContext.Database.Migrate();
                }
                
                new CatalogContextSeed().SeedAsync(catalogContext, env, settings, logger).Wait();
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
    
    public partial class Program
    {
        public static readonly string Namespace = typeof(Startup).Namespace;
        public static readonly string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
    }
}
