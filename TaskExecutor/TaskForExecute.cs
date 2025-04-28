namespace TaskExecutor;

public class TaskForExecute
{
    public object Id { get; set; }
    public Func<Task> TaskFunc { get; set; }

    public TaskForExecute(object id, Func<Task> taskFunc)
    {
        Id = id;
        TaskFunc = taskFunc;
    }
}
