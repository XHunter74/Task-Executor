using System.Collections.Concurrent;

namespace TaskExecutor;

/// <summary>
/// This class is responsible for executing tasks concurrently.
/// </summary>
public class TaskExecutor : IDisposable
{
    private readonly ConcurrentQueue<TaskForExecute> _taskQueue = new();
    private readonly CancellationTokenSource _internalCts = new();
    private readonly ActiveTaskRegistry _taskRegistry = new();
    private readonly CancellationToken _externalCancellationToken;
    private readonly SemaphoreSlim _semaphore;
    private int _maxConcurrency;
    private bool _disposed;

    /// <summary>
    /// Event that is triggered when a task fails.
    /// </summary>
    public event Action<object, Exception>? OnTaskError;

    /// <summary>
    /// Gets a value indicating whether there are any tasks currently running.
    /// </summary>
    public bool HasRunningTasks
    {
        get
        {
            return _taskRegistry.HasRunningTasks;

        }
    }

    /// <summary>
    /// Constructor for the TaskExecutor class.
    /// </summary>
    /// <param name="initialConcurrency">
    /// Specifies the initial level of concurrency for the TaskExecutor. 
    /// This determines how many tasks can run concurrently. 
    /// Must be greater than 0.
    /// </param>
    /// <param name="cancellationToken">
    /// An optional CancellationToken that can be used to signal cancellation 
    /// of task processing from an external source.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the initialConcurrency is less than or equal to 0.
    /// </exception>
    public TaskExecutor(int initialConcurrency, CancellationToken cancellationToken = default)
    {
        if (initialConcurrency <= 0)
            throw new ArgumentException("Concurrency must be greater than 0");

        _maxConcurrency = initialConcurrency;
        _semaphore = new SemaphoreSlim(_maxConcurrency);
        _externalCancellationToken = cancellationToken;

        StartProcessing();
    }

    /// <summary>
    /// Enqueues a task for execution.
    /// </summary>
    /// <param name="taskId">
    /// The unique identifier for the task. This can be any object that represents the task's identity.
    /// </param>
    /// <param name="taskFunc">
    /// A delegate representing the asynchronous function to be executed. The function must return a Task.
    /// </param>
    public void EnqueueTask(object taskId, Func<Task> taskFunc)
    {
        _taskQueue.Enqueue(new TaskForExecute(taskId, taskFunc));
    }


    /// <summary>
    /// Adjusts the concurrency level of the TaskExecutor.
    /// </summary>
    /// <param name="newConcurrency">
    /// The new concurrency level to set. This determines how many tasks can run concurrently.
    /// Must be greater than 0.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the newConcurrency is less than or equal to 0.
    /// </exception>
    /// <remarks>
    /// If the new concurrency level is higher than the current level, additional permits are released
    /// to the semaphore to allow more tasks to run concurrently. If the new concurrency level is lower,
    /// permits are acquired to reduce the number of tasks that can run concurrently.
    /// </remarks>
    public void ChangeConcurrency(int newConcurrency)
    {
        if (newConcurrency <= 0)
            throw new ArgumentException("Concurrency must be greater than 0");

        lock (_semaphore)
        {
            var diff = newConcurrency - _maxConcurrency;

            if (diff > 0)
            {
                _semaphore.Release(diff);
            }
            else if (diff < 0)
            {
                for (int i = 0; i < -diff; i++)
                {
                    _semaphore.Wait();
                }
            }

            _maxConcurrency = newConcurrency;
        }
    }

    private void StartProcessing()
    {
        Task.Run(async () =>
        {
            try
            {
                while (!_internalCts.Token.IsCancellationRequested && !_externalCancellationToken.IsCancellationRequested)
                {
                    await _semaphore.WaitAsync(_internalCts.Token).ConfigureAwait(false);

                    if (_taskQueue.TryDequeue(out var taskForExecute))
                    {
                        var task = ExecuteTaskAsync(taskForExecute);
                        _taskRegistry.Add(task);
                        _ = task.ContinueWith(t =>
                        {
                            _taskRegistry.Remove(t);
                            _semaphore.Release();
                        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
                    }
                    else
                    {
                        _semaphore.Release();
                        await Task.Delay(50, _internalCts.Token).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
        }, _internalCts.Token).ConfigureAwait(false);
    }

    private async Task ExecuteTaskAsync(TaskForExecute taskForExecute)
    {
        try
        {
            await taskForExecute.TaskFunc().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            OnTaskError?.Invoke(taskForExecute.Id, ex);
        }
    }

    /// <summary>
    /// Stops the TaskExecutor and waits for all running tasks to complete.
    /// </summary>
    /// <returns></returns>
    public async Task StopAsync()
    {
        _internalCts.Cancel();

        var running = _taskRegistry.GetRunningTasks();

        if (running.Length > 0)
            await Task.WhenAll(running).ConfigureAwait(false);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _internalCts.Cancel();
                _semaphore.Dispose();
                _internalCts.Dispose();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

