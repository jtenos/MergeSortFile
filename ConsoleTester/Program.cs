using MergeSortFile;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddSingleton<LineSorter>();
        services.AddHostedService<Worker>();  
    })
    .Build()
    .Run();

class Worker
    : BackgroundService
{
    private readonly LineSorter _lineSorter;

    public Worker(LineSorter lineSorter)
    {
        _lineSorter = lineSorter;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (StreamWriter writer = new(@"C:\temp\file1.txt"))
        {
            for (int i = 0; i < 5_000_000; ++i)
            {
                if (i % 100_000 == 0) { Console.WriteLine($"{i:#,##0}"); }
                writer.WriteLine(Guid.NewGuid().ToString("N"));
            }
        }
        _lineSorter.SortFile(@"C:\temp\file1.txt", @"C:\temp\file1.txt.sorted", numLinesPerTempFile: 10_000);

        Console.WriteLine("DONE");
        Environment.Exit(0);
        return Task.CompletedTask;
    }
}
