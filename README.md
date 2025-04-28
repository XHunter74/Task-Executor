# TaskExecutor

TaskExecutor is a .NET 6.0 library that provides a concurrency-controlled task execution framework. It allows you to enqueue tasks and execute them with a specified level of concurrency.

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

### Using the Library

To use the `TaskExecutor` library in your project:

1. Add a reference to the `TaskExecutor` project or include the compiled DLL in your application.
2. Create an instance of the `TaskExecutor` class and configure it as needed.

### Example Test Application

A test application is included in the repository to demonstrate how to use the `TaskExecutor` library. The test application is located in the `TaskExecutorApp` project.

#### Running the Test Application

1. Open a terminal in the project directory.
2. Build the solution:
   ```bash
   dotnet build
   ```
3. Run the test application:
   ```bash
   dotnet run --project TaskExecutorApp
   ```

#### Example Code

The `Program.cs` file in the `TaskExecutorApp` project demonstrates how to use the `TaskExecutor` library:

```csharp
var cts = new CancellationTokenSource();
using var executor = new TaskExecutor.TaskExecutor(3, cts.Token);

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

- `TaskExecutor/`: Contains the `TaskExecutor` library source code.
  - `TaskExecutor.cs`: Core implementation of the `TaskExecutor` class.
  - `TaskForExecute.cs`: Represents a task with an ID and a function to execute.
- `TaskExecutorApp/`: Test application demonstrating the usage of the `TaskExecutor` library.
  - `Program.cs`: Example usage of the `TaskExecutor`.
- `TaskExecutor.sln`: Solution file for building the library and test application.

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
