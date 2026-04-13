using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.02_Perf_02 - [Performance] Verify response time of /games with nested fields and limit 100
/// Priority: High
/// Endpoint: POST https://api.igdb.com/v4/games
/// Body: fields id, name, cover.url, genres.name, platforms.name,
///   involved_companies.company.name, rating, summary, first_release_date; limit 100;
/// Acceptance threshold: Response Time ≤ 2000ms, Response Size ≤ 2MB
/// </summary>
[TestFixture]
[Order(2)]
public class TC02_Perf02_ResponseTimeComplexTests : ApiBaseTest
{
    private const string ComplexBody =
        "fields id, name, cover.url, genres.name, platforms.name, " +
        "involved_companies.company.name, rating, summary, first_release_date; limit 100;";

    [Test, Order(1)]
    public async Task Step1_ComplexQuery_ResponseTimeShouldBeLessThan2000ms()
    {
        // Arrange & Act
        testLog.Info("Step 1: [Complex query] Send a request with 9 fields including nested fields, limit 100.");
        var response = await apiClient.PostAsync("games", ComplexBody);

        // Log kết quả
        var data = response.AsArray();
        var sizeKB = response.Body.Length / 1024.0;
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Response Time: {response.ResponseTimeMs}ms");
        testLog.Info($"Records returned: {data.Count}");
        testLog.Info($"Response Size: {sizeKB:F2} KB");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(response.ResponseTimeMs, Is.LessThanOrEqualTo(2000),
            $"Complex query response time {response.ResponseTimeMs}ms should be ≤ 2000ms");
        Assert.That(data.Count, Is.EqualTo(100), "Should return exactly 100 records");
    }

    [Test, Order(2)]
    public async Task Step2_WarmRequests_ShouldBeLessThan2000ms()
    {
        // Arrange & Act: Gửi 2 warm requests
        testLog.Info("Step 2: [Warm requests] Send 2 additional requests and record response times.");

        for (int i = 1; i <= 2; i++)
        {
            var response = await apiClient.PostAsync("games", ComplexBody);
            testLog.Info($"Warm run #{i}: {response.ResponseTimeMs}ms — HTTP {response.StatusCode}");

            Assert.That(response.StatusCode, Is.EqualTo(200), $"Run #{i} should return HTTP 200");
            Assert.That(response.ResponseTimeMs, Is.LessThanOrEqualTo(2000),
                $"Warm request #{i} response time {response.ResponseTimeMs}ms should be ≤ 2000ms");
        }
    }

    [Test, Order(3)]
    public async Task Step3_BaselineComparison_IncreaseRatioShouldBeLessThan5x()
    {
        // Arrange: Lấy baseline từ simple query
        testLog.Info("Step 3: [Baseline comparison] Compare complex vs simple query response time.");

        var simpleResponse = await apiClient.PostAsync("games", "fields id, name; limit 10;");
        var complexResponse = await apiClient.PostAsync("games", ComplexBody);

        double ratio = (double)complexResponse.ResponseTimeMs / Math.Max(simpleResponse.ResponseTimeMs, 1);
        testLog.Info($"Simple query: {simpleResponse.ResponseTimeMs}ms");
        testLog.Info($"Complex query: {complexResponse.ResponseTimeMs}ms");
        testLog.Info($"Increase ratio: {ratio:F2}x");

        // Assert
        Assert.That(ratio, Is.LessThanOrEqualTo(5),
            $"Complex/Simple ratio {ratio:F2}x should be ≤ 5x");
    }
}
