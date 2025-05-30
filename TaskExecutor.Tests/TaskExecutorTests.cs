namespace TaskExecutor.Tests;

public class TaskExecutorTests
{
    [Fact]
    public async Task EnqueueTask_ShouldExecuteTasksConcurrently()
    {
        var cts = new CancellationTokenSource();
        var executor = new TaskExecutor(3, cts.Token);
        int completedTasks = 0;

        for (int i = 0; i < 10; i++)
        {
            executor.EnqueueTask(i, async () =>
            {
                await Task.Delay(100);
                Interlocked.Increment(ref completedTasks);
            });
        }

        await Task.Delay(500);

        while (executor.HasRunningTasks)
        {
            await Task.Delay(100);
        }
        await executor.StopAsync();

        Assert.Equal(10, completedTasks);
    }

    [Fact]
    public async Task ChangeConcurrency_ShouldAdjustConcurrencyLevel()
    {
        var cts = new CancellationTokenSource();
        var executor = new TaskExecutor(1, cts.Token);
        int runningTasks = 0;

        for (int i = 0; i < 5; i++)
        {
            executor.EnqueueTask(i, async () =>
            {
                Interlocked.Increment(ref runningTasks);
                await Task.Delay(200);
                Interlocked.Decrement(ref runningTasks);
            });
        }

        await Task.Delay(100);
        Assert.Equal(1, runningTasks);

        executor.ChangeConcurrency(3);
        await Task.Delay(100);

        Assert.True(runningTasks >= 2);

        await executor.StopAsync();
    }

    [Fact]
    public async Task OnTaskError_ShouldCaptureTaskExceptions()
    {
        var cts = new CancellationTokenSource();
        var executor = new TaskExecutor(3, cts.Token);
        Exception? capturedException = null;

        executor.OnTaskError += (id, ex) => capturedException = ex;

        executor.EnqueueTask(1, () => throw new InvalidOperationException("Test exception"));
        await Task.Delay(200);
        await executor.StopAsync();

        Assert.NotNull(capturedException);
        Assert.IsType<InvalidOperationException>(capturedException);
    }

    [Fact]
    public async Task StopAsync_ShouldWaitForRunningTasksToComplete()
    {
        var cts = new CancellationTokenSource();
        var executor = new TaskExecutor(3, cts.Token);
        bool taskCompleted = false;

        executor.EnqueueTask(1, async () =>
        {
            await Task.Delay(300);
            taskCompleted = true;
        });

        await Task.Delay(100);
        await executor.StopAsync();

        Assert.True(taskCompleted);
    }

    [Fact]
    public void QueuePollingDelayMilliseconds_Setter_ShouldThrowOnNegative()
    {
        var executor = new TaskExecutor(1, CancellationToken.None);
        Assert.Throws<ArgumentException>(() => executor.QueuePollingDelayMilliseconds = -1);
    }

    [Fact]
    public void Constructor_ShouldThrowOnNegativeQueuePollingDelay()
    {
        Assert.Throws<ArgumentException>(() => new TaskExecutor(1, -10));
    }

    [Fact]
    public void Constructor_ShouldThrowOnZeroOrNegativeConcurrency()
    {
        Assert.Throws<ArgumentException>(() => new TaskExecutor(0, CancellationToken.None));
        Assert.Throws<ArgumentException>(() => new TaskExecutor(-1, CancellationToken.None));
    }

    [Fact]
    public void ChangeConcurrency_ShouldThrowOnZeroOrNegative()
    {
        var executor = new TaskExecutor(1, CancellationToken.None);
        Assert.Throws<ArgumentException>(() => executor.ChangeConcurrency(0));
        Assert.Throws<ArgumentException>(() => executor.ChangeConcurrency(-5));
    }
}