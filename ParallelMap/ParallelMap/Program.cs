using ParallelMap;

Func<int>[] funcs = new Func<int>[]
{
        () => { Thread.Sleep(1000000); return 1; },
        () => { return 2; },
        () => { Thread.Sleep(100); return 3; },
        () => { return 4; },
        () => { Thread.Sleep(400000); return 5; },
        () => { return 6; },
        () => { Thread.Sleep(100); return 7; },
        () => { return 8; },
};

int N = 2;
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
    Console.ReadLine();
}
