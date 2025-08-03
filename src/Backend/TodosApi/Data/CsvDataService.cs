using TodosApi.Models;
using System.Globalization;

namespace TodosApi.Data
{
    public interface ICsvDataService
    {
        Task<List<TodoTask>> GetTasksAsync();
        Task SaveTasksAsync(List<TodoTask> tasks);
        Task<List<DailyHistory>> GetHistoryAsync();
        Task SaveDailyHistoryAsync(DailyHistory history);
    }

    public class CsvDataService : ICsvDataService
    {
        private readonly string _dataPath;
        private readonly string _tasksFilePath;
        private readonly string _historyFilePath;

        public CsvDataService(IConfiguration configuration)
        {
            _dataPath = configuration.GetValue<string>("DataPath") ?? "Data";
            _tasksFilePath = Path.Combine(_dataPath, "tasks.csv");
            _historyFilePath = Path.Combine(_dataPath, "history.csv");
            
            // Ensure data directory exists
            Directory.CreateDirectory(_dataPath);
        }

        public async Task<List<TodoTask>> GetTasksAsync()
        {
            var tasks = new List<TodoTask>();

            if (!File.Exists(_tasksFilePath))
            {
                return tasks;
            }

            var lines = await File.ReadAllLinesAsync(_tasksFilePath);
            
            // Skip header if exists
            var dataLines = lines.Skip(1);

            foreach (var line in dataLines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = ParseCsvLine(line);
                if (parts.Length >= 5)
                {
                    tasks.Add(new TodoTask
                    {
                        Id = int.Parse(parts[0]),
                        Name = parts[1],
                        CreatedDate = DateTime.Parse(parts[2]),
                        IsCompleted = bool.Parse(parts[3]),
                        Order = int.Parse(parts[4])
                    });
                }
            }

            return tasks.OrderBy(t => t.Order).ToList();
        }

        public async Task SaveTasksAsync(List<TodoTask> tasks)
        {
            var lines = new List<string>
            {
                "Id,Name,CreatedDate,IsCompleted,Order"
            };

            foreach (var task in tasks)
            {
                var line = $"{task.Id},\"{EscapeCsvValue(task.Name)}\",{task.CreatedDate:yyyy-MM-dd HH:mm:ss},{task.IsCompleted},{task.Order}";
                lines.Add(line);
            }

            await File.WriteAllLinesAsync(_tasksFilePath, lines);
        }

        public async Task<List<DailyHistory>> GetHistoryAsync()
        {
            var history = new List<DailyHistory>();

            if (!File.Exists(_historyFilePath))
            {
                return history;
            }

            var lines = await File.ReadAllLinesAsync(_historyFilePath);
            
            // Skip header if exists
            var dataLines = lines.Skip(1);

            foreach (var line in dataLines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = ParseCsvLine(line);
                if (parts.Length >= 3)
                {
                    history.Add(new DailyHistory
                    {
                        Date = DateTime.Parse(parts[0]),
                        TotalTasks = int.Parse(parts[1]),
                        CompletedTasks = int.Parse(parts[2])
                    });
                }
            }

            return history.OrderByDescending(h => h.Date).ToList();
        }

        public async Task SaveDailyHistoryAsync(DailyHistory history)
        {
            var lines = new List<string>();

            // Read existing history
            if (File.Exists(_historyFilePath))
            {
                lines.AddRange(await File.ReadAllLinesAsync(_historyFilePath));
            }
            else
            {
                lines.Add("Date,TotalTasks,CompletedTasks");
            }

            // Check if entry for this date already exists
            var dateString = history.Date.ToString("yyyy-MM-dd");
            var existingLineIndex = -1;

            for (int i = 1; i < lines.Count; i++)
            {
                if (lines[i].StartsWith(dateString))
                {
                    existingLineIndex = i;
                    break;
                }
            }

            var newLine = $"{history.Date:yyyy-MM-dd},{history.TotalTasks},{history.CompletedTasks}";

            if (existingLineIndex >= 0)
            {
                lines[existingLineIndex] = newLine;
            }
            else
            {
                lines.Add(newLine);
            }

            await File.WriteAllLinesAsync(_historyFilePath, lines);
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"' && (i == 0 || line[i - 1] == ','))
                {
                    inQuotes = true;
                }
                else if (c == '"' && inQuotes && (i == line.Length - 1 || line[i + 1] == ','))
                {
                    inQuotes = false;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        private string EscapeCsvValue(string value)
        {
            if (value.Contains("\""))
            {
                value = value.Replace("\"", "\"\"");
            }
            return value;
        }
    }
}
