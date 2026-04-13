using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.13_PP_01 - [/player_perspectives] Verify player perspective query and linkage with /games
/// Priority: Low
/// Endpoint: POST https://api.igdb.com/v4/player_perspectives
/// Reference enum: 1=First person, 2=Third person, 3=Bird view, 4=Side view, 5=Text, 6=Auditory, 7=VR
/// </summary>
[TestFixture]
[Order(13)]
public class TC13_PP01_PlayerPerspectiveTests : ApiBaseTest
{
    [Test, Order(1)]
    public async Task Step1_StructureAndEnum_IdShouldBeWithinRange1To7()
    {
        // Arrange & Act
        testLog.Info("Step 1: [Structure and enum] Query /player_perspectives with fields id, name, slug; limit 10.");
        var response = await apiClient.PostAsync("player_perspectives",
            "fields id, name, slug; limit 10;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Records: {data.Count}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");

        foreach (var item in data)
        {
            var id = item["id"]!.Value<int>();
            var name = item["name"]!.ToString();

            Assert.That(id, Is.InRange(1, 7),
                $"Player perspective ID should be 1-7, got {id}");
            Assert.That(name, Is.Not.Empty, "Name should not be empty");

            testLog.Info($"  Perspective: {name} (ID: {id})");
        }
    }

    [Test, Order(2)]
    public async Task Step2_CrossCheckGames_FirstPersonGamesShouldContainId1()
    {
        // Arrange & Act
        testLog.Info("Step 2: [Cross-check with /games] Find First Person games (perspective ID=1).");
        var response = await apiClient.PostAsync("games",
            "fields id, name, player_perspectives; where player_perspectives = (1); limit 5;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"First Person games: {data.Count}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");

        foreach (var item in data)
        {
            var perspectives = item["player_perspectives"] as JArray;
            Assert.That(perspectives, Is.Not.Null, "player_perspectives should exist");
            Assert.That(perspectives!.Select(p => p.Value<int>()).ToList(), Does.Contain(1),
                $"Game '{item["name"]}' should have player_perspectives containing 1 (First person)");
            testLog.Info($"  Game: {item["name"]} — perspectives: [{string.Join(",", perspectives)}]");
        }
    }

    [Test, Order(3)]
    public async Task Step3_VRCheck_ShouldReturnArrayWithoutError()
    {
        // Arrange & Act
        testLog.Info("Step 3: [Full enum check] Query /games with player_perspectives = (7) (VR).");
        var response = await apiClient.PostAsync("games",
            "fields id, name, player_perspectives; where player_perspectives = (7); limit 3;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"VR games: {data.Count}");

        // Assert: Chấp nhận array rỗng hoặc có data — không được lỗi
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(data, Is.Not.Null, "Response should be a valid array (even if empty)");

        foreach (var item in data)
        {
            testLog.Info($"  VR Game: {item["name"]}");
        }
    }
}
