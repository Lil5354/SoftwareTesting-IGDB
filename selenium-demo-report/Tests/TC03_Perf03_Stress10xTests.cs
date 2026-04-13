using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.03_Perf_03 - [Performance] Verify stability when sending 10 consecutive requests without delay
/// Priority: Critical
/// Endpoint: POST https://api.igdb.com/v4/games
/// Body: fields id, name, rating; limit 20;
/// Acceptance threshold: 10/10 HTTP 200, no request > 1500ms
/// </summary>
[TestFixture]
[Order(3)]
public class TC03_Perf03_Stress10xTests : ApiBaseTest
{
    private const string Body = "fields id, name, rating; limit 20;";
    private const int Iterations = 10;

    [Test, Order(1)]
    public async Task Step1_AllRequestsShouldReturn200_NoRateLimiting()
    {
        // Arrange & Act: Chạy 10 request liên tục
        testLog.Info($"Step 1: [Run Collection Runner] Send {Iterations} consecutive requests, Delay=0ms.");

        var statusCodes = new List<int>();
        for (int i = 1; i <= Iterations; i++)
        {
            var response = await apiClient.PostAsync("games", Body);
            statusCodes.Add(response.StatusCode);
            testLog.Info($"Request #{i}: HTTP {response.StatusCode} — {response.ResponseTimeMs}ms");
        }

        // Assert: Tất cả đều HTTP 200, không có 429 hoặc 5xx
        foreach (var code in statusCodes)
        {
            Assert.That(code, Is.EqualTo(200), "All requests should return HTTP 200");
            Assert.That(code, Is.Not.EqualTo(429), "No rate limiting (429)");
            Assert.That(code, Is.Not.AnyOf(500, 502, 503), "No server errors (5xx)");
        }
    }

    [Test, Order(2)]
    public async Task Step2_ResponseTimeAnalysis_MaxShouldBeLessThan1500ms()
    {
        // Arrange & Act: Measure min, max, average
        testLog.Info($"Step 2: [Response time analysis] Record min, max, and average of {Iterations} requests.");

        var times = new List<long>();
        for (int i = 1; i <= Iterations; i++)
        {
            var response = await apiClient.PostAsync("games", Body);
            times.Add(response.ResponseTimeMs);
        }

        var min = times.Min();
        var max = times.Max();
        var avg = times.Average();
        testLog.Info($"Min: {min}ms, Max: {max}ms, Average: {avg:F1}ms");

        // Assert
        Assert.That(max, Is.LessThanOrEqualTo(1500),
            $"Max response time {max}ms should be ≤ 1500ms");
        Assert.That(avg, Is.LessThanOrEqualTo(800),
            $"Average response time {avg:F1}ms should be ≤ 800ms");
    }

    [Test, Order(3)]
    public async Task Step3_SpikeCheck_ResponseBodyShouldBeComplete()
    {
        // Arrange & Act: Tìm request có spike time cao nhất, kiểm tra body
        testLog.Info("Step 3: [Spike check] Verify spiked request still has complete response body.");

        long maxTime = 0;
        JArray? spikeData = null;
        int spikeIndex = 0;

        for (int i = 1; i <= Iterations; i++)
        {
            var response = await apiClient.PostAsync("games", Body);
            if (response.ResponseTimeMs > maxTime)
            {
                maxTime = response.ResponseTimeMs;
                spikeData = response.AsArray();
                spikeIndex = i;
            }
        }

        testLog.Info($"Highest spike: Request #{spikeIndex} at {maxTime}ms");
        testLog.Info($"Spike response records: {spikeData?.Count}");

        // Assert: Body phải complete
        Assert.That(spikeData, Is.Not.Null, "Spike response should have data");
        Assert.That(spikeData!.Count, Is.GreaterThan(0), "Spike response should contain records");
    }

    [Test, Order(4)]
    public async Task Step4_DegradationCheck_NoConsistentIncrease()
    {
        // Arrange & Act: So sánh 3 request đầu vs 3 request cuối
        testLog.Info("Step 4: [Degradation check] Compare first 3 vs last 3 response times.");

        var times = new List<long>();
        for (int i = 1; i <= Iterations; i++)
        {
            var response = await apiClient.PostAsync("games", Body);
            times.Add(response.ResponseTimeMs);
        }

        var first3Avg = times.Take(3).Average();
        var last3Avg = times.Skip(Iterations - 3).Take(3).Average();
        testLog.Info($"First 3 average: {first3Avg:F1}ms");
        testLog.Info($"Last 3 average: {last3Avg:F1}ms");

        // Assert: Không có degradation quá 2x
        Assert.That(last3Avg, Is.LessThanOrEqualTo(first3Avg * 2),
            "Last 3 requests should not be more than 2x slower than first 3");
    }
}
