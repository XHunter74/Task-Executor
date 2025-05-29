namespace TaskExecutor;

/// <summary>
/// Represents an entry for a tracked task, including its running state and reference.
/// </summary>
internal class TaskEntry
{
    /// <summary>
    /// Gets or sets a value indicating whether the task is currently running.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// Gets or sets the reference to the tracked <see cref="Task"/>.
    /// </summary>
    public Task? TaskForKeep { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskEntry"/> class with the specified task.
    /// </summary>
    /// <param name="task">The task to track.</param>
    public TaskEntry(Task task)
    {
        TaskForKeep = task;
        IsRunning = true;
    }
}
