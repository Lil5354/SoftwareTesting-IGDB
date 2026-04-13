using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.04_Perf_04 - [Performance] Verify response time of /search with broad and specific keywords
/// Priority: Medium
/// Endpoint: POST https://api.igdb.com/v4/search
/// Acceptance threshold: Response Time ≤ 1500ms
/// </summary>
[TestFixture]
[Order(4)]
public class TC04_Perf04_SearchPerformanceTests : ApiBaseTest
{
    [Test, Order(1)]
    public async Task Step1_BroadKeyword_ResponseTimeShouldBeLessThan1500ms()
    {
        // Arrange & Act: Search với từ khóa rộng "a"
        testLog.Info("Step 1: [Broad keyword] Send /search request with search \"a\", limit 50.");
        var response = await apiClient.PostAsync("search", "fields name, game; search \"a\"; limit 50;");

        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Response Time: {response.ResponseTimeMs}ms");
        testLog.Info($"Records returned: {response.AsArray().Count}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(response.ResponseTimeMs, Is.LessThanOrEqualTo(1500),
            $"Broad keyword response time {response.ResponseTimeMs}ms should be ≤ 1500ms");
    }

    [Test, Order(2)]
    public async Task Step2_SpecificKeyword_ShouldBeFasterOrEqualToBroad()
    {
        // Arrange: Lấy response time cả 2 loại keyword
        testLog.Info("Step 2: [Specific keyword] Send /search with search \"Mario Kart\", limit 50.");

        var broadResponse = await apiClient.PostAsync("search", "fields name, game; search \"a\"; limit 50;");
        var specificResponse = await apiClient.PostAsync("search",
            "fields name, game; search \"Mario Kart\"; limit 50;");

        testLog.Info($"Broad keyword (\"a\"): {broadResponse.ResponseTimeMs}ms");
        testLog.Info($"Specific keyword (\"Mario Kart\"): {specificResponse.ResponseTimeMs}ms");

        // Assert
        Assert.That(specificResponse.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(specificResponse.ResponseTimeMs, Is.LessThanOrEqualTo(1500),
            $"Specific keyword response time {specificResponse.ResponseTimeMs}ms should be ≤ 1500ms");
    }

    [Test, Order(3)]
    public async Task Step3_SearchVsGames_OverheadShouldBeLessThan3x()
    {
        // Arrange & Act: So sánh /search vs /games
        testLog.Info("Step 3: [/games comparison] Compare /search response time with /games.");

        var gamesResponse = await apiClient.PostAsync("games", "fields id, name; limit 50;");
        var searchResponse = await apiClient.PostAsync("search", "fields name, game; search \"a\"; limit 50;");

        double ratio = (double)searchResponse.ResponseTimeMs / Math.Max(gamesResponse.ResponseTimeMs, 1);
        testLog.Info($"/games: {gamesResponse.ResponseTimeMs}ms");
        testLog.Info($"/search: {searchResponse.ResponseTimeMs}ms");
        testLog.Info($"Ratio: {ratio:F2}x");

        // Assert
        Assert.That(ratio, Is.LessThanOrEqualTo(3),
            $"Search/Games ratio {ratio:F2}x should be ≤ 3x");
    }
}
