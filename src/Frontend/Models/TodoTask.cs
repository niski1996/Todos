namespace Frontend.Models;

public class TodoTask
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsCompleted { get; set; }
}
