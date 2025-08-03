using Microsoft.AspNetCore.Mvc;
using TodosApi.Models.DTOs;
using TodosApi.Services;

namespace TodosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskService taskService, ILogger<TasksController> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        /// <summary>
        /// Get all tasks
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<TaskResponseDto>>> GetTasks()
        {
            try
            {
                var tasks = await _taskService.GetAllTasksAsync();
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tasks");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get task by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
        {
            try
            {
                var task = await _taskService.GetTaskByIdAsync(id);
                if (task == null)
                    return NotFound();

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting task {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Create a new task
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TaskResponseDto>> CreateTask(CreateTaskDto createTaskDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createTaskDto.Name))
                {
                    return BadRequest("Task name is required");
                }

                var task = await _taskService.CreateTaskAsync(createTaskDto);
                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Update task name
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<TaskResponseDto>> UpdateTask(int id, UpdateTaskDto updateTaskDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(updateTaskDto.Name))
                {
                    return BadRequest("Task name is required");
                }

                var task = await _taskService.UpdateTaskAsync(id, updateTaskDto);
                if (task == null)
                    return NotFound();

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Delete task
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTask(int id)
        {
            try
            {
                var result = await _taskService.DeleteTaskAsync(id);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Toggle task completion status
        /// </summary>
        [HttpPut("{id}/toggle")]
        public async Task<ActionResult<TaskResponseDto>> ToggleTask(int id)
        {
            try
            {
                var task = await _taskService.ToggleTaskCompletionAsync(id);
                if (task == null)
                    return NotFound();

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling task {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
