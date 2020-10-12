using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bogus;

namespace Producer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Randomizer.Seed = new Random(8675309);

            var builder = new Faker<CreditCardTransaction>()
            .StrictMode(true)
            .RuleFor(o => o.TransactionId, f => f.Random.AlphaNumeric(10))
            .RuleFor(o => o.Owner, f => f.Name.FullName())
            .RuleFor(o => o.CardNumber, f => f.Finance.CreditCardNumber())
            .RuleFor(o => o.AccountNumber, f => f.Finance.Account())
            .RuleFor(o => o.Currency, f => f.Finance.Currency().Code)
            .RuleFor(o => o.Amount, f => f.Finance.Amount(1, 10000, 2).ToString().Replace(".", string.Empty))
            .RuleFor(o => o.MerchantName, f => f.Company.CompanyName())
            .RuleFor(o => o.Date, f => f.Date.Recent().ToString("yyyyMMdd"));

            while (true)
            {
                var filepath = Path.Combine("/mnt/fileshare", Guid.NewGuid().ToString());
                var lines = new List<string>();

                for (int i = 0; i < 1000; i++)
                {
                    var fakeData = builder.Generate();

                    lines.Add(fakeData.DumpAsTabBased());
                }

                File.WriteAllLines(filepath, lines);

                await Task.Delay(1000);
            }
        }
    }
}
