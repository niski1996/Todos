using Frontend.Models;
using System.Text.Json;
using System.Text;

namespace Frontend.Services;

public class TodoApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public TodoApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<List<TodoTask>> GetTasksAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/tasks");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TodoTask>>(json, _jsonOptions) ?? new List<TodoTask>();
        }
        catch
        {
            return new List<TodoTask>();
        }
    }

    public async Task<TodoTask?> CreateTaskAsync(CreateTaskDto task)
    {
        try
        {
            var json = JsonSerializer.Serialize(task, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/tasks", content);
            response.EnsureSuccessStatusCode();
            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TodoTask>(responseJson, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UpdateTaskAsync(int id, UpdateTaskDto task)
    {
        try
        {
            var json = JsonSerializer.Serialize(task, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/tasks/{id}", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/tasks/{id}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ToggleTaskAsync(int id)
    {
        try
        {
            var response = await _httpClient.PutAsync($"api/tasks/{id}/toggle", null);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<DailyHistory>> GetHistoryAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/history");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<DailyHistory>>(json, _jsonOptions) ?? new List<DailyHistory>();
        }
        catch
        {
            return new List<DailyHistory>();
        }
    }
}
