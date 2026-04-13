using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.11_COMPANIES_01 - [/companies] Verify company information query and validity of linked data
/// Priority: Medium
/// Endpoint: POST https://api.igdb.com/v4/companies
/// </summary>
[TestFixture]
[Order(11)]
public class TC11_Companies01_CompanyInfoTests : ApiBaseTest
{
    [Test, Order(1)]
    public async Task Step1_ResponseStructure_FieldTypesShouldBeValid()
    {
        // Arrange & Act
        testLog.Info("Step 1: [Response structure] Query /companies with fields id, name, country, developed, published; limit 10.");
        var response = await apiClient.PostAsync("companies",
            "fields id, name, country, developed, published, start_date; limit 10;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Records: {data.Count}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");

        foreach (var item in data)
        {
            // country là ISO 3166-1 numeric (number)
            var country = item["country"];
            if (country != null)
            {
                Assert.That(country.Type, Is.EqualTo(JTokenType.Integer),
                    $"country should be a number (ISO 3166-1) for {item["name"]}");
            }

            // developed là array
            var developed = item["developed"];
            if (developed != null)
            {
                Assert.That(developed.Type, Is.EqualTo(JTokenType.Array),
                    $"developed should be an array for {item["name"]}");
            }

            // published là array
            var published = item["published"];
            if (published != null)
            {
                Assert.That(published.Type, Is.EqualTo(JTokenType.Array),
                    $"published should be an array for {item["name"]}");
            }

            testLog.Info($"  Company: {item["name"]} (country: {country})");
        }
    }

    [Test, Order(2)]
    public async Task Step2_FilterNintendo_ShouldReturnCorrectData()
    {
        // Arrange & Act
        testLog.Info("Step 2: [Specific filter] Query /companies with where name = \"Nintendo\".");
        var response = await apiClient.PostAsync("companies",
            "fields id, name, country, developed; where name = \"Nintendo\"; limit 1;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(data.Count, Is.GreaterThanOrEqualTo(1), "Should return at least 1 Nintendo record");

        var nintendo = data[0];
        Assert.That(nintendo["name"]!.ToString(), Is.EqualTo("Nintendo"), "Name should be Nintendo");
        Assert.That(nintendo["country"]!.Value<int>(), Is.EqualTo(392), "Country should be 392 (Japan)");
        testLog.Info($"  Nintendo: country = {nintendo["country"]}, developed count = {nintendo["developed"]?.Count()}");
    }

    [Test, Order(3)]
    public async Task Step3_CrossCheckGame_GameIdShouldExist()
    {
        // Arrange: Lấy 1 game ID từ Nintendo's developed list
        testLog.Info("Step 3: [Cross-check] Take a game ID from Nintendo's developed list, verify in /games.");

        var nintendoResponse = await apiClient.PostAsync("companies",
            "fields id, name, country, developed; where name = \"Nintendo\"; limit 1;");
        var nintendo = nintendoResponse.AsArray()[0];

        var developed = nintendo["developed"] as JArray;
        Assert.That(developed, Is.Not.Null.And.Not.Empty, "Nintendo should have developed games");

        var gameId = developed![0]!.Value<int>();
        testLog.Info($"Testing game ID: {gameId}");

        // Act: Cross-check với /games
        var gameResponse = await apiClient.PostAsync("games",
            $"fields id, name, involved_companies; where id = {gameId}; limit 1;");

        var gameData = gameResponse.AsArray();
        testLog.Info($"Game response: HTTP {gameResponse.StatusCode}");

        // Assert
        Assert.That(gameResponse.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(gameData.Count, Is.EqualTo(1), "Should return exactly 1 game");
        Assert.That(gameData[0]["name"]!.ToString(), Is.Not.Empty, "Game should have a name");
        testLog.Info($"  Game: {gameData[0]["name"]} (ID: {gameData[0]["id"]})");
    }
}
