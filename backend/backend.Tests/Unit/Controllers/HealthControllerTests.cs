using backend.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace backend.Tests.Unit.Controllers;

[Trait("Category", "Unit")]
public class HealthControllerTests
{
    private readonly HealthController _controller = new(new FakeWebHostEnvironment());

    [Fact]
    public void GetHealth_ReturnsOk()
    {
        var result = _controller.GetHealth();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public void GetHealth_ReturnsEnvironmentName()
    {
        var result = _controller.GetHealth();

        var ok = Assert.IsType<OkObjectResult>(result);
        var value = ok.Value!.ToString();
        Assert.Contains("Testing", value);
    }

    private class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Testing";
        public string ApplicationName { get; set; } = "TestApp";
        public string WebRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = "";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
