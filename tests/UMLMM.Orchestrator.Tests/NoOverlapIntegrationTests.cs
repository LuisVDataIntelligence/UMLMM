using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Quartz;
using UMLMM.Core.Interfaces;
using UMLMM.Core.Models;
using UMLMM.Core.Services;
using UMLMM.Orchestrator.Jobs;

namespace UMLMM.Orchestrator.Tests;

/// <summary>
/// Integration tests for validating no-overlap functionality per source
/// </summary>
public class NoOverlapIntegrationTests
{
    [Fact]
    public async Task TwoJobsForSameSource_ShouldNotOverlap()
    {
        // Arrange
        var dataContext = new InMemoryDataContext();
        var logger = new Mock<ILogger<CivitAIIngestionJob>>();
        var job = new CivitAIIngestionJob(dataContext, logger.Object);

        // Create a mock job execution context
        var context1 = CreateJobExecutionContext();
        var context2 = CreateJobExecutionContext();

        // Start first job but don't await - simulate long-running job
        var firstJobTask = Task.Run(async () =>
        {
            // Manually set a running status before execution
            var fetchRun = await dataContext.CreateFetchRunAsync(DataSource.CivitAI);
            fetchRun.Status = FetchRunStatus.Running;
            await dataContext.UpdateFetchRunAsync(fetchRun);

            // Simulate job taking time
            await Task.Delay(100);

            // Now execute the job which will see the running status
            await job.Execute(context1);
        });

        // Wait for first job to start
        await Task.Delay(50);

        // Act - Try to execute second job while first is running
        await job.Execute(context2);

        // Wait for first job to complete
        await firstJobTask;

        // Assert - Verify second job was skipped
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("already running")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task TwoJobsForDifferentSources_ShouldRunConcurrently()
    {
        // Arrange
        var dataContext = new InMemoryDataContext();
        var civitaiLogger = new Mock<ILogger<CivitAIIngestionJob>>();
        var danbooruLogger = new Mock<ILogger<DanbooruIngestionJob>>();

        var civitaiJob = new CivitAIIngestionJob(dataContext, civitaiLogger.Object);
        var danbooruJob = new DanbooruIngestionJob(dataContext, danbooruLogger.Object);

        var context1 = CreateJobExecutionContext();
        var context2 = CreateJobExecutionContext();

        // Act - Execute both jobs concurrently
        var task1 = civitaiJob.Execute(context1);
        var task2 = danbooruJob.Execute(context2);

        await Task.WhenAll(task1, task2);

        // Assert - Both jobs should have completed without skipping
        var civitaiRun = await dataContext.GetLatestFetchRunAsync(DataSource.CivitAI);
        var danbooruRun = await dataContext.GetLatestFetchRunAsync(DataSource.Danbooru);

        Assert.NotNull(civitaiRun);
        Assert.NotNull(danbooruRun);
        Assert.Equal(FetchRunStatus.Completed, civitaiRun.Status);
        Assert.Equal(FetchRunStatus.Completed, danbooruRun.Status);
    }

    [Fact]
    public async Task Job_ShouldRecordFetchRunStatistics()
    {
        // Arrange
        var dataContext = new InMemoryDataContext();
        var logger = new Mock<ILogger<CivitAIIngestionJob>>();
        var job = new CivitAIIngestionJob(dataContext, logger.Object);
        var context = CreateJobExecutionContext();

        // Act
        await job.Execute(context);

        // Assert
        var fetchRun = await dataContext.GetLatestFetchRunAsync(DataSource.CivitAI);
        Assert.NotNull(fetchRun);
        Assert.Equal(DataSource.CivitAI, fetchRun.Source);
        Assert.Equal(FetchRunStatus.Completed, fetchRun.Status);
        Assert.NotNull(fetchRun.EndTime);
        Assert.True(fetchRun.DurationMs > 0);
        Assert.True(fetchRun.RecordsFetched > 0);
        Assert.True(fetchRun.RecordsProcessed > 0);
    }

    [Fact]
    public async Task Job_WhenCancelled_ShouldRecordCancelledStatus()
    {
        // Arrange
        var dataContext = new InMemoryDataContext();
        var logger = new Mock<ILogger<CivitAIIngestionJob>>();
        var job = new TestCancellableJob(dataContext, logger.Object);
        
        var cts = new CancellationTokenSource();
        var context = CreateJobExecutionContext(cts.Token);

        // Act
        var executeTask = job.Execute(context);
        await Task.Delay(50); // Let the job start
        cts.Cancel();
        
        try
        {
            await executeTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }

        // Assert
        var fetchRun = await dataContext.GetLatestFetchRunAsync(DataSource.CivitAI);
        Assert.NotNull(fetchRun);
        Assert.Equal(FetchRunStatus.Cancelled, fetchRun.Status);
        Assert.Contains("cancelled", fetchRun.ErrorMessage ?? "", StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Job_WhenFailed_ShouldRecordFailureDetails()
    {
        // Arrange
        var dataContext = new InMemoryDataContext();
        var logger = new Mock<ILogger<CivitAIIngestionJob>>();
        var job = new TestFailingJob(dataContext, logger.Object);
        var context = CreateJobExecutionContext();

        // Act
        await job.Execute(context);

        // Assert
        var fetchRun = await dataContext.GetLatestFetchRunAsync(DataSource.CivitAI);
        Assert.NotNull(fetchRun);
        Assert.Equal(FetchRunStatus.Failed, fetchRun.Status);
        Assert.NotNull(fetchRun.ErrorMessage);
        Assert.NotNull(fetchRun.ErrorDetails);
        Assert.Contains("Test failure", fetchRun.ErrorMessage);
    }

    private static IJobExecutionContext CreateJobExecutionContext(CancellationToken cancellationToken = default)
    {
        var context = new Mock<IJobExecutionContext>();
        context.Setup(c => c.CancellationToken).Returns(cancellationToken);
        return context.Object;
    }

    private class TestCancellableJob : BaseIngestionJob
    {
        protected override DataSource Source => DataSource.CivitAI;

        public TestCancellableJob(IDataContext dataContext, ILogger logger) 
            : base(dataContext, logger)
        {
        }

        protected override async Task ExecuteIngestionAsync(FetchRun fetchRun, IJobExecutionContext context)
        {
            // Simulate work that can be cancelled
            await Task.Delay(5000, context.CancellationToken);
        }
    }

    private class TestFailingJob : BaseIngestionJob
    {
        protected override DataSource Source => DataSource.CivitAI;

        public TestFailingJob(IDataContext dataContext, ILogger logger) 
            : base(dataContext, logger)
        {
        }

        protected override Task ExecuteIngestionAsync(FetchRun fetchRun, IJobExecutionContext context)
        {
            throw new InvalidOperationException("Test failure");
        }
    }
}
