using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TodosApi.Data;
using TodosApi.Services;

namespace Backend.IntegrationTests;

/// <summary>
/// Fabryka aplikacji testowej z konfiguracją do testów integracyjnych
/// </summary>
public class TodosApiWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Zastąpienie serwisu CSV testową implementacją w pamięci
            var csvServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ICsvDataService));
            
            if (csvServiceDescriptor != null)
            {
                services.Remove(csvServiceDescriptor);
            }

            // Dodanie testowej implementacji w pamięci
            services.AddSingleton<ICsvDataService, InMemoryCsvDataService>();
            
            // Usunięcie background service dla testów
            var backgroundServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DailyResetService));
            
            if (backgroundServiceDescriptor != null)
            {
                services.Remove(backgroundServiceDescriptor);
            }
        });

        builder.UseEnvironment("Testing");
    }
}
