using System;
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
            if (N <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(N), "N must be greater than zero.");
            }

            if (funcs == null || funcs.Length == 0)
            {
                return new List<T>();
            }
            
            var tasks = new List<Task<T>>();
            var semaphore = new SemaphoreSlim(N, N);
            var results = new List<T>(funcs.Length);

            using (var cts = new CancellationTokenSource(timeoutMs))
            {
                for (int i = 0; i < funcs.Length; i++)
                {
                    tasks.Add(ProcessFunctionAsync(funcs[i], results, i, cts.Token, semaphore));
                }

                await Task.WhenAll(tasks.ToArray());
            }

            return results;
        }

        private static async Task<T> ProcessFunctionAsync<T>(Func<T> func, List<T> results, int index, CancellationToken ct, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync(ct);
            try
            {
                T result;
                try
                {
                    result = func();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Function at index {index} crashed: {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                }
                results.Insert(index, result);
                return result;
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("Timeout reached during parallel execution.");
            }
        }

    }
}
