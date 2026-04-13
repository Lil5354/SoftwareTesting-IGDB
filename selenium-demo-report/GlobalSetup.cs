using NUnit.Framework;
using SeleniumTests.Utilities;

namespace SeleniumTests;

[SetUpFixture] // Gắn cờ đánh dấu đây là file cấu hình Toàn cục
public class GlobalSetup
{
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        // Khởi tạo file HTML ngay khi dự án bắt đầu chạy
        ReportManager.InitializeReport();
    }

    [OneTimeTearDown]
    public void RunAfterAllTests()
    {
        // Xuất file HTML sau khi tất cả các test case đã chạy xong
        ReportManager.FlushReport();
    }
}