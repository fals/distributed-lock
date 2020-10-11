public class ConsumerSettings
{
    public ConnectionStrings ConnectionStrings { get; set; }
}

public class ConnectionStrings
{
    public string RedisCache { get; set; }
    public string MongoDb { get; set; }
    public string MongoDatabase { get; set; }
}