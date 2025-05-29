namespace TaskExecutor;

internal class TaskKeeper
{
    public bool IsRunning { get; set; }
    public Task? TaskForKeep { get; set; }

    public TaskKeeper(Task task)
    {
        TaskForKeep = task;
        IsRunning = true;
    }
}
