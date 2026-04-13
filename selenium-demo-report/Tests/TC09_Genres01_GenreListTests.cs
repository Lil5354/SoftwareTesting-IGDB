using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.09_GENRES_01 - [/genres] Verify game genre list query and validity of returned data
/// Priority: Medium
/// Endpoint: POST https://api.igdb.com/v4/genres
/// Body: fields id, name, slug; limit 20;
/// </summary>
[TestFixture]
[Order(9)]
public class TC09_Genres01_GenreListTests : ApiBaseTest
{
    private const string Body = "fields id, name, slug; limit 20;";

    [Test, Order(1)]
    public async Task Step1_ResponseStructure_ShouldBeArrayOfValidGenres()
    {
        // Arrange & Act
        testLog.Info("Step 1: [Response structure] Query /genres with fields id, name, slug; limit 20.");
        var response = await apiClient.PostAsync("genres", Body);

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Records: {data.Count}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(data.Count, Is.LessThanOrEqualTo(20), "Should return ≤ 20 genres");

        // Kiểm tra từng genre
        foreach (var item in data)
        {
            var id = item["id"];
            var name = item["name"];

            Assert.That(id, Is.Not.Null, "id should exist");
            Assert.That(id!.Type, Is.EqualTo(JTokenType.Integer), "id should be a number");

            Assert.That(name, Is.Not.Null, "name should exist");
            Assert.That(name!.Type, Is.EqualTo(JTokenType.String), "name should be a string");

            var slug = item["slug"];
            if (slug != null)
            {
                Assert.That(slug.Type, Is.EqualTo(JTokenType.String), "slug should be a string if present");
            }

            testLog.Info($"  Genre: {name} (ID: {id}, slug: {slug})");
        }
    }

    [Test, Order(2)]
    public async Task Step2_Consistency_SameResultsAcross3Calls()
    {
        // Arrange & Act: Gọi 3 lần, so sánh kết quả
        testLog.Info("Step 2: [Consistency] Call the same request 3 times. Compare results.");

        var response1 = await apiClient.PostAsync("genres", Body);
        var response2 = await apiClient.PostAsync("genres", Body);
        var response3 = await apiClient.PostAsync("genres", Body);

        var ids1 = response1.AsArray().Select(g => g["id"]!.Value<int>()).ToList();
        var ids2 = response2.AsArray().Select(g => g["id"]!.Value<int>()).ToList();
        var ids3 = response3.AsArray().Select(g => g["id"]!.Value<int>()).ToList();

        testLog.Info($"Call 1: {ids1.Count} genres, IDs: [{string.Join(",", ids1)}]");
        testLog.Info($"Call 2: {ids2.Count} genres, IDs: [{string.Join(",", ids2)}]");
        testLog.Info($"Call 3: {ids3.Count} genres, IDs: [{string.Join(",", ids3)}]");

        // Assert: Kết quả phải nhất quán
        Assert.That(ids1, Is.EqualTo(ids2), "Call 1 and 2 should have identical results");
        Assert.That(ids2, Is.EqualTo(ids3), "Call 2 and 3 should have identical results");
    }
}
