using AventStack.ExtentReports;
using NUnit.Framework;
using SeleniumTests.Utilities;

namespace SeleniumTests.Tests;

/// <summary>
/// Base class cho tất cả API test classes.
/// Cung cấp ApiClient và tự động ghi log vào ExtentReport.
/// </summary>
public class ApiBaseTest
{
    protected ApiClient apiClient;
    protected ExtentTest testLog;

    [SetUp]
    public void Setup()
    {
        // Khởi tạo ApiClient để gọi IGDB API
        apiClient = new ApiClient();

        // Tạo test entry trong ExtentReport với tên test hiện tại
        testLog = ReportManager.CreateTest(TestContext.CurrentContext.Test.Name);
    }

    [TearDown]
    public void TearDown()
    {
        // Ghi kết quả test vào report
        var outcome = TestContext.CurrentContext.Result.Outcome.Status;

        if (outcome == NUnit.Framework.Interfaces.TestStatus.Passed)
        {
            testLog.Pass("Test PASSED ✅");
        }
        else if (outcome == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            testLog.Fail($"Test FAILED ❌: {TestContext.CurrentContext.Result.Message}");
        }
        else
        {
            testLog.Skip("Test SKIPPED ⏭️");
        }

        // Giải phóng tài nguyên
        apiClient?.Dispose();
    }
}
