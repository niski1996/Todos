namespace TodosApi.Models.DTOs;

/// <summary>
/// DTO do tworzenia nowego zadania
/// </summary>
public class CreateTaskDto
{
    /// <summary>
    /// Nazwa zadania
    /// </summary>
    /// <example>Zrobić zakupy</example>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// DTO do aktualizacji istniejącego zadania
/// </summary>
public class UpdateTaskDto
{
    /// <summary>
    /// Nowa nazwa zadania
    /// </summary>
    /// <example>Zrobić zakupy w supermarkecie</example>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// DTO odpowiedzi z zadaniem
/// </summary>
public class TaskResponseDto
{
    /// <summary>
    /// Identyfikator zadania
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }
    
    /// <summary>
    /// Nazwa zadania
    /// </summary>
    /// <example>Zrobić zakupy</example>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Data utworzenia zadania
    /// </summary>
    /// <example>2025-08-03T10:00:00</example>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// Czy zadanie zostało wykonane
    /// </summary>
    /// <example>false</example>
    public bool IsCompleted { get; set; }
}
