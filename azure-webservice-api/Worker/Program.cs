using Azure.Messaging.ServiceBus;
using azure_web_service_quartz_worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Quartz;

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices((cxt, services) =>
    {
        services.AddSingleton<ServiceBusClient>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString("ServiceBusConnectionString");

            // Crea e restituisci il ServiceBusClient
            return new ServiceBusClient(connectionString);
        });

        services.AddQuartz();
        services.AddQuartzHostedService(opt =>
        {
            opt.WaitForJobsToComplete = true;
        });
    }).Build();

var schedulerFactory = builder.Services.GetRequiredService<ISchedulerFactory>();
var scheduler = await schedulerFactory.GetScheduler();

// define the job and tie it to our HelloJob class
var job = JobBuilder.Create<Worker>()
    .WithIdentity("job-azure-service-bus", "service-bus")
    .Build();

// Trigger the job to run now, and then every 40 seconds
var trigger = TriggerBuilder.Create()
    .WithIdentity("trigger-azure-service-bus", "service-bus")
    .StartNow()
    .WithSimpleSchedule(x => x
        .WithIntervalInSeconds(2)
        .RepeatForever())
    .Build();


await scheduler.ScheduleJob(job, trigger);

// will block until the last running job completes
await builder.RunAsync();