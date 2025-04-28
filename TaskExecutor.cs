using System.Collections.Concurrent;

namespace TaskExecutor;

public class TaskExecutor : IDisposable
{
    private readonly ConcurrentQueue<TaskForExecute> _taskQueue = new();
    private readonly List<Task> _runningTasks = new List<Task>();
    private readonly CancellationTokenSource _internalCts = new();
    private readonly CancellationToken _externalCancellationToken;
    private readonly SemaphoreSlim _semaphore;
    private int _maxConcurrency;

    public event Action<Object, Exception>? OnTaskError;

    public bool HasRunningTasks
    {
        get
        {
            lock (_runningTasks)
            {
                return _runningTasks.Count > 0;
            }
        }
    }

    public TaskExecutor(int initialConcurrency, CancellationToken cancellationToken = default)
    {
        if (initialConcurrency <= 0)
            throw new ArgumentException("Concurrency must be greater than 0");

        _maxConcurrency = initialConcurrency;
        _semaphore = new SemaphoreSlim(_maxConcurrency);
        _externalCancellationToken = cancellationToken;

        StartProcessing();
    }

    public void EnqueueTask(object taskId, Func<Task> taskFunc)
    {
        _taskQueue.Enqueue(new TaskForExecute(taskId, taskFunc));
    }

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
                    await _semaphore.WaitAsync(_internalCts.Token);

                    if (_taskQueue.TryDequeue(out var taskForExecute))
                    {
                        var task = ExecuteTaskAsync(taskForExecute);
                        lock (_runningTasks)
                        {
                            _runningTasks.Add(task);
                        }

                        _ = task.ContinueWith(t =>
                        {
                            lock (_runningTasks)
                            {
                                _runningTasks.Remove(t);
                            }
                            _semaphore.Release();
                        }, TaskContinuationOptions.ExecuteSynchronously);
                    }
                    else
                    {
                        _semaphore.Release();
                        await Task.Delay(50, _internalCts.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
        }, _internalCts.Token);
    }

    private async Task ExecuteTaskAsync(TaskForExecute taskForExecute)
    {
        try
        {
            await taskForExecute.TaskFunc();
        }
        catch (Exception ex)
        {
            OnTaskError?.Invoke(taskForExecute.Id, ex);
        }
    }

    public async Task StopAsync()
    {
        _internalCts.Cancel();

        Task[] running;
        lock (_runningTasks)
        {
            running = _runningTasks.ToArray();
        }

        if (running.Length > 0)
            await Task.WhenAll(running);
    }

    public void Dispose()
    {
        _internalCts.Cancel();
        _semaphore.Dispose();
        _internalCts.Dispose();
    }
}

