using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.01_Perf_01 - [Performance] Verify response time of /games with a simple query
/// Priority: High
/// Endpoint: POST https://api.igdb.com/v4/games
/// Body: fields id, name; limit 10;
/// Acceptance threshold: Response Time ≤ 500ms (cold ≤ 800ms)
/// </summary>
[TestFixture]
[Order(1)]
public class TC01_Perf01_ResponseTimeSimpleTests : ApiBaseTest
{
    [Test, Order(1)]
    public async Task Step1_ColdRequest_ResponseTimeShouldBeLessThan800ms()
    {
        // Arrange & Act: Gửi cold request đầu tiên
        testLog.Info("Step 1: [Cold request] Send the first request and record the response time.");
        var response = await apiClient.PostAsync("games", "fields id, name; limit 10;");

        // Log kết quả
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Response Time: {response.ResponseTimeMs}ms");
        testLog.Info($"Records returned: {response.AsArray().Count}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(response.ResponseTimeMs, Is.LessThanOrEqualTo(800),
            $"Cold request response time {response.ResponseTimeMs}ms should be ≤ 800ms");
    }

    [Test, Order(2)]
    public async Task Step2_WarmRequests_AllShouldReturn200AndBeLessThan500ms()
    {
        // Arrange & Act: Gửi 4 warm requests
        testLog.Info("Step 2: [Warm requests] Send the request 4 more times and record each response time.");

        var times = new List<long>();
        for (int i = 1; i <= 4; i++)
        {
            var response = await apiClient.PostAsync("games", "fields id, name; limit 10;");
            times.Add(response.ResponseTimeMs);

            testLog.Info($"Warm request #{i}: {response.ResponseTimeMs}ms — HTTP {response.StatusCode}");
            Assert.That(response.StatusCode, Is.EqualTo(200), $"Request #{i} should return HTTP 200");
        }

        // Assert: Kiểm tra tất cả warm requests ≤ 500ms
        foreach (var time in times)
        {
            Assert.That(time, Is.LessThanOrEqualTo(500),
                $"Warm request response time {time}ms should be ≤ 500ms");
        }
    }

    [Test, Order(3)]
    public async Task Step3_AverageResponseTime_ShouldBeLessThan500ms()
    {
        // Arrange & Act: Tính trung bình 5 lần chạy
        testLog.Info("Step 3: [Summary] Calculate average response time across 5 runs.");

        var times = new List<long>();
        for (int i = 1; i <= 5; i++)
        {
            var response = await apiClient.PostAsync("games", "fields id, name; limit 10;");
            times.Add(response.ResponseTimeMs);
            testLog.Info($"Run #{i}: {response.ResponseTimeMs}ms");
        }

        var average = times.Average();
        testLog.Info($"Average response time: {average:F1}ms");

        // Assert
        Assert.That(average, Is.LessThanOrEqualTo(500),
            $"Average response time {average:F1}ms should be ≤ 500ms");
    }
}
