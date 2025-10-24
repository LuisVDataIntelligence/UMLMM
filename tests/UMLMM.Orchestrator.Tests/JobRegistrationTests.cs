using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using UMLMM.Core.Interfaces;
using UMLMM.Orchestrator.Configuration;
using UMLMM.Orchestrator.Jobs;

namespace UMLMM.Orchestrator.Tests;

/// <summary>
/// Unit tests for job registration and configuration
/// </summary>
public class JobRegistrationTests
{
    [Fact]
    public void AllJobsAreRegistered()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JobSchedules:CivitAI:CronSchedule"] = "0 0 */6 * * ?",
                ["JobSchedules:Danbooru:CronSchedule"] = "0 0 */4 * * ?",
                ["JobSchedules:E621:CronSchedule"] = "0 0 */4 * * ?",
                ["JobSchedules:ComfyUI:CronSchedule"] = "0 0 */12 * * ?",
                ["JobSchedules:Ollama:CronSchedule"] = "0 0 0 * * ?"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        // Act
        var scheduler = CreateScheduler(services, configuration);
        var jobKeys = scheduler.GetJobKeys(Quartz.Impl.Matchers.GroupMatcher<JobKey>.AnyGroup())
            .GetAwaiter().GetResult();

        // Assert
        Assert.Equal(5, jobKeys.Count);
        Assert.Contains(jobKeys, k => k.Name == "CivitAIIngestionJob");
        Assert.Contains(jobKeys, k => k.Name == "DanbooruIngestionJob");
        Assert.Contains(jobKeys, k => k.Name == "E621IngestionJob");
        Assert.Contains(jobKeys, k => k.Name == "ComfyUIIngestionJob");
        Assert.Contains(jobKeys, k => k.Name == "OllamaIngestionJob");
    }

    [Fact]
    public void JobSchedulesAreConfiguredCorrectly()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JobSchedules:CivitAI:CronSchedule"] = "0 0 */6 * * ?",
                ["JobSchedules:CivitAI:Description"] = "Every 6 hours",
                ["JobSchedules:Danbooru:CronSchedule"] = "0 0 */4 * * ?",
                ["JobSchedules:E621:CronSchedule"] = "0 0 */4 * * ?",
                ["JobSchedules:ComfyUI:CronSchedule"] = "0 0 */12 * * ?",
                ["JobSchedules:Ollama:CronSchedule"] = "0 0 0 * * ?"
            })
            .Build();

        // Act
        var jobSchedules = configuration.GetSection("JobSchedules").Get<JobSchedulesConfig>();

        // Assert
        Assert.NotNull(jobSchedules);
        Assert.Equal("0 0 */6 * * ?", jobSchedules.CivitAI.CronSchedule);
        Assert.Equal("Every 6 hours", jobSchedules.CivitAI.Description);
        Assert.Equal("0 0 */4 * * ?", jobSchedules.Danbooru.CronSchedule);
        Assert.Equal("0 0 */4 * * ?", jobSchedules.E621.CronSchedule);
        Assert.Equal("0 0 */12 * * ?", jobSchedules.ComfyUI.CronSchedule);
        Assert.Equal("0 0 0 * * ?", jobSchedules.Ollama.CronSchedule);
    }

    [Fact]
    public void JobsHaveDisallowConcurrentExecutionAttribute()
    {
        // Assert
        Assert.True(typeof(CivitAIIngestionJob).GetCustomAttributes(typeof(DisallowConcurrentExecutionAttribute), true).Any());
        Assert.True(typeof(DanbooruIngestionJob).GetCustomAttributes(typeof(DisallowConcurrentExecutionAttribute), true).Any());
        Assert.True(typeof(E621IngestionJob).GetCustomAttributes(typeof(DisallowConcurrentExecutionAttribute), true).Any());
        Assert.True(typeof(ComfyUIIngestionJob).GetCustomAttributes(typeof(DisallowConcurrentExecutionAttribute), true).Any());
        Assert.True(typeof(OllamaIngestionJob).GetCustomAttributes(typeof(DisallowConcurrentExecutionAttribute), true).Any());
    }

    private static IScheduler CreateScheduler(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDataContext>(sp => new Core.Services.InMemoryDataContext());
        services.AddLogging();

        services.AddQuartz(q =>
        {
            q.UseInMemoryStore();

            var jobSchedules = configuration
                .GetSection("JobSchedules")
                .Get<JobSchedulesConfig>() ?? new JobSchedulesConfig();

            var civitaiJobKey = new JobKey("CivitAIIngestionJob");
            q.AddJob<CivitAIIngestionJob>(opts => opts.WithIdentity(civitaiJobKey));
            q.AddTrigger(opts => opts
                .ForJob(civitaiJobKey)
                .WithIdentity("CivitAIIngestionJob-trigger")
                .WithCronSchedule(jobSchedules.CivitAI.CronSchedule));

            var danbooruJobKey = new JobKey("DanbooruIngestionJob");
            q.AddJob<DanbooruIngestionJob>(opts => opts.WithIdentity(danbooruJobKey));
            q.AddTrigger(opts => opts
                .ForJob(danbooruJobKey)
                .WithIdentity("DanbooruIngestionJob-trigger")
                .WithCronSchedule(jobSchedules.Danbooru.CronSchedule));

            var e621JobKey = new JobKey("E621IngestionJob");
            q.AddJob<E621IngestionJob>(opts => opts.WithIdentity(e621JobKey));
            q.AddTrigger(opts => opts
                .ForJob(e621JobKey)
                .WithIdentity("E621IngestionJob-trigger")
                .WithCronSchedule(jobSchedules.E621.CronSchedule));

            var comfyuiJobKey = new JobKey("ComfyUIIngestionJob");
            q.AddJob<ComfyUIIngestionJob>(opts => opts.WithIdentity(comfyuiJobKey));
            q.AddTrigger(opts => opts
                .ForJob(comfyuiJobKey)
                .WithIdentity("ComfyUIIngestionJob-trigger")
                .WithCronSchedule(jobSchedules.ComfyUI.CronSchedule));

            var ollamaJobKey = new JobKey("OllamaIngestionJob");
            q.AddJob<OllamaIngestionJob>(opts => opts.WithIdentity(ollamaJobKey));
            q.AddTrigger(opts => opts
                .ForJob(ollamaJobKey)
                .WithIdentity("OllamaIngestionJob-trigger")
                .WithCronSchedule(jobSchedules.Ollama.CronSchedule));
        });

        var serviceProvider = services.BuildServiceProvider();
        var schedulerFactory = serviceProvider.GetRequiredService<ISchedulerFactory>();
        return schedulerFactory.GetScheduler().GetAwaiter().GetResult();
    }
}
