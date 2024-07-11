using ParallelMap;

Func<int>[] funcs = new Func<int>[]
{
        () => { Thread.Sleep(1000); return 1; },
        () => { Thread.Sleep(500); return 2; },
        () => { Thread.Sleep(2000); return 3; },
        () => { Thread.Sleep(500); return 4; },
};

int N = 4;
int timeoutMs = 5000;

try
{ 
    var results = await ParallelMapper.PMapN<int>(N, funcs, timeoutMs);

    Console.WriteLine("Results:");
    foreach (var result in results)
    {
        Console.WriteLine(result);
    }
    Console.ReadLine();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
