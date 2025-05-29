using System.Collections.Concurrent;

namespace TaskExecutor;

/// <summary>
/// Manages a collection of running tasks, allowing for tracking, adding, and removing tasks in a thread-safe manner.
/// </summary>
internal class ActiveTaskRegistry
{
    private readonly ConcurrentBag<TaskEntry> _runningTasks;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActiveTaskRegistry"/> class.
    /// </summary>
    internal ActiveTaskRegistry()
    {
        _runningTasks = [];
    }

    /// <summary>
    /// Gets a value indicating whether there are any tasks currently running.
    /// </summary>
    public bool HasRunningTasks
    {
        get
        {
            return _runningTasks.Any(taskKeeper => taskKeeper.IsRunning);
        }
    }

    /// <summary>
    /// Marks the specified task as not running and removes its reference from the collection.
    /// </summary>
    /// <param name="task">The task to remove from the running tasks collection.</param>
    public void Remove(Task task)
    {
        var taskKeeper = _runningTasks.FirstOrDefault(t => t.TaskForKeep == task);
        if (taskKeeper != null)
        {
            taskKeeper.IsRunning = false;
            taskKeeper.TaskForKeep = null;
        }
    }

    /// <summary>
    /// Adds a new task to the collection of running tasks, or reuses a slot for a finished task.
    /// </summary>
    /// <param name="task">The task to add to the running tasks collection.</param>
    public void Add(Task task)
    {
        var finishedTak = _runningTasks.FirstOrDefault(t => !t.IsRunning);
        if (finishedTak != null)
        {
            finishedTak.IsRunning = true;
            finishedTak.TaskForKeep = task;
        }
        else
        {
            _runningTasks.Add(new TaskEntry(task));
        }
    }

    /// <summary>
    /// Returns an array of currently running tasks.
    /// </summary>
    /// <returns>An array of <see cref="Task"/> objects that are currently running.</returns>
    public Task[] GetRunningTasks()
    {
        return _runningTasks
            .Where(t => t.IsRunning && t.TaskForKeep != null)
            .Select(t => t.TaskForKeep!)
            .ToArray();
    }
}
