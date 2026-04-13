using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.15_COLL_01 - [/collections] Verify collection/game series list query, search, and filter
/// Priority: High
/// Endpoint: POST https://api.igdb.com/v4/collections
/// </summary>
[TestFixture]
[Order(15)]
public class TC15_Coll01_CollectionTests : ApiBaseTest
{
    [Test, Order(1)]
    public async Task Step1_ResponseStructure_ShouldBeArrayOfValidCollections()
    {
        // Arrange & Act
        testLog.Info("Step 1: [Response structure] Send POST /collections with fields id, name, slug, games; limit 10.");
        var response = await apiClient.PostAsync("collections",
            "fields id, name, slug, games, type; limit 10;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Records: {data.Count}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(data.Count, Is.LessThanOrEqualTo(10), "Should return ≤ 10 collections");

        foreach (var item in data)
        {
            var id = item["id"];
            var name = item["name"];

            Assert.That(id, Is.Not.Null, "id should exist");
            Assert.That(id!.Type, Is.EqualTo(JTokenType.Integer), "id should be a number");
            Assert.That(id.Value<int>(), Is.GreaterThan(0), "id should be positive");

            Assert.That(name, Is.Not.Null, "name should exist");
            Assert.That(name!.ToString(), Is.Not.Empty, "name should not be empty");

            var slug = item["slug"];
            if (slug != null)
            {
                Assert.That(slug.Type, Is.EqualTo(JTokenType.String), "slug should be a string");
            }

            var games = item["games"];
            if (games != null)
            {
                Assert.That(games.Type, Is.EqualTo(JTokenType.Array), "games should be an array");
            }

            testLog.Info($"  Collection: {name} (ID: {id}, games: {games?.Count() ?? 0})");
        }
    }

    [Test, Order(2)]
    public async Task Step2_SearchMario_ShouldReturnRelatedCollections()
    {
        // Arrange & Act
        testLog.Info("Step 2: [Search by name] Send POST /collections with search \"Mario\".");
        var response = await apiClient.PostAsync("collections",
            "fields id, name, games; search \"Mario\"; limit 5;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Records: {data.Count}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(data.Count, Is.GreaterThan(0), "Should return at least 1 Mario collection");

        // Ít nhất 1 kết quả phải chứa "Mario" trong tên
        var hasMario = data.Any(c =>
            c["name"]!.ToString().ToLower().Contains("mario"));
        Assert.That(hasMario, Is.True,
            "At least 1 collection name should contain 'Mario'");

        foreach (var item in data)
        {
            testLog.Info($"  Collection: {item["name"]} (games: {item["games"]?.Count() ?? 0})");
        }
    }

    [Test, Order(3)]
    public async Task Step3_FilterType1_AllShouldHaveType1()
    {
        // Arrange & Act
        testLog.Info("Step 3: [Filter by type] Send /collections with where type = 1; limit 5.");
        var response = await apiClient.PostAsync("collections",
            "fields id, name, type; where type = 1; limit 5;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Records: {data.Count}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");

        // Nếu có data, tất cả phải có type = 1
        if (data.Count > 0)
        {
            foreach (var item in data)
            {
                Assert.That(item["type"]!.Value<int>(), Is.EqualTo(1),
                    $"Collection '{item["name"]}' should have type=1");
                testLog.Info($"  Collection: {item["name"]} (type: {item["type"]})");
            }
        }
        else
        {
            testLog.Info("No collections with type=1 — acceptable (empty result).");
        }
    }
}
