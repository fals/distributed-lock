using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

public class MongoDb
{
    private readonly IMongoDatabase _database;

    public MongoDb(IOptions<ConsumerSettings> settings)
    {
        var mongoClient = new MongoClient(settings.Value.ConnectionStrings.MongoDb);
        _database = mongoClient.GetDatabase(settings.Value.ConnectionStrings.MongoDatabase);
        Map();
    }

    internal IMongoCollection<CreditCardTransaction> CreditCardTransactions
    {
        get
        {
            return _database.GetCollection<CreditCardTransaction>("CreditCardTransactions");
        }
    }

    private void Map()
    {
        BsonClassMap.RegisterClassMap<CreditCardTransaction>(cm =>
        {
            cm.AutoMap();
        });
    }
}