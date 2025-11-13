using Quartz;
using UMLMM.Core.Interfaces;
using UMLMM.Core.Services;
using UMLMM.Orchestrator.Configuration;
using UMLMM.Orchestrator.Jobs;

var builder = Host.CreateApplicationBuilder(args);

// Register IDataContext
builder.Services.AddSingleton<IDataContext, InMemoryDataContext>();

// Configure job schedules from configuration
builder.Services.Configure<JobSchedulesConfig>(
    builder.Configuration.GetSection("JobSchedules"));

// Configure Quartz
builder.Services.AddQuartz(q =>
{
    // Use in-memory job store
    q.UseInMemoryStore();

    // Get job schedules configuration
    var jobSchedules = builder.Configuration
        .GetSection("JobSchedules")
        .Get<JobSchedulesConfig>() ?? new JobSchedulesConfig();

    // Register and schedule CivitAI job
    var civitaiJobKey = new JobKey("CivitAIIngestionJob");
    q.AddJob<CivitAIIngestionJob>(opts => opts.WithIdentity(civitaiJobKey));
    q.AddTrigger(opts => opts
        .ForJob(civitaiJobKey)
        .WithIdentity("CivitAIIngestionJob-trigger")
        .WithCronSchedule(jobSchedules.CivitAI.CronSchedule)
        .WithDescription(jobSchedules.CivitAI.Description));

    // Register and schedule Danbooru job
    var danbooruJobKey = new JobKey("DanbooruIngestionJob");
    q.AddJob<DanbooruIngestionJob>(opts => opts.WithIdentity(danbooruJobKey));
    q.AddTrigger(opts => opts
        .ForJob(danbooruJobKey)
        .WithIdentity("DanbooruIngestionJob-trigger")
        .WithCronSchedule(jobSchedules.Danbooru.CronSchedule)
        .WithDescription(jobSchedules.Danbooru.Description));

    // Register and schedule e621 job
    var e621JobKey = new JobKey("E621IngestionJob");
    q.AddJob<E621IngestionJob>(opts => opts.WithIdentity(e621JobKey));
    q.AddTrigger(opts => opts
        .ForJob(e621JobKey)
        .WithIdentity("E621IngestionJob-trigger")
        .WithCronSchedule(jobSchedules.E621.CronSchedule)
        .WithDescription(jobSchedules.E621.Description));

    // Register and schedule ComfyUI job
    var comfyuiJobKey = new JobKey("ComfyUIIngestionJob");
    q.AddJob<ComfyUIIngestionJob>(opts => opts.WithIdentity(comfyuiJobKey));
    q.AddTrigger(opts => opts
        .ForJob(comfyuiJobKey)
        .WithIdentity("ComfyUIIngestionJob-trigger")
        .WithCronSchedule(jobSchedules.ComfyUI.CronSchedule)
        .WithDescription(jobSchedules.ComfyUI.Description));

    // Register and schedule Ollama job
    var ollamaJobKey = new JobKey("OllamaIngestionJob");
    q.AddJob<OllamaIngestionJob>(opts => opts.WithIdentity(ollamaJobKey));
    q.AddTrigger(opts => opts
        .ForJob(ollamaJobKey)
        .WithIdentity("OllamaIngestionJob-trigger")
        .WithCronSchedule(jobSchedules.Ollama.CronSchedule)
        .WithDescription(jobSchedules.Ollama.Description));
});

// Add Quartz hosted service
builder.Services.AddQuartzHostedService(options =>
{
    // Wait for jobs to complete on shutdown
    options.WaitForJobsToComplete = true;
});

var host = builder.Build();
host.Run();
