using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;

namespace Backend.IntegrationTests;

/// <summary>
/// Klasa bazowa dla wszystkich testów integracyjnych
/// </summary>
public abstract class IntegrationTestBase
{
    protected readonly TodosApiWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly InMemoryCsvDataService DataService;

    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    protected IntegrationTestBase()
    {
        Factory = new TodosApiWebApplicationFactory();
        Client = Factory.CreateClient();
        
        // Pobranie referencji do testowego serwisu danych
        DataService = Factory.Services.GetRequiredService<TodosApi.Data.ICsvDataService>() as InMemoryCsvDataService
            ?? throw new InvalidOperationException("Unable to get InMemoryCsvDataService from DI container");
    }

    /// <summary>
    /// Czyszczenie danych przed każdym testem
    /// </summary>
    protected virtual void SetUp()
    {
        DataService.ClearAllData();
    }

    [TearDown]
    public virtual void TearDown()
    {
        DataService.ClearAllData();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Client?.Dispose();
        Factory?.Dispose();
    }

    /// <summary>
    /// Pomocnicza metoda do deserializacji odpowiedzi JSON
    /// </summary>
    protected async Task<T?> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    /// <summary>
    /// Pomocnicza metoda do wysyłania JSON POST
    /// </summary>
    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
    {
        return await Client.PostAsJsonAsync(requestUri, value, JsonOptions);
    }

    /// <summary>
    /// Pomocnicza metoda do wysyłania JSON PUT
    /// </summary>
    protected async Task<HttpResponseMessage> PutAsJsonAsync<T>(string requestUri, T value)
    {
        return await Client.PutAsJsonAsync(requestUri, value, JsonOptions);
    }
}
