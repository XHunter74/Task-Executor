namespace TaskExecutor.Tests;

public class TaskExecutorTests
{
    [Fact]
    public async Task EnqueueTask_ShouldExecuteTasksConcurrently()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var executor = new TaskExecutor(3, cts.Token);
        int completedTasks = 0;

        // Act
        for (int i = 0; i < 10; i++)
        {
            executor.EnqueueTask(i, async () =>
            {
                await Task.Delay(100);
                Interlocked.Increment(ref completedTasks);
            });
        }

        await Task.Delay(500); // Allow time for tasks to complete
        await executor.StopAsync();

        // Assert
        Assert.Equal(10, completedTasks);
    }

    [Fact]
    public async Task ChangeConcurrency_ShouldAdjustConcurrencyLevel()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var executor = new TaskExecutor(1, cts.Token);
        int runningTasks = 0;

        // Act
        for (int i = 0; i < 5; i++)
        {
            executor.EnqueueTask(i, async () =>
            {
                Interlocked.Increment(ref runningTasks);
                await Task.Delay(200);
                Interlocked.Decrement(ref runningTasks);
            });
        }

        await Task.Delay(100); // Allow initial tasks to start
        Assert.Equal(1, runningTasks);

        executor.ChangeConcurrency(3);
        await Task.Delay(100); // Allow new concurrency level to take effect

        // Assert
        Assert.True(runningTasks >= 2);

        await executor.StopAsync();
    }

    [Fact]
    public async Task OnTaskError_ShouldCaptureTaskExceptions()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var executor = new TaskExecutor(3, cts.Token);
        Exception? capturedException = null;

        executor.OnTaskError += (id, ex) => capturedException = ex;

        // Act
        executor.EnqueueTask(1, () => throw new InvalidOperationException("Test exception"));
        await Task.Delay(200); // Allow time for the task to fail
        await executor.StopAsync();

        // Assert
        Assert.NotNull(capturedException);
        Assert.IsType<InvalidOperationException>(capturedException);
    }

    [Fact]
    public async Task StopAsync_ShouldWaitForRunningTasksToComplete()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var executor = new TaskExecutor(3, cts.Token);
        bool taskCompleted = false;

        executor.EnqueueTask(1, async () =>
        {
            await Task.Delay(300);
            taskCompleted = true;
        });

        // Act
        await Task.Delay(100); // Allow time for the task to start
        await executor.StopAsync();

        // Assert
        Assert.True(taskCompleted);
    }
}