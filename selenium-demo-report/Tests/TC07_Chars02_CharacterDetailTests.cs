using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.07_CHARS_02 - [/characters] Verify detailed character query: mug_shot, species, and akas
/// Priority: Medium
/// Endpoint: POST https://api.igdb.com/v4/characters
/// </summary>
[TestFixture]
[Order(7)]
public class TC07_Chars02_CharacterDetailTests : ApiBaseTest
{
    [Test, Order(1)]
    public async Task Step1_NestedMugShot_UrlShouldBeValidIfPresent()
    {
        // Arrange & Act
        testLog.Info("Step 1: [Nested mug_shot] Query /characters with mug_shot fields.");
        var response = await apiClient.PostAsync("characters",
            "fields id, name, mug_shot.image_id, mug_shot.url, mug_shot.width, mug_shot.height; limit 5;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Records: {data.Count}");

        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");

        // Assert: Kiểm tra mug_shot nếu có
        foreach (var item in data)
        {
            var mugShot = item["mug_shot"];
            if (mugShot != null && mugShot.Type == JTokenType.Object)
            {
                var url = mugShot["url"]?.ToString();
                Assert.That(url, Is.Not.Null.And.Contains("//"),
                    $"mug_shot.url should contain '//' — got: {url}");

                var width = mugShot["width"];
                if (width != null)
                {
                    Assert.That(width.Value<int>(), Is.GreaterThan(0), "mug_shot.width should be > 0");
                }

                testLog.Info($"  {item["name"]}: mug_shot url = {url}");
            }
        }
    }

    [Test, Order(2)]
    public async Task Step2_SpeciesEnum_ShouldBeWithinRange1To5()
    {
        // Arrange & Act
        testLog.Info("Step 2: [Species enum] Verify species value is within range 1-5.");
        var response = await apiClient.PostAsync("characters",
            "fields id, name, species; where species != null; limit 5;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Records with species: {data.Count}");

        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");

        // Assert: species trong range 1-5
        foreach (var item in data)
        {
            var species = item["species"];
            if (species != null)
            {
                var val = species.Value<int>();
                Assert.That(val, Is.InRange(1, 5),
                    $"species should be 1-5, got {val} for {item["name"]}");
                testLog.Info($"  {item["name"]}: species = {val}");
            }
        }
    }

    [Test, Order(3)]
    public async Task Step3_AlternativeNames_AkasShouldBeArrayIfPresent()
    {
        // Arrange & Act
        testLog.Info("Step 3: [Alternative names] Verify akas field is array of strings when present.");
        var response = await apiClient.PostAsync("characters",
            "fields id, name, akas; limit 5;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");

        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");

        // Assert
        foreach (var item in data)
        {
            var akas = item["akas"];
            if (akas != null)
            {
                Assert.That(akas.Type, Is.EqualTo(JTokenType.Array),
                    $"akas should be an array for {item["name"]}");
                testLog.Info($"  {item["name"]}: akas = [{string.Join(", ", akas)}]");
            }
        }
    }
}
