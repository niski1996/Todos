using Microsoft.AspNetCore.Mvc;
using TodosApi.Models;
using TodosApi.Services;

namespace TodosApi.Controllers
{
    /// <summary>
    /// Kontroler do zarządzania historią wykonanych zadań
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryService _historyService;
        private readonly ILogger<HistoryController> _logger;

        public HistoryController(IHistoryService historyService, ILogger<HistoryController> logger)
        {
            _historyService = historyService;
            _logger = logger;
        }

        /// <summary>
        /// Pobiera historię dziennych wykonań zadań
        /// </summary>
        /// <param name="days">Liczba dni do pobrania (opcjonalne)</param>
        /// <returns>Lista historii dziennych</returns>
        /// <response code="200">Zwraca historię wykonań</response>
        /// <response code="500">Błąd serwera</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<DailyHistory>>> GetHistory([FromQuery] int? days = null)
        {
            try
            {
                var history = await _historyService.GetHistoryAsync(days);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting history");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Pobiera statystyki dzisiejszego dnia
        /// </summary>
        /// <returns>Statystyki dzisiejszego dnia</returns>
        /// <response code="200">Zwraca statystyki dnia</response>
        /// <response code="500">Błąd serwera</response>
        [HttpGet("today")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DailyHistory>> GetTodayStatistics()
        {
            try
            {
                var todayStats = await _historyService.GetTodayStatisticsAsync();
                if (todayStats == null)
                {
                    return Ok(new DailyHistory 
                    { 
                        Date = DateTime.Today, 
                        TotalTasks = 0, 
                        CompletedTasks = 0 
                    });
                }

                return Ok(todayStats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today's statistics");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Get basic statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetStatistics()
        {
            try
            {
                var history = await _historyService.GetHistoryAsync(30); // Last 30 days
                
                if (!history.Any())
                {
                    return Ok(new 
                    {
                        TotalDays = 0,
                        AverageCompletionRate = 0,
                        BestDay = (object?)null,
                        TotalTasksCompleted = 0
                    });
                }

                var averageCompletionRate = history.Average(h => h.CompletionPercentage);
                var bestDay = history.OrderByDescending(h => h.CompletionPercentage).FirstOrDefault();
                var totalTasksCompleted = history.Sum(h => h.CompletedTasks);

                return Ok(new 
                {
                    TotalDays = history.Count,
                    AverageCompletionRate = Math.Round(averageCompletionRate, 2),
                    BestDay = bestDay,
                    TotalTasksCompleted = totalTasksCompleted
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
