
var cts = new CancellationTokenSource();
var executor = new TaskExecutor.TaskExecutor(3, cts.Token);

executor.OnTaskError += (id, ex) => Console.WriteLine($"Task {id} error: {ex.Message}");

for (int i = 0; i < 20; i++)
{
    int id = i;
    executor.EnqueueTask(id, async () =>
    {
        Console.WriteLine($"Task {id} started");
        await Task.Delay(Random.Shared.Next(2000, 5000));
        Console.WriteLine($"Task {id} finished");
    });
}

await Task.Delay(100); //Need to give time for the executor to start.

while (executor.HasRunningTasks)
{
    await Task.Delay(1000);
}
await executor.StopAsync();