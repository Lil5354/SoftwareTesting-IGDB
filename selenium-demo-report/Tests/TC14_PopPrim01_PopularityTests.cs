using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.14_POPPRIM_01 - [/popularity_primitives] Verify popularity score (PopScore) query and filtering
/// Priority: High
/// Endpoint: POST https://api.igdb.com/v4/popularity_primitives
/// popularity_type enum: 1=igdb_score, 2=want_to_play, 3=playing, 4=played
/// </summary>
[TestFixture]
[Order(14)]
public class TC14_PopPrim01_PopularityTests : ApiBaseTest
{
    [Test, Order(1)]
    public async Task Step1_ResponseStructure_FieldsShouldHaveCorrectTypes()
    {
        // Arrange & Act
        testLog.Info("Step 1: [Response structure] Query /popularity_primitives with fields game_id, popularity_type, value; limit 10.");
        var response = await apiClient.PostAsync("popularity_primitives",
            "fields game_id, popularity_type, value; limit 10;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Records: {data.Count}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");

        foreach (var item in data)
        {
            var gameId = item["game_id"];
            Assert.That(gameId, Is.Not.Null, "game_id should exist");
            Assert.That(gameId!.Type, Is.EqualTo(JTokenType.Integer), "game_id should be a number");

            var popType = item["popularity_type"]!.Value<int>();
            Assert.That(popType, Is.InRange(1, 6),
                $"popularity_type should be 1-6, got {popType}");

            var value = item["value"];
            Assert.That(value, Is.Not.Null, "value should exist");
            Assert.That(value!.Value<double>(), Is.GreaterThanOrEqualTo(0), "value should be ≥ 0");

            testLog.Info($"  game_id: {gameId}, type: {popType}, value: {value}");
        }
    }

    [Test, Order(2)]
    public async Task Step2_FilterAndSort_Type1ShouldBeDescending()
    {
        // Arrange & Act
        testLog.Info("Step 2: [Filter popularity_type] Query with type=1, sort value desc, limit 5.");
        var response = await apiClient.PostAsync("popularity_primitives",
            "fields game_id, popularity_type, value; where popularity_type = 1; sort value desc; limit 5;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Records: {data.Count}");

        // Assert: Tất cả đều type=1
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");

        foreach (var item in data)
        {
            Assert.That(item["popularity_type"]!.Value<int>(), Is.EqualTo(1),
                "All records should have popularity_type = 1");
        }

        // Assert: Giá trị giảm dần
        var values = data.Select(p => p["value"]!.Value<double>()).ToList();
        for (int i = 1; i < values.Count; i++)
        {
            Assert.That(values[i - 1], Is.GreaterThanOrEqualTo(values[i]),
                $"Values should be descending: {values[i - 1]} >= {values[i]}");
        }

        testLog.Info($"  Values (desc): [{string.Join(", ", values.Select(v => v.ToString("F2")))}]");
    }

    [Test, Order(3)]
    public async Task Step3_CrossCheckGameId_GameShouldExist()
    {
        // Arrange: Lấy game_id từ popularity results
        testLog.Info("Step 3: [Cross-check game_id] Verify game_id exists in /games.");

        var popResponse = await apiClient.PostAsync("popularity_primitives",
            "fields game_id, popularity_type, value; where popularity_type = 1; sort value desc; limit 5;");
        var popData = popResponse.AsArray();

        Assert.That(popData.Count, Is.GreaterThan(0), "Should have popularity data");

        var gameId = popData[0]["game_id"]!.Value<int>();
        testLog.Info($"Testing game_id: {gameId}");

        // Act: Cross-check với /games
        var gameResponse = await apiClient.PostAsync("games",
            $"fields id, name; where id = {gameId}; limit 1;");

        var gameData = gameResponse.AsArray();

        // Assert
        Assert.That(gameResponse.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(gameData.Count, Is.EqualTo(1), "Game should exist");
        Assert.That(gameData[0]["name"]!.ToString(), Is.Not.Empty, "Game should have a name");
        testLog.Info($"  Game: {gameData[0]["name"]} (ID: {gameData[0]["id"]})");
    }

    [Test, Order(4)]
    public async Task Step4_DataFreshness_StructureShouldBeConsistent()
    {
        // Arrange & Act: Gọi cùng request 2 lần, so sánh structure
        testLog.Info("Step 4: [Data freshness] Call the same request twice. Verify structure consistency.");

        var body = "fields game_id, popularity_type, value; where popularity_type = 1; sort value desc; limit 5;";
        var response1 = await apiClient.PostAsync("popularity_primitives", body);
        var response2 = await apiClient.PostAsync("popularity_primitives", body);

        var data1 = response1.AsArray();
        var data2 = response2.AsArray();

        testLog.Info($"Call 1: {data1.Count} records");
        testLog.Info($"Call 2: {data2.Count} records");

        // Assert: Structure nhất quán
        Assert.That(data1.Count, Is.EqualTo(data2.Count), "Record count should be consistent");
        Assert.That(data1.Count, Is.LessThanOrEqualTo(5), "Should be ≤ 5 (limit 5)");

        // Kiểm tra cả hai đều có đúng fields
        foreach (var item in data2)
        {
            Assert.That(item["game_id"], Is.Not.Null, "game_id should exist");
            Assert.That(item["popularity_type"], Is.Not.Null, "popularity_type should exist");
            Assert.That(item["value"], Is.Not.Null, "value should exist");
        }
    }
}
