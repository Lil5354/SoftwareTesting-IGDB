using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.05_Perf_05 - [Performance] Verify /games performance with fields * at limit 50 and limit 500
/// Priority: Medium
/// Endpoint: POST https://api.igdb.com/v4/games
/// Acceptance threshold: limit 50 ≤ 3000ms, limit 500 ≤ 10000ms
/// </summary>
[TestFixture]
[Order(5)]
public class TC05_Perf05_FieldsStarPayloadTests : ApiBaseTest
{
    [Test, Order(1)]
    public async Task Step1_FieldsStarLimit50_ResponseTimeShouldBeLessThan3000ms()
    {
        // Arrange & Act
        testLog.Info("Step 1: [fields * limit 50] Send request with fields *; limit 50.");
        var response = await apiClient.PostAsync("games", "fields *; limit 50;");

        var data = response.AsArray();
        var sizeKB = response.Body.Length / 1024.0;
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Response Time: {response.ResponseTimeMs}ms");
        testLog.Info($"Records: {data.Count}");
        testLog.Info($"Response Size: {sizeKB:F2} KB");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(response.ResponseTimeMs, Is.LessThanOrEqualTo(3000),
            $"fields * limit 50 response time {response.ResponseTimeMs}ms should be ≤ 3000ms");
        Assert.That(data.Count, Is.EqualTo(50), "Should return 50 records");
    }

    [Test, Order(2)]
    public async Task Step2_BaselineRatio_ShouldBeLessThan15x()
    {
        // Arrange & Act
        testLog.Info("Step 2: [Baseline ratio] Compare fields * vs simple query.");

        var simpleResponse = await apiClient.PostAsync("games", "fields id, name; limit 10;");
        var starResponse = await apiClient.PostAsync("games", "fields *; limit 50;");

        double ratio = (double)starResponse.ResponseTimeMs / Math.Max(simpleResponse.ResponseTimeMs, 1);
        testLog.Info($"Simple query: {simpleResponse.ResponseTimeMs}ms");
        testLog.Info($"Fields * limit 50: {starResponse.ResponseTimeMs}ms");
        testLog.Info($"Ratio: {ratio:F2}x");

        // Assert
        Assert.That(ratio, Is.LessThanOrEqualTo(15),
            $"fields*/simple ratio {ratio:F2}x should be ≤ 15x");
    }

    [Test, Order(3)]
    public async Task Step3_FieldsStarLimit500_ResponseTimeShouldBeLessThan10000ms()
    {
        // Arrange & Act
        testLog.Info("Step 3: [fields * limit 500] Send request with fields *; limit 500 (max).");
        var response = await apiClient.PostAsync("games", "fields *; limit 500;");

        var sizeKB = response.Body.Length / 1024.0;
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Response Time: {response.ResponseTimeMs}ms");
        testLog.Info($"Response Size: {sizeKB:F2} KB");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(response.ResponseTimeMs, Is.LessThanOrEqualTo(10000),
            $"fields * limit 500 response time {response.ResponseTimeMs}ms should be ≤ 10000ms");
    }

    [Test, Order(4)]
    public async Task Step4_StabilityCheck_NoTimeouts()
    {
        // Arrange & Act: Gửi 2 request fields * limit 500, kiểm tra stability
        testLog.Info("Step 4: [Stability check] With fields * limit 500, send 2 more requests.");

        for (int i = 1; i <= 2; i++)
        {
            var response = await apiClient.PostAsync("games", "fields *; limit 500;");
            var data = response.AsArray();
            testLog.Info($"Run #{i}: {response.ResponseTimeMs}ms — HTTP {response.StatusCode} — Records: {data.Count}");

            Assert.That(response.StatusCode, Is.EqualTo(200), $"Run #{i} should return HTTP 200");
            Assert.That(data.Count, Is.GreaterThan(0), $"Run #{i} should return data");
        }
    }
}
