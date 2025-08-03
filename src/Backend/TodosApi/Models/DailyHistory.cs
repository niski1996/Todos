namespace TodosApi.Models;

/// <summary>
/// Reprezentuje historię dziennych wykonań zadań
/// </summary>
public class DailyHistory
{
    /// <summary>
    /// Data dla której są statystyki
    /// </summary>
    /// <example>2025-08-03</example>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Łączna liczba zadań w tym dniu
    /// </summary>
    /// <example>5</example>
    public int TotalTasks { get; set; }
    
    /// <summary>
    /// Liczba wykonanych zadań w tym dniu
    /// </summary>
    /// <example>3</example>
    public int CompletedTasks { get; set; }
    
    /// <summary>
    /// Procent wykonanych zadań
    /// </summary>
    /// <example>60.0</example>
    public double CompletionPercentage => TotalTasks > 0 ? Math.Round((double)CompletedTasks / TotalTasks * 100, 1) : 0;
}
