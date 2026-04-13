using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace SeleniumTests.Utilities;

/// <summary>
/// Kết quả trả về từ API: chứa StatusCode, Body (JSON), và ResponseTime (ms)
/// </summary>
public class ApiResponse
{
    public int StatusCode { get; set; }
    public string Body { get; set; } = string.Empty;
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Parse body thành JArray (dùng cho hầu hết response IGDB trả về array)
    /// </summary>
    public JArray AsArray()
    {
        try { return JArray.Parse(Body); }
        catch { return new JArray(); }
    }

    /// <summary>
    /// Parse body thành JObject (dùng cho error response hoặc single object)
    /// </summary>
    public JObject AsObject()
    {
        try { return JObject.Parse(Body); }
        catch { return new JObject(); }
    }
}

/// <summary>
/// Wrapper HttpClient để gọi IGDB API.
/// Tự động gắn headers Client-ID và Authorization từ appsettings.json.
/// Đo response time cho mỗi request.
/// </summary>
public class ApiClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ApiClient()
    {
        // Đọc config từ appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _baseUrl = config["IGDB:BaseUrl"]!;
        var clientId = config["IGDB:ClientId"]!;
        var accessToken = config["IGDB:AccessToken"]!;

        // Khởi tạo HttpClient với headers mặc định
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Client-ID", clientId);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    /// <summary>
    /// Gửi POST request tới IGDB API.
    /// </summary>
    /// <param name="endpoint">Tên endpoint, ví dụ: "games", "characters", "genres"</param>
    /// <param name="body">Body dạng chuỗi IGDB query, ví dụ: "fields id, name; limit 10;"</param>
    /// <returns>ApiResponse chứa status, body, response time</returns>
    public async Task<ApiResponse> PostAsync(string endpoint, string body)
    {
        var url = $"{_baseUrl}/{endpoint}";
        var content = new StringContent(body, Encoding.UTF8, "text/plain");

        // Đo response time bằng Stopwatch
        var stopwatch = Stopwatch.StartNew();
        var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = content
        });
        stopwatch.Stop();

        var responseBody = await response.Content.ReadAsStringAsync();

        return new ApiResponse
        {
            StatusCode = (int)response.StatusCode,
            Body = responseBody,
            ResponseTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
