namespace TaskExecutor.Tests;

public class ActiveTaskRegistryTests
{
    [Fact]
    public void Add_AddsNewTask_WhenNoFinishedTaskExists()
    {
        var registry = new ActiveTaskRegistry();
        var task = Task.CompletedTask;

        registry.Add(task);

        var runningTasks = registry.GetRunningTasks();
        Assert.Single(runningTasks);
        Assert.Equal(task, runningTasks[0]);
        Assert.True(registry.HasRunningTasks);
    }

    [Fact]
    public void Add_ReusesFinishedTaskEntry_WhenAvailable()
    {
        var registry = new ActiveTaskRegistry();
        var task1 = Task.CompletedTask;
        var task2 = Task.FromResult(42);

        registry.Add(task1);
        registry.Remove(task1);
        registry.Add(task2);

        var runningTasks = registry.GetRunningTasks();
        Assert.Single(runningTasks);
        Assert.Equal(task2, runningTasks[0]);
        Assert.True(registry.HasRunningTasks);
    }

    [Fact]
    public void Remove_MarksTaskAsNotRunning()
    {
        var registry = new ActiveTaskRegistry();
        var task = Task.CompletedTask;
        registry.Add(task);

        registry.Remove(task);

        Assert.False(registry.HasRunningTasks);
        Assert.Empty(registry.GetRunningTasks());
    }

    [Fact]
    public void GetRunningTasks_ReturnsOnlyRunningTasks()
    {
        var registry = new ActiveTaskRegistry();
        var task1 = Task.CompletedTask;
        var task2 = Task.FromResult(42);
        registry.Add(task1);
        registry.Add(task2);
        registry.Remove(task1);

        var runningTasks = registry.GetRunningTasks();
        Assert.Single(runningTasks);
        Assert.Equal(task2, runningTasks[0]);
    }

    [Fact]
    public void HasRunningTasks_ReturnsFalse_WhenNoTasksRunning()
    {
        var registry = new ActiveTaskRegistry();
        Assert.False(registry.HasRunningTasks);
    }

    [Fact]
    public void Remove_DoesNothing_IfTaskNotFound()
    {
        var registry = new ActiveTaskRegistry();
        var task = Task.CompletedTask;
        // Should not throw
        registry.Remove(task);
        Assert.False(registry.HasRunningTasks);
    }

    [Fact]
    public void Add_MultipleTasks_AllTracked()
    {
        var registry = new ActiveTaskRegistry();
        var tasks = new[] { Task.CompletedTask, Task.FromResult(1), Task.FromResult(2) };
        foreach (var t in tasks)
            registry.Add(t);

        var runningTasks = registry.GetRunningTasks();
        Assert.Equal(3, runningTasks.Length);
        Assert.All(tasks, t => Assert.Contains(t, runningTasks));
    }
}
