using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.10_PLATFORMS_01 - [/platforms] Verify game platform query and filtering by category
/// Priority: High
/// Endpoint: POST https://api.igdb.com/v4/platforms
/// Category enum: 1=Console, 2=Arcade, 3=Platform, 4=OS, 5=Portable, 6=Computer
/// </summary>
[TestFixture]
[Order(10)]
public class TC10_Platforms01_FilterTests : ApiBaseTest
{
    [Test, Order(1)]
    public async Task Step1_FilterCategory1Console_AllShouldHaveCategory1()
    {
        // Arrange & Act
        testLog.Info("Step 1: [Filter category] Query /platforms with where category = 1 (Console), limit 10.");
        var response = await apiClient.PostAsync("platforms",
            "fields id, name, category, slug; where category = 1; limit 10;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Records: {data.Count}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");

        foreach (var item in data)
        {
            var category = item["category"]!.Value<int>();
            var name = item["name"]!.ToString();
            Assert.That(category, Is.EqualTo(1),
                $"Platform '{name}' should have category=1, got {category}");
            Assert.That(name, Is.Not.Empty, "Platform name should not be empty");
            testLog.Info($"  Platform: {name} (category: {category})");
        }
    }

    [Test, Order(2)]
    public async Task Step2_CrossCheckCategory5_ShouldBeDistinctFromCategory1()
    {
        // Arrange: Lấy cả 2 category
        testLog.Info("Step 2: [Cross-check] Compare category=1 (Console) vs category=5 (Portable).");

        var cat1Response = await apiClient.PostAsync("platforms",
            "fields id, name, category, slug; where category = 1; limit 10;");
        var cat5Response = await apiClient.PostAsync("platforms",
            "fields id, name, category, slug; where category = 5; limit 10;");

        var cat1Ids = cat1Response.AsArray().Select(p => p["id"]!.Value<int>()).ToList();
        var cat5Ids = cat5Response.AsArray().Select(p => p["id"]!.Value<int>()).ToList();

        testLog.Info($"Category 1 (Console): {cat1Ids.Count} platforms");
        testLog.Info($"Category 5 (Portable): {cat5Ids.Count} platforms");

        // Assert: category = 5 check
        Assert.That(cat5Response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        foreach (var item in cat5Response.AsArray())
        {
            Assert.That(item["category"]!.Value<int>(), Is.EqualTo(5),
                $"Platform '{item["name"]}' should have category=5");
        }

        // Assert: Không có overlap
        var overlap = cat5Ids.Intersect(cat1Ids).ToList();
        Assert.That(overlap.Count, Is.EqualTo(0),
            $"Category 1 and 5 should have no overlap — found: {string.Join(",", overlap)}");
    }
}
