using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Hangfire.Redis.StackExchange;
using StackExchange.Redis;
using Hangfire.Redis;
using Hangfire.Redis.Controllers;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

var services = builder.Services;
services.AddSwaggerGen();
services.AddControllers();

var redis = ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!);
services.AddHangfire(opts =>
{
    var storageOptions = new RedisStorageOptions
    {
        Prefix = "fast",
        Db = 1
    };
    opts.UseRedisStorage(redis, storageOptions);
    opts.UseFilter(new AutomaticRetryAttribute { Attempts = 3 }); // 重试次数
    
}).AddHangfireServer(opts =>
{
    opts.ServerName = $"fast.{configuration["Env"]}"; // 服务名
    opts.SchedulePollingInterval = TimeSpan.FromSeconds(1); // 执行间隔
    opts.HeartbeatInterval  = TimeSpan.FromSeconds(1); // 心跳间隔
    opts.Queues = configuration.GetSection("Hangfire:Queues").Get<string[]>(); // 队列名字
    opts.WorkerCount = 30; // 并行数
});

services.AddTransient<JobController>();
services.AddHostedService<WorkerService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
    Authorization = new[]
    {
        new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
        {
            RequireSsl = false, // 必须https
            SslRedirect = false, // https重定向
            LoginCaseSensitive = true, // 区分大小写
            Users = new[]
            {
                new BasicAuthAuthorizationUser
                {
                    Login = configuration["Hangfire:Account"],
                    PasswordClear = configuration["Hangfire:Password"]
                }
            }
        })
    }
});

app.Run();