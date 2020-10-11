using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ProcessTransactionsHostedService : IHostedService, IDisposable
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ProcessTransactionsHostedService> _logger;
    private Timer _timer;

    public ProcessTransactionsHostedService(IServiceProvider services, ILogger<ProcessTransactionsHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(ProcessTransactionsHostedService)} Service running.");

        _timer = new Timer(Process, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));

        return Task.CompletedTask;
    }

    private async void Process(object state)
    {
        using (var scope = _services.CreateScope())
        {
            var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLock>();
            var database = scope.ServiceProvider.GetRequiredService<MongoDb>();
            var fileToProcess = GetNextFileToProcess();
            var fileExists = File.Exists(fileToProcess);

            try
            {
                if (fileExists && await distributedLock.Lock(fileToProcess))
                {
                    var data = GetData(fileToProcess);
                    var parsedData = ParseData(data);

                    await database.CreditCardTransactions.InsertManyAsync(parsedData);

                    File.Delete(fileToProcess);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError("FailedToProcessFile", ex);
            }
            finally
            {
                await distributedLock.Unlock(fileToProcess);
            }
        }
    }

    private CreditCardTransaction[] ParseData(string[] data)
    {
        var parsedData = new CreditCardTransaction[data.Length];

        for (int i = 0; i < data.Length; i++)
        {
            var columns = data[i].Split("\t");

            var transaction = new CreditCardTransaction
            {
                TransactionId = columns[0],
                Owner = columns[1],
                AccountNumber = columns[2],
                CardNumber = columns[3],
                Currency = columns[4],
                Amount = int.Parse(columns[5]) / 100,
                MerchantName = columns[6],
                Date = Convert.ToDateTime(columns[7])
            };

            parsedData[i] = transaction;
        }

        return parsedData;
    }

    private string[] GetData(string nextFile)
    {
        return File.ReadAllLines(nextFile);
    }

    private string GetNextFileToProcess()
    {
        return Directory
            .EnumerateFiles(@"/mnt/fileshare", "*", SearchOption.AllDirectories)
            .LastOrDefault();
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"{nameof(ProcessTransactionsHostedService)} Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}