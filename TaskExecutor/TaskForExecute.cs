namespace TaskExecutor;

/// <summary>
/// This class represents a task that is to be executed by the TaskExecutor.
/// </summary>
public class TaskForExecute
{
    /// <summary>
    /// The id of the task. This is used to identify the task in the queue.
    /// </summary>
    public object Id { get; set; }

    /// <summary>
    /// The function to execute. This function should return a Task.
    /// </summary>
    public Func<Task> TaskFunc { get; set; }

    /// <summary>
    /// The constructor for the TaskForExecute class.
    /// </summary>
    /// <param name="id">This parameter represents the unique identifier for the task. It can be any type since it is declared as object. This allows flexibility in how tasks are identified (e.g., using a string, integer, GUID, etc.).</param>
    /// <param name="taskFunc">This parameter is a delegate that represents the function to be executed. The function must return a Task, making it asynchronous.</param>
    public TaskForExecute(object id, Func<Task> taskFunc)
    {
        Id = id;
        TaskFunc = taskFunc;
    }
}
