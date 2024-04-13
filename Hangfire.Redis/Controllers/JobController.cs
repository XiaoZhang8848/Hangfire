using Microsoft.AspNetCore.Mvc;

namespace Hangfire.Redis.Controllers;

[ApiController]
[Route("job")]
public class JobController: ControllerBase
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IConfiguration _configuration;

    public JobController(IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
    {
        _webHostEnvironment = webHostEnvironment;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task Test()
    {
        await Task.Delay(5000);
        Console.WriteLine($"{DateTime.Now} {_configuration["Env"]}");
    }
}