namespace Backend.IntegrationTests;

/// <summary>
/// Podstawowe testy weryfikujące konfigurację testów integracyjnych
/// </summary>
[TestFixture]
public class BasicIntegrationTests : IntegrationTestBase
{
    [SetUp]
    public void Setup()
    {
        base.SetUp();
    }

    [TearDown]
    public override void TearDown()
    {
        base.TearDown();
    }

    [Test]
    public async Task HealthCheck_ApiResponds_ReturnsSuccess()
    {
        // Act
        var response = await Client.GetAsync("/api/tasks");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }

    [Test]
    public async Task SwaggerEndpoint_IsAccessible_ReturnsSuccess()
    {
        // Act
        var response = await Client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Does.Contain("Todos API"));
    }

    [Test]
    public void InMemoryDataService_IsConfiguredCorrectly_CanStoreAndRetrieveData()
    {
        // Arrange
        var testTask = new TodosApi.Models.TodoTask
        {
            Id = 1,
            Name = "Test Task",
            CreatedDate = DateTime.Now,
            IsCompleted = false
        };

        // Act
        DataService.SeedTasks(testTask);
        var tasks = DataService.GetTasksAsync().Result;

        // Assert
        Assert.That(tasks.Count, Is.EqualTo(1));
        Assert.That(tasks[0].Name, Is.EqualTo("Test Task"));
    }
}