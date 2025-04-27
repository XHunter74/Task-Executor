
var cts = new CancellationTokenSource();
var executor = new TaskExecutor.TaskExecutor(3, cts.Token);

executor.OnTaskError += ex => Console.WriteLine($"Error: {ex.Message}");

for (int i = 0; i < 20; i++)
{
    int id = i;
    executor.EnqueueTask(async () =>
    {
        Console.WriteLine($"Task {id} started");
        await Task.Delay(Random.Shared.Next(2000, 5000));
        Console.WriteLine($"Task {id} finished");
    });
}

await Task.Delay(1000);

while (executor.HasRunningTasks)
{
    await Task.Delay(1000);
}
await executor.StopAsync();