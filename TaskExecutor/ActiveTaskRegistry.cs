using System.Collections.Concurrent;

namespace TaskExecutor;

/// <summary>
/// Manages a collection of running tasks, allowing for tracking, adding, and removing tasks in a thread-safe manner.
/// </summary>
internal class ActiveTaskRegistry
{
    private readonly ConcurrentBag<TaskEntry> _runningTasks;
    private readonly object _lock = new();

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
            return _runningTasks.Any(t => t.IsRunning);
        }
    }

    /// <summary>
    /// Marks the specified task as not running and removes its reference from the collection.
    /// </summary>
    /// <param name="task">The task to remove from the running tasks collection.</param>
    public void Remove(Task task)
    {
        lock (_lock)
        {
            var taskEntry = _runningTasks.FirstOrDefault(t => t.TaskReference == task);
            if (taskEntry != null)
            {
                taskEntry.IsRunning = false;
                taskEntry.TaskReference = null;
            }
        }
    }

    /// <summary>
    /// Adds a new task to the collection of running tasks, or reuses a slot for a finished task.
    /// </summary>
    /// <param name="task">The task to add to the running tasks collection.</param>
    public void Add(Task task)
    {
        lock (_lock)
        {
            var taskEntry = _runningTasks.FirstOrDefault(t => !t.IsRunning);
            if (taskEntry != null)
            {
                taskEntry.IsRunning = true;
                taskEntry.TaskReference = task;
            }
            else
            {
                _runningTasks.Add(new TaskEntry(task));
            }
        }
    }

    /// <summary>
    /// Returns an array of currently running tasks.
    /// </summary>
    /// <returns>An array of <see cref="Task"/> objects that are currently running.</returns>
    public Task[] GetRunningTasks()
    {
        lock (_lock)
        {
            return _runningTasks
            .Where(t => t.IsRunning && t.TaskReference != null)
            .Select(t => t.TaskReference!)
            .ToArray();
        }
    }
}
