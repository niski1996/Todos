using TodosApi.Data;
using TodosApi.Models;
using TodosApi.Models.DTOs;

namespace TodosApi.Services
{
    public interface ITaskService
    {
        Task<List<TaskResponseDto>> GetAllTasksAsync();
        Task<TaskResponseDto?> GetTaskByIdAsync(int id);
        Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto);
        Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto);
        Task<bool> DeleteTaskAsync(int id);
        Task<TaskResponseDto?> ToggleTaskCompletionAsync(int id);
        Task ResetAllTasksAsync();
    }

    public class TaskService : ITaskService
    {
        private readonly ICsvDataService _csvDataService;

        public TaskService(ICsvDataService csvDataService)
        {
            _csvDataService = csvDataService;
        }

        public async Task<List<TaskResponseDto>> GetAllTasksAsync()
        {
            var tasks = await _csvDataService.GetTasksAsync();
            return tasks.Select(MapToDto).ToList();
        }

        public async Task<TaskResponseDto?> GetTaskByIdAsync(int id)
        {
            var tasks = await _csvDataService.GetTasksAsync();
            var task = tasks.FirstOrDefault(t => t.Id == id);
            return task != null ? MapToDto(task) : null;
        }

        public async Task<TaskResponseDto> CreateTaskAsync(CreateTaskDto createTaskDto)
        {
            var tasks = await _csvDataService.GetTasksAsync();
            
            var newId = tasks.Count > 0 ? tasks.Max(t => t.Id) + 1 : 1;
            var newOrder = tasks.Count > 0 ? tasks.Max(t => t.Order) + 1 : 1;

            var newTask = new TodoTask
            {
                Id = newId,
                Name = createTaskDto.Name.Trim(),
                CreatedDate = DateTime.Now,
                IsCompleted = false,
                Order = newOrder
            };

            tasks.Add(newTask);
            await _csvDataService.SaveTasksAsync(tasks);

            return MapToDto(newTask);
        }

        public async Task<TaskResponseDto?> UpdateTaskAsync(int id, UpdateTaskDto updateTaskDto)
        {
            var tasks = await _csvDataService.GetTasksAsync();
            var task = tasks.FirstOrDefault(t => t.Id == id);

            if (task == null)
                return null;

            task.Name = updateTaskDto.Name.Trim();
            await _csvDataService.SaveTasksAsync(tasks);

            return MapToDto(task);
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var tasks = await _csvDataService.GetTasksAsync();
            var task = tasks.FirstOrDefault(t => t.Id == id);

            if (task == null)
                return false;

            tasks.Remove(task);
            await _csvDataService.SaveTasksAsync(tasks);

            return true;
        }

        public async Task<TaskResponseDto?> ToggleTaskCompletionAsync(int id)
        {
            var tasks = await _csvDataService.GetTasksAsync();
            var task = tasks.FirstOrDefault(t => t.Id == id);

            if (task == null)
                return null;

            task.IsCompleted = !task.IsCompleted;
            await _csvDataService.SaveTasksAsync(tasks);

            return MapToDto(task);
        }

        public async Task ResetAllTasksAsync()
        {
            var tasks = await _csvDataService.GetTasksAsync();
            
            foreach (var task in tasks)
            {
                task.IsCompleted = false;
            }

            await _csvDataService.SaveTasksAsync(tasks);
        }

        private static TaskResponseDto MapToDto(TodoTask task)
        {
            return new TaskResponseDto
            {
                Id = task.Id,
                Name = task.Name,
                CreatedDate = task.CreatedDate,
                IsCompleted = task.IsCompleted
            };
        }
    }
}
