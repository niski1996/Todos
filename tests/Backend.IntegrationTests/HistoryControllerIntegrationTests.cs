using System.Net;
using TodosApi.Models;
using Bogus;

namespace Backend.IntegrationTests;

/// <summary>
/// Testy integracyjne dla HistoryController
/// </summary>
[TestFixture]
public class HistoryControllerIntegrationTests : IntegrationTestBase
{
    private Faker<DailyHistory> _historyFaker;

    [SetUp]
    public void Setup()
    {
        base.SetUp();
        
        // Konfiguracja Faker dla generowania danych testowych
        _historyFaker = new Faker<DailyHistory>()
            .RuleFor(h => h.Date, f => f.Date.Recent(30).Date) // Tylko data bez czasu
            .RuleFor(h => h.TotalTasks, f => f.Random.Int(1, 10))
            .RuleFor(h => h.CompletedTasks, (f, h) => f.Random.Int(0, h.TotalTasks)); // CompletedTasks <= TotalTasks
    }

    [TearDown]
    public override void TearDown()
    {
        base.TearDown();
    }

    #region GET /api/history

    [Test]
    public async Task GetHistory_WhenNoHistory_ReturnsEmptyList()
    {
        // Act
        var response = await Client.GetAsync("/api/history");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var history = await DeserializeResponse<List<DailyHistory>>(response);
        Assert.That(history, Is.Not.Null);
        Assert.That(history, Is.Empty);
    }

    [Test]
    public async Task GetHistory_WhenHistoryExists_ReturnsAllHistory()
    {
        // Arrange
        var seedHistory = _historyFaker.Generate(5)
            .OrderBy(h => h.Date)
            .ToList();
        
        // Upewnienie się, że daty są unikalne
        for (int i = 0; i < seedHistory.Count; i++)
        {
            seedHistory[i].Date = DateTime.Today.AddDays(-i);
        }
        
        DataService.SeedHistory(seedHistory.ToArray());

        // Act
        var response = await Client.GetAsync("/api/history");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var history = await DeserializeResponse<List<DailyHistory>>(response);
        Assert.That(history, Is.Not.Null);
        Assert.That(history.Count, Is.EqualTo(5));
        
        // Sprawdzenie że historia jest posortowana od najnowszej
        var orderedHistory = history.OrderByDescending(h => h.Date).ToList();
        for (int i = 0; i < history.Count; i++)
        {
            Assert.That(history[i].Date, Is.EqualTo(orderedHistory[i].Date));
        }
    }

    [Test]
    public async Task GetHistory_WithDaysFilter_ReturnsLimitedHistory()
    {
        // Arrange
        var seedHistory = new List<DailyHistory>();
        for (int i = 0; i < 10; i++)
        {
            var historyEntry = _historyFaker.Generate();
            historyEntry.Date = DateTime.Today.AddDays(-i);
            seedHistory.Add(historyEntry);
        }
        
        DataService.SeedHistory(seedHistory.ToArray());

        // Act
        var response = await Client.GetAsync("/api/history?days=5");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var history = await DeserializeResponse<List<DailyHistory>>(response);
        Assert.That(history, Is.Not.Null);
        Assert.That(history.Count, Is.EqualTo(5));
    }

    [Test]
    public async Task GetHistory_WithZeroDaysFilter_ReturnsEmptyList()
    {
        // Arrange
        var seedHistory = _historyFaker.Generate(3);
        DataService.SeedHistory(seedHistory.ToArray());

        // Act
        var response = await Client.GetAsync("/api/history?days=0");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var history = await DeserializeResponse<List<DailyHistory>>(response);
        Assert.That(history, Is.Not.Null);
        Assert.That(history, Is.Empty);
    }

    [Test]
    public async Task GetHistory_WithNegativeDaysFilter_ReturnsAllHistory()
    {
        // Arrange
        var seedHistory = _historyFaker.Generate(3);
        DataService.SeedHistory(seedHistory.ToArray());

        // Act
        var response = await Client.GetAsync("/api/history?days=-1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var history = await DeserializeResponse<List<DailyHistory>>(response);
        Assert.That(history, Is.Not.Null);
        Assert.That(history.Count, Is.EqualTo(3));
    }

    #endregion

    #region GET /api/history/today

    [Test]
    public async Task GetTodayStatistics_WhenNoTasksToday_ReturnsEmptyStatistics()
    {
        // Act
        var response = await Client.GetAsync("/api/history/today");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var todayStats = await DeserializeResponse<DailyHistory>(response);
        Assert.That(todayStats, Is.Not.Null);
        Assert.That(todayStats.Date.Date, Is.EqualTo(DateTime.Today));
        Assert.That(todayStats.TotalTasks, Is.EqualTo(0));
        Assert.That(todayStats.CompletedTasks, Is.EqualTo(0));
        Assert.That(todayStats.CompletionPercentage, Is.EqualTo(0));
    }

    [Test]
    public async Task GetTodayStatistics_WhenTodayHistoryExists_ReturnsTodayStats()
    {
        // Arrange
        var todayHistory = new DailyHistory
        {
            Date = DateTime.Today,
            TotalTasks = 5,
            CompletedTasks = 3
        };
        
        // Dodanie też innych dni żeby sprawdzić czy zwraca tylko dzisiejszy
        var otherHistory = _historyFaker.Generate();
        otherHistory.Date = DateTime.Today.AddDays(-1);
        
        DataService.SeedHistory(todayHistory, otherHistory);

        // Act
        var response = await Client.GetAsync("/api/history/today");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var todayStats = await DeserializeResponse<DailyHistory>(response);
        Assert.That(todayStats, Is.Not.Null);
        Assert.That(todayStats.Date.Date, Is.EqualTo(DateTime.Today));
        Assert.That(todayStats.TotalTasks, Is.EqualTo(5));
        Assert.That(todayStats.CompletedTasks, Is.EqualTo(3));
        Assert.That(todayStats.CompletionPercentage, Is.EqualTo(60.0));
    }

    #endregion

    #region GET /api/history/statistics

    [Test]
    public async Task GetStatistics_WhenNoHistory_ReturnsEmptyStatistics()
    {
        // Act
        var response = await Client.GetAsync("/api/history/statistics");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.Not.Null);
        
        // Deserializacja jako dynamic object żeby sprawdzić strukturę
        var stats = await DeserializeResponse<dynamic>(response);
        Assert.That(stats, Is.Not.Null);
    }

    [Test]
    public async Task GetStatistics_WhenHistoryExists_ReturnsCorrectStatistics()
    {
        // Arrange
        var history1 = new DailyHistory { Date = DateTime.Today.AddDays(-2), TotalTasks = 5, CompletedTasks = 5 }; // 100%
        var history2 = new DailyHistory { Date = DateTime.Today.AddDays(-1), TotalTasks = 4, CompletedTasks = 2 }; // 50%
        var history3 = new DailyHistory { Date = DateTime.Today, TotalTasks = 6, CompletedTasks = 3 }; // 50%
        
        DataService.SeedHistory(history1, history2, history3);

        // Act
        var response = await Client.GetAsync("/api/history/statistics");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var content = await response.Content.ReadAsStringAsync();
        
        // Sprawdzenie czy response zawiera oczekiwane pola
        Assert.That(content, Does.Contain("totalDays"));
        Assert.That(content, Does.Contain("averageCompletionRate"));
        Assert.That(content, Does.Contain("bestDay"));
        Assert.That(content, Does.Contain("totalTasksCompleted"));
        
        // Sprawdzenie wartości
        Assert.That(content, Does.Contain("\"totalDays\":3")); // 3 dni historii
        Assert.That(content, Does.Contain("\"totalTasksCompleted\":10")); // 5+2+3 = 10
    }

    #endregion

    #region Testy scenariuszowe

    [Test]
    public async Task HistoryEndpoints_CompleteWorkflow_WorksCorrectly()
    {
        // Arrange - Dodanie historii dla różnych dni
        var histories = new[]
        {
            new DailyHistory { Date = DateTime.Today.AddDays(-4), TotalTasks = 3, CompletedTasks = 3 },
            new DailyHistory { Date = DateTime.Today.AddDays(-3), TotalTasks = 5, CompletedTasks = 4 },
            new DailyHistory { Date = DateTime.Today.AddDays(-2), TotalTasks = 4, CompletedTasks = 1 },
            new DailyHistory { Date = DateTime.Today.AddDays(-1), TotalTasks = 6, CompletedTasks = 6 },
            new DailyHistory { Date = DateTime.Today, TotalTasks = 2, CompletedTasks = 1 }
        };
        
        DataService.SeedHistory(histories);

        // Act & Assert - Pobranie wszystkich historii
        var allHistoryResponse = await Client.GetAsync("/api/history");
        Assert.That(allHistoryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var allHistory = await DeserializeResponse<List<DailyHistory>>(allHistoryResponse);
        Assert.That(allHistory!.Count, Is.EqualTo(5));

        // Act & Assert - Pobranie ostatnich 3 dni
        var recentHistoryResponse = await Client.GetAsync("/api/history?days=3");
        Assert.That(recentHistoryResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var recentHistory = await DeserializeResponse<List<DailyHistory>>(recentHistoryResponse);
        Assert.That(recentHistory!.Count, Is.EqualTo(3));

        // Act & Assert - Pobranie dzisiejszych statystyk
        var todayResponse = await Client.GetAsync("/api/history/today");
        Assert.That(todayResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var todayStats = await DeserializeResponse<DailyHistory>(todayResponse);
        Assert.That(todayStats!.Date.Date, Is.EqualTo(DateTime.Today));
        Assert.That(todayStats.TotalTasks, Is.EqualTo(2));
        Assert.That(todayStats.CompletedTasks, Is.EqualTo(1));

        // Act & Assert - Pobranie ogólnych statystyk
        var statsResponse = await Client.GetAsync("/api/history/statistics");
        Assert.That(statsResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var statsContent = await statsResponse.Content.ReadAsStringAsync();
        Assert.That(statsContent, Does.Contain("\"totalDays\":5"));
        Assert.That(statsContent, Does.Contain("\"totalTasksCompleted\":15")); // 3+4+1+6+1 = 15
    }

    #endregion
}
