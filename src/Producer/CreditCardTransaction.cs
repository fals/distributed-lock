public class CreditCardTransaction
{
    public string TransactionId { get; set; }
    public string Owner { get; set; }
    public string AccountNumber { get; set; }
    public string CardNumber { get; set; }
    public string Currency { get; set; }
    public string Amount { get; set; }
    public string MerchantName { get; set; }
    public string Date { get; set; }

    public string DumpAsTabBased()
    {
        return $"{TransactionId}\t{Owner}\t{AccountNumber}\t{CardNumber}\t{Currency}\t{Amount}\t{MerchantName}\t{Date}";
    }
}