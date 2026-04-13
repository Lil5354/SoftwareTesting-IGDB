using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.06_CHARS_01 - [/characters] Verify character search by name and completeness of returned data
/// Priority: High
/// Endpoint: POST https://api.igdb.com/v4/characters
/// Body: fields id, name, gender, description, games; search "Mario"; limit 5;
/// </summary>
[TestFixture]
[Order(6)]
public class TC06_Chars01_CharacterSearchTests : ApiBaseTest
{
    private const string Body = "fields id, name, gender, description, games; search \"Mario\"; limit 5;";

    [Test, Order(1)]
    public async Task Step1_SearchCharacter_ShouldReturnMarioRelatedResults()
    {
        // Arrange & Act
        testLog.Info("Step 1: [Search character] Query /characters with search \"Mario\".");
        var response = await apiClient.PostAsync("characters", Body);

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Records returned: {data.Count}");

        // Log tên các character
        foreach (var item in data)
        {
            testLog.Info($"  Character: {item["name"]} (ID: {item["id"]})");
        }

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(data.Count, Is.GreaterThan(0),
            "Response should contain at least 1 character related to Mario");
    }

    [Test, Order(2)]
    public async Task Step2_FieldValidation_IdAndNameShouldBeValid()
    {
        // Arrange & Act
        testLog.Info("Step 2: [Field validation] Confirm data types: id is number, name is string.");
        var response = await apiClient.PostAsync("characters", Body);

        var data = response.AsArray();

        // Assert: Kiểm tra từng object
        foreach (var item in data)
        {
            var id = item["id"];
            var name = item["name"];

            Assert.That(id, Is.Not.Null, "id field should exist");
            Assert.That(id!.Type, Is.EqualTo(JTokenType.Integer), $"id should be a number, got {id.Type}");

            Assert.That(name, Is.Not.Null, "name field should exist");
            Assert.That(name!.Type, Is.EqualTo(JTokenType.String), $"name should be a string, got {name.Type}");
            Assert.That(name.ToString(), Is.Not.Empty, "name should not be empty");

            // gender và games là optional
            var gender = item["gender"];
            if (gender != null)
            {
                testLog.Info($"  {name}: gender = {gender}");
            }

            var games = item["games"];
            if (games != null)
            {
                Assert.That(games.Type, Is.EqualTo(JTokenType.Array), "games should be an array if present");
                testLog.Info($"  {name}: {((JArray)games).Count} games linked");
            }
        }
    }
}
