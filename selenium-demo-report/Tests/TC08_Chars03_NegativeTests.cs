using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SeleniumTests.Tests;

/// <summary>
/// TC1.08_CHARS_03 - [/characters] Verify exception handling (Negative Testing)
/// with an empty search string and out-of-range species enum
/// Priority: Medium
/// Expected: These tests are designed to FAIL — documenting known IGDB API behavior
/// </summary>
[TestFixture]
[Order(8)]
public class TC08_Chars03_NegativeTests : ApiBaseTest
{
    [Test, Order(1)]
    public async Task Step1_EmptySearchString_ShouldReturn400()
    {
        // Arrange & Act
        testLog.Info("Step 1: [Empty string search] Send /characters with search \"\".");
        testLog.Info("NOTE: This is a negative test — IGDB may not validate empty search properly.");

        var response = await apiClient.PostAsync("characters",
            "fields id, name; search \"\"; limit 5;");

        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Response Body (first 200 chars): {response.Body[..Math.Min(200, response.Body.Length)]}");

        // Assert: Mong đợi 400, nhưng IGDB có thể trả 200 (known behavior)
        Assert.That(response.StatusCode, Is.EqualTo(400),
            $"Expected HTTP 400 for empty search string, but got {response.StatusCode}. " +
            "IGDB lacks input validation for empty search — this is a known API behavior.");
    }

    [Test, Order(2)]
    public async Task Step2_OutOfEnumSpecies_ShouldReturn400()
    {
        // Arrange & Act
        testLog.Info("Step 2: [Out-of-enum species] Send /characters with where species = 99.");
        testLog.Info("NOTE: This is a negative test — IGDB may not validate enum range.");

        var response = await apiClient.PostAsync("characters",
            "fields id, name, species; where species = 99; limit 5;");

        testLog.Info($"HTTP Status: {response.StatusCode}");
        testLog.Info($"Response Body (first 200 chars): {response.Body[..Math.Min(200, response.Body.Length)]}");

        // Assert: Mong đợi 400, nhưng IGDB trả 200 với [] (known behavior)
        Assert.That(response.StatusCode, Is.EqualTo(400),
            $"Expected HTTP 400 for species=99 (out of enum 1-5), but got {response.StatusCode}. " +
            "IGDB does not validate enum range — species=99 is accepted silently.");
    }
}
