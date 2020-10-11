using System.Collections.Generic;
using System.Threading.Tasks;
using Consumer.Entities;

namespace Consumer.Infrastructure
{
    public interface ITransactionRepository
    {
        Task AddRange(IEnumerable<CreditCardTransaction> transactions);
    }

    public class TransactionRepository : ITransactionRepository
    {
        private readonly MongoDb database;

        public async Task AddRange(IEnumerable<CreditCardTransaction> transactions)
        {
            await database.CreditCardTransactions.InsertManyAsync(transactions);
        }
    }
}