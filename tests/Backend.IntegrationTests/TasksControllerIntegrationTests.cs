using System.Net;
using TodosApi.Models;
using TodosApi.Models.DTOs;
using Bogus;

namespace Backend.IntegrationTests;

/// <summary>
/// Testy integracyjne dla TasksController
/// </summary>
[TestFixture]
public class TasksControllerIntegrationTests : IntegrationTestBase
{
    private Faker<TodoTask> _taskFaker;
    private Faker<CreateTaskDto> _createTaskFaker;
    private Faker<UpdateTaskDto> _updateTaskFaker;

    [SetUp]
    public void Setup()
    {
        base.SetUp();
        
        // Konfiguracja Faker dla generowania danych testowych
        _taskFaker = new Faker<TodoTask>()
            .RuleFor(t => t.Id, f => f.Random.Int(1, 1000))
            .RuleFor(t => t.Name, f => f.Lorem.Sentence(3, 5))
            .RuleFor(t => t.CreatedDate, f => f.Date.Recent(30))
            .RuleFor(t => t.IsCompleted, f => f.Random.Bool());

        _createTaskFaker = new Faker<CreateTaskDto>()
            .RuleFor(t => t.Name, f => f.Lorem.Sentence(3, 5));

        _updateTaskFaker = new Faker<UpdateTaskDto>()
            .RuleFor(t => t.Name, f => f.Lorem.Sentence(3, 5));
    }

    [TearDown]
    public override void TearDown()
    {
        base.TearDown();
    }

    #region GET /api/tasks

    [Test]
    public async Task GetTasks_WhenNoTasks_ReturnsEmptyList()
    {
        // Act
        var response = await Client.GetAsync("/api/tasks");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var tasks = await DeserializeResponse<List<TaskResponseDto>>(response);
        Assert.That(tasks, Is.Not.Null);
        Assert.That(tasks, Is.Empty);
    }

    [Test]
    public async Task GetTasks_WhenTasksExist_ReturnsAllTasks()
    {
        // Arrange
        var seedTasks = _taskFaker.Generate(3);
        DataService.SeedTasks(seedTasks.ToArray());

        // Act
        var response = await Client.GetAsync("/api/tasks");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var tasks = await DeserializeResponse<List<TaskResponseDto>>(response);
        Assert.That(tasks, Is.Not.Null);
        Assert.That(tasks.Count, Is.EqualTo(3));
        
        // Sprawdzenie że zadania zostały zwrócone w kolejności utworzenia
        for (int i = 0; i < seedTasks.Count; i++)
        {
            Assert.That(tasks[i].Id, Is.EqualTo(seedTasks[i].Id));
            Assert.That(tasks[i].Name, Is.EqualTo(seedTasks[i].Name));
            Assert.That(tasks[i].IsCompleted, Is.EqualTo(seedTasks[i].IsCompleted));
        }
    }

    #endregion

    #region GET /api/tasks/{id}

    [Test]
    public async Task GetTask_WhenTaskExists_ReturnsTask()
    {
        // Arrange
        var task = _taskFaker.Generate();
        DataService.SeedTasks(task);

        // Act
        var response = await Client.GetAsync($"/api/tasks/{task.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var returnedTask = await DeserializeResponse<TaskResponseDto>(response);
        Assert.That(returnedTask, Is.Not.Null);
        Assert.That(returnedTask.Id, Is.EqualTo(task.Id));
        Assert.That(returnedTask.Name, Is.EqualTo(task.Name));
        Assert.That(returnedTask.IsCompleted, Is.EqualTo(task.IsCompleted));
    }

    [Test]
    public async Task GetTask_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await Client.GetAsync("/api/tasks/999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    #endregion

    #region POST /api/tasks

    [Test]
    public async Task CreateTask_WithValidData_CreatesTaskAndReturnsCreated()
    {
        // Arrange
        var createTaskDto = _createTaskFaker.Generate();

        // Act
        var response = await PostAsJsonAsync("/api/tasks", createTaskDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        
        var createdTask = await DeserializeResponse<TaskResponseDto>(response);
        Assert.That(createdTask, Is.Not.Null);
        Assert.That(createdTask.Id, Is.GreaterThan(0));
        Assert.That(createdTask.Name, Is.EqualTo(createTaskDto.Name));
        Assert.That(createdTask.IsCompleted, Is.False);
        Assert.That(createdTask.CreatedDate, Is.LessThanOrEqualTo(DateTime.Now));

        // Sprawdzenie że task został zapisany
        var tasks = await DataService.LoadTasksAsync();
        Assert.That(tasks.Count, Is.EqualTo(1));
        Assert.That(tasks[0].Name, Is.EqualTo(createTaskDto.Name));
    }

    [Test]
    public async Task CreateTask_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto { Name = "" };

        // Act
        var response = await PostAsJsonAsync("/api/tasks", createTaskDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateTask_WithNullName_ReturnsBadRequest()
    {
        // Arrange
        var createTaskDto = new CreateTaskDto { Name = null! };

        // Act
        var response = await PostAsJsonAsync("/api/tasks", createTaskDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    #endregion

    #region PUT /api/tasks/{id}

    [Test]
    public async Task UpdateTask_WithValidData_UpdatesTaskAndReturnsOk()
    {
        // Arrange
        var task = _taskFaker.Generate();
        DataService.SeedTasks(task);
        
        var updateTaskDto = _updateTaskFaker.Generate();

        // Act
        var response = await PutAsJsonAsync($"/api/tasks/{task.Id}", updateTaskDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var updatedTask = await DeserializeResponse<TaskResponseDto>(response);
        Assert.That(updatedTask, Is.Not.Null);
        Assert.That(updatedTask.Id, Is.EqualTo(task.Id));
        Assert.That(updatedTask.Name, Is.EqualTo(updateTaskDto.Name));
        Assert.That(updatedTask.IsCompleted, Is.EqualTo(task.IsCompleted)); // Status pozostaje bez zmian

        // Sprawdzenie że task został zaktualizowany w storage
        var tasks = await DataService.LoadTasksAsync();
        var savedTask = tasks.FirstOrDefault(t => t.Id == task.Id);
        Assert.That(savedTask, Is.Not.Null);
        Assert.That(savedTask.Name, Is.EqualTo(updateTaskDto.Name));
    }

    [Test]
    public async Task UpdateTask_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateTaskDto = _updateTaskFaker.Generate();

        // Act
        var response = await PutAsJsonAsync("/api/tasks/999", updateTaskDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task UpdateTask_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var task = _taskFaker.Generate();
        DataService.SeedTasks(task);
        
        var updateTaskDto = new UpdateTaskDto { Name = "" };

        // Act
        var response = await PutAsJsonAsync($"/api/tasks/{task.Id}", updateTaskDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    #endregion

    #region DELETE /api/tasks/{id}

    [Test]
    public async Task DeleteTask_WhenTaskExists_DeletesTaskAndReturnsNoContent()
    {
        // Arrange
        var task = _taskFaker.Generate();
        DataService.SeedTasks(task);

        // Act
        var response = await Client.DeleteAsync($"/api/tasks/{task.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Sprawdzenie że task został usunięty
        var tasks = await DataService.LoadTasksAsync();
        Assert.That(tasks.Any(t => t.Id == task.Id), Is.False);
    }

    [Test]
    public async Task DeleteTask_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await Client.DeleteAsync("/api/tasks/999");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    #endregion

    #region PUT /api/tasks/{id}/toggle

    [Test]
    public async Task ToggleTask_WhenTaskExists_TogglesStatusAndReturnsOk()
    {
        // Arrange
        var task = _taskFaker.Generate();
        var originalStatus = task.IsCompleted;
        DataService.SeedTasks(task);

        // Act
        var response = await Client.PutAsync($"/api/tasks/{task.Id}/toggle", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var toggledTask = await DeserializeResponse<TaskResponseDto>(response);
        Assert.That(toggledTask, Is.Not.Null);
        Assert.That(toggledTask.Id, Is.EqualTo(task.Id));
        Assert.That(toggledTask.IsCompleted, Is.EqualTo(!originalStatus));

        // Sprawdzenie że status został zmieniony w storage
        var tasks = await DataService.LoadTasksAsync();
        var savedTask = tasks.FirstOrDefault(t => t.Id == task.Id);
        Assert.That(savedTask, Is.Not.Null);
        Assert.That(savedTask.IsCompleted, Is.EqualTo(!originalStatus));
    }

    [Test]
    public async Task ToggleTask_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await Client.PutAsync("/api/tasks/999/toggle", null);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    #endregion

    #region Testy scenariuszowe

    [Test]
    public async Task TaskLifecycle_CreateUpdateToggleDelete_WorksCorrectly()
    {
        // Arrange
        var createDto = _createTaskFaker.Generate();

        // Act & Assert - Create
        var createResponse = await PostAsJsonAsync("/api/tasks", createDto);
        Assert.That(createResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        
        var createdTask = await DeserializeResponse<TaskResponseDto>(createResponse);
        Assert.That(createdTask, Is.Not.Null);
        Assert.That(createdTask.Name, Is.EqualTo(createDto.Name));
        Assert.That(createdTask.IsCompleted, Is.False);

        // Act & Assert - Update
        var updateDto = _updateTaskFaker.Generate();
        var updateResponse = await PutAsJsonAsync($"/api/tasks/{createdTask.Id}", updateDto);
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var updatedTask = await DeserializeResponse<TaskResponseDto>(updateResponse);
        Assert.That(updatedTask!.Name, Is.EqualTo(updateDto.Name));

        // Act & Assert - Toggle
        var toggleResponse = await Client.PutAsync($"/api/tasks/{createdTask.Id}/toggle", null);
        Assert.That(toggleResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var toggledTask = await DeserializeResponse<TaskResponseDto>(toggleResponse);
        Assert.That(toggledTask!.IsCompleted, Is.True);

        // Act & Assert - Delete
        var deleteResponse = await Client.DeleteAsync($"/api/tasks/{createdTask.Id}");
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        // Verify task is deleted
        var getResponse = await Client.GetAsync($"/api/tasks/{createdTask.Id}");
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    #endregion
}
