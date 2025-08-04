using TodosApi.Data;
using TodosApi.Models;

namespace TodosApi.Services
{
    public interface IHistoryService
    {
        Task<List<DailyHistory>> GetHistoryAsync(int? days = null);
        Task<DailyHistory?> GetTodayStatisticsAsync();
        Task SaveTodayStatisticsAsync();
    }

    public class HistoryService : IHistoryService
    {
        private readonly ICsvDataService _csvDataService;
        private readonly ITaskService _taskService;

        public HistoryService(ICsvDataService csvDataService, ITaskService taskService)
        {
            _csvDataService = csvDataService;
            _taskService = taskService;
        }

        public async Task<List<DailyHistory>> GetHistoryAsync(int? days = null)
        {
            var history = await _csvDataService.GetHistoryAsync();
            
            if (days.HasValue)
            {
                if (days.Value == 0)
                {
                    return new List<DailyHistory>();
                }
                else if (days.Value > 0)
                {
                    return history.Take(days.Value).ToList();
                }
                // Dla wartości ujemnych zwracamy całą historię
            }

            return history;
        }

        public async Task<DailyHistory?> GetTodayStatisticsAsync()
        {
            var today = DateTime.Today;
            var history = await _csvDataService.GetHistoryAsync();
            
            return history.FirstOrDefault(h => h.Date.Date == today);
        }

        public async Task SaveTodayStatisticsAsync()
        {
            var tasks = await _taskService.GetAllTasksAsync();
            var today = DateTime.Today;

            var todayHistory = new DailyHistory
            {
                Date = today,
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.IsCompleted)
            };

            await _csvDataService.SaveDailyHistoryAsync(todayHistory);
        }
    }
}
