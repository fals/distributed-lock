using System;

namespace Consumer.Entities
{

    public class CreditCardTransaction
    {
        public CreditCardTransaction()
        {

        }

        public string TransactionId { get; set; }
        public string Owner { get; set; }
        public string AccountNumber { get; set; }
        public string CardNumber { get; set; }
        public string Currency { get; set; }
        public decimal Amount { get; set; }
        public string MerchantName { get; set; }
        public DateTime Date { get; set; }
    }
}