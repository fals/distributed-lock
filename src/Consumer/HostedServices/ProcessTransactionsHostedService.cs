using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Consumer.Entities;
using Consumer.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Consumer.HostedServices
{
    public class ProcessTransactionsHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ProcessTransactionsHostedService> _logger;
        private Timer _timer;
        //
        // This is the guanrantee that only this node can remove the lock
        //
        private static readonly string _fenceToken = Guid.NewGuid().ToString();

        public ProcessTransactionsHostedService(IServiceProvider services, ILogger<ProcessTransactionsHostedService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"{nameof(ProcessTransactionsHostedService)} Service running.");

            _timer = new Timer(Process, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        private async void Process(object state)
        {
            using (var scope = _services.CreateScope())
            {
                var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLock>();
                var database = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
                var fileToProcess = GetNextFileToProcess();
                var fileExists = File.Exists(fileToProcess);

                try
                {
                    if (fileExists && await distributedLock.Lock(fileToProcess, _fenceToken))
                    {
                        var data = GetData(fileToProcess);
                        var parsedData = ParseData(data);

                        await database.AddRange(parsedData);

                        File.Delete(fileToProcess);

                        _logger.LogInformation("File Processed: {0}", fileToProcess);
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogWarning("Error: {0} | {1}", ex.Message, fileToProcess);
                }
                finally
                {
                    await distributedLock.Unlock(fileToProcess, _fenceToken);
                }
            }
        }

        private IEnumerable<CreditCardTransaction> ParseData(string[] data)
        {
            var parsedData = new List<CreditCardTransaction>(data.Length);

            for (int i = 0; i < data.Length; i++)
            {
                try
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
                        Date = DateTime.ParseExact(columns[7], "yyyyMMdd", CultureInfo.InvariantCulture)
                    };

                    parsedData.Add(transaction);
                }
                catch
                {

                }
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
}