# TaskExecutor

TaskExecutor is a .NET 8.0 console application that provides a concurrency-controlled task execution framework. It allows you to enqueue tasks and execute them with a specified level of concurrency.

## Features

- **Limited Concurrent Tasks**: Restrict the number of tasks running simultaneously.
- **Automatic Task Execution**: Automatically start a new task as soon as one completes.
- **Concurrency Control**: Dynamically adjust the number of concurrent tasks.
- **Task Queue**: Enqueue tasks with unique IDs and execute them asynchronously.
- **Error Handling**: Capture and handle task-specific errors via an event.
- **Graceful Shutdown**: Stop the executor and wait for running tasks to complete.

## Getting Started

### Prerequisites

- .NET 6.0 SDK

### Building and Running

1. Clone the repository or download the source code.
2. Open a terminal in the project directory.
3. Build the project:
   ```bash
   dotnet build
   ```
4. Run the project:
   ```bash
   dotnet run
   ```

### Example Usage

The `Program.cs` file demonstrates how to use the `TaskExecutor` class:

- Create a `TaskExecutor` instance with a specified concurrency level.
- Enqueue tasks with unique IDs and asynchronous functions.
- Handle task errors using the `OnTaskError` event.
- Monitor running tasks and stop the executor gracefully.

```csharp
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

await Task.Delay(1000);

while (executor.HasRunningTasks)
{
    await Task.Delay(1000);
}
await executor.StopAsync();
```

## Project Structure

- `TaskExecutor.cs`: Core implementation of the `TaskExecutor` class.
- `TaskForExecute.cs`: Represents a task with an ID and a function to execute.
- `Program.cs`: Example usage of the `TaskExecutor`.
- `TaskExecutor.csproj`: Project file for building the application.

## License

MIT License

Copyright (c) 2025 Serhiy Krasovskyy xhunter74@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
