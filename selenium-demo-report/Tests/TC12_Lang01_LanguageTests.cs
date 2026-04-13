using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.12_LANG_01 - [/languages] Verify supported language list query and data validity
/// Priority: Medium
/// Endpoint: POST https://api.igdb.com/v4/languages
/// </summary>
[TestFixture]
[Order(12)]
public class TC12_Lang01_LanguageTests : ApiBaseTest
{
    [Test, Order(1)]
    public async Task Step1_ResponseStructure_IdNameLocaleShouldBeValid()
    {
        // Arrange & Act
        testLog.Info("Step 1: [Response structure] Query /languages with fields id, name, native_name, locale; limit 20.");
        var response = await apiClient.PostAsync("languages",
            "fields id, name, native_name, locale; limit 20;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Records: {data.Count}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(data.Count, Is.GreaterThan(0), "Should return at least 1 language");

        foreach (var item in data)
        {
            var id = item["id"];
            var name = item["name"];

            Assert.That(id, Is.Not.Null, "id should exist");
            Assert.That(id!.Type, Is.EqualTo(JTokenType.Integer), "id should be a number");

            Assert.That(name, Is.Not.Null, "name should exist");
            Assert.That(name!.ToString(), Is.Not.Empty, "name should not be empty");

            // locale nên có ít nhất 2 ký tự (BCP-47)
            var locale = item["locale"];
            if (locale != null)
            {
                Assert.That(locale.ToString().Length, Is.GreaterThanOrEqualTo(2),
                    $"locale should have at least 2 chars for {name}");
            }

            testLog.Info($"  Language: {name} (locale: {locale})");
        }
    }

    [Test, Order(2)]
    public async Task Step2_FilterLocaleEnGB_ShouldReturnEnglish()
    {
        // Arrange & Act
        testLog.Info("Step 2: [Filter locale] Query /languages with where locale = \"en-GB\".");
        var response = await apiClient.PostAsync("languages",
            "fields id, name, locale; where locale = \"en-GB\"; limit 1;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");
        Assert.That(data.Count, Is.EqualTo(1), "Should return exactly 1 English language");

        var english = data[0];
        Assert.That(english["locale"]!.ToString(), Is.EqualTo("en-GB"), "Locale should be en-GB");
        Assert.That(english["name"]!.ToString(), Does.Contain("English"), "Name should contain 'English'");
        testLog.Info($"  English: {english["name"]} (locale: {english["locale"]})");
    }

    [Test, Order(3)]
    public async Task Step3_CrossCheckLanguageSupports_ShouldHaveLanguageField()
    {
        // Arrange & Act
        testLog.Info("Step 3: [Cross-check] Retrieve language_supports, verify it has language field.");
        var response = await apiClient.PostAsync("language_supports",
            "fields id, language; limit 1;");

        var data = response.AsArray();
        testLog.Info($"HTTP Status: {response.StatusCode}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(200), "Expected HTTP 200 OK");

        if (data.Count > 0)
        {
            Assert.That(data[0]["language"], Is.Not.Null, "language_supports should have 'language' field");
            testLog.Info($"  language_supports[0]: language = {data[0]["language"]}");
        }
    }
}
