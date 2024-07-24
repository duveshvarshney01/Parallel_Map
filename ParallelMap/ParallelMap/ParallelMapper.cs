using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelMap
{
    public static class ParallelMapper
    {
        public static async Task<List<T>> PMapN<T>(int N, Func<T>[] funcs, int timeoutMs)
        {
            int cpuCores = Environment.ProcessorCount;
            Console.WriteLine($"Number of CPU cores: {cpuCores}");

            if (N <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(N), "N must be greater than zero.");
            }

            if (funcs == null || funcs.Length == 0)
            {
                return new List<T>();
            }

            var tasks = new List<Task>();
            var semaphore = new SemaphoreSlim(N, N);
            var results = new List<T>(new T[funcs.Length]);
            var exceptions = new ConcurrentBag<string>();

            using (var cts = new CancellationTokenSource())
            {
                for (int i = 0; i < funcs.Length; i++)
                {
                    int index = i; // Capture the current index
                    tasks.Add(ProcessFunctionAsync(funcs[index], results, exceptions, index, cts.Token, semaphore, timeoutMs));
                }

                try
                {
                    await Task.WhenAll(tasks.ToArray()); // Wait for all tasks to complete
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
                finally
                {
                    cts.Cancel(); // Cancel remaining tasks
                }
            }

            foreach (var exception in exceptions)
            {
                Console.WriteLine(exception);
            }

            Console.WriteLine("Main thread executed..!");
            return results;
        }

        private static async Task ProcessFunctionAsync<T>(Func<T> func, List<T> results, ConcurrentBag<string> exceptions, int index, CancellationToken ct, SemaphoreSlim semaphore, int timeoutMs)
        {
            await semaphore.WaitAsync(ct);
            try
            {
                var resultTask = Task.Run(func, ct);
                if (await Task.WhenAny(resultTask, Task.Delay(timeoutMs, ct)) == resultTask)
                {
                    results[index] = resultTask.Result; // Store result in correct position`
                }
                else
                {
                    exceptions.Add($"Task at index {index+1} timed out.");
                }
            }
            finally
            {
                semaphore.Release(); // Release semaphore
            }
        }
    }
}
