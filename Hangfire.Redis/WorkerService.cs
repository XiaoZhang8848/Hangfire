using Hangfire.Redis.Controllers;

namespace Hangfire.Redis;

public class WorkerService: BackgroundService
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IServiceProvider _serviceProvider;

    public WorkerService(
        IRecurringJobManager recurringJobManager,
        IServiceProvider serviceProvider
    )
    {
        _recurringJobManager = recurringJobManager;
        _serviceProvider = serviceProvider;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var jobController = _serviceProvider.GetRequiredService<JobController>();
        _recurringJobManager.AddOrUpdate("testJobId", "worker", () => jobController.Test(), "10/3 * * * * ?");
    }
}