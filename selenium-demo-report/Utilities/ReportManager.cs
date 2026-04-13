using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System.IO;

namespace SeleniumTests.Utilities;

public static class ReportManager
{
    private static ExtentReports _extent;

    // Thuộc tính để các lớp Test gọi đến và ghi log
    public static ExtentTest CurrentTest { get; set; }

    public static void InitializeReport()
    {
        // 1. Xác định đường dẫn lưu file HTML (Lưu vào thư mục bin/Debug/...)
        string reportPath = Path.Combine(Directory.GetCurrentDirectory(), "ExtentReport.html");

        // 2. Khởi tạo SparkReporter (Giao diện HTML hiện đại của Extent)
        var sparkReporter = new ExtentSparkReporter(reportPath);
        sparkReporter.Config.DocumentTitle = "IGDB API - Automated Test Report";
        sparkReporter.Config.ReportName = "IGDB API Test Suite - VoDangKhoa";
        sparkReporter.Config.Theme = AventStack.ExtentReports.Reporter.Config.Theme.Dark;

        // 3. Đính kèm giao diện vào bộ máy Report
        _extent = new ExtentReports();
        _extent.AttachReporter(sparkReporter);

        // Thêm thông tin môi trường
        _extent.AddSystemInfo("Người thực hiện", "Võ Đăng Khoa");
        _extent.AddSystemInfo("Hệ điều hành", System.Environment.OSVersion.ToString());
        _extent.AddSystemInfo("API Base URL", "https://api.igdb.com/v4");
        _extent.AddSystemInfo("Framework", "NUnit + HttpClient");
    }

    public static ExtentTest CreateTest(string testName)
    {
        CurrentTest = _extent.CreateTest(testName);
        return CurrentTest;
    }

    public static void FlushReport()
    {
        // Lệnh bắt buộc để xuất/lưu file HTML vào ổ cứng
        _extent?.Flush();
    }
}