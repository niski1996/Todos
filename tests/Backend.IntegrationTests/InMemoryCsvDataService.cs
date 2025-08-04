using TodosApi.Data;
using TodosApi.Models;

namespace Backend.IntegrationTests;

/// <summary>
/// Implementacja in-memory serwisu danych CSV dla testów integracyjnych
/// </summary>
public class InMemoryCsvDataService : ICsvDataService
{
    private readonly List<TodoTask> _tasks = new();
    private readonly List<DailyHistory> _history = new();

    public Task<List<TodoTask>> GetTasksAsync()
    {
        return Task.FromResult(_tasks.ToList());
    }

    public Task SaveTasksAsync(List<TodoTask> tasks)
    {
        _tasks.Clear();
        _tasks.AddRange(tasks);
        return Task.CompletedTask;
    }

    public Task<List<DailyHistory>> GetHistoryAsync()
    {
        return Task.FromResult(_history.ToList());
    }

    public Task SaveDailyHistoryAsync(DailyHistory history)
    {
        var existingEntry = _history.FirstOrDefault(h => h.Date.Date == history.Date.Date);
        if (existingEntry != null)
        {
            _history.Remove(existingEntry);
        }
        _history.Add(history);
        return Task.CompletedTask;
    }

    // Dodatkowe metody pomocnicze dla testów
    public void ClearAllData()
    {
        _tasks.Clear();
        _history.Clear();
    }

    public void SeedTasks(params TodoTask[] tasks)
    {
        _tasks.AddRange(tasks);
    }

    public void SeedHistory(params DailyHistory[] historyEntries)
    {
        _history.AddRange(historyEntries);
    }

    public Task<List<TodoTask>> LoadTasksAsync() => GetTasksAsync();
    public Task SaveTaskAsync(TodoTask task) => SaveTasksAsync(_tasks);
}
