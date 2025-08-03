namespace Frontend.Models;

public class DailyHistory
{
    public DateTime Date { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    
    public double CompletionPercentage => TotalTasks > 0 ? Math.Round((double)CompletedTasks / TotalTasks * 100, 1) : 0;
}
