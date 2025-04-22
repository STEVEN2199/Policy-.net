using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using proyecto.ModelsMongoDb;

namespace proyecto.Context
{
    public class MongoDbContext: DbContext
    {

        private readonly IMongoDatabase _database;
        private readonly string _collectionName;

        public MongoDbContext(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
            _collectionName = settings.Value.CollectionName;
        }

        public IMongoCollection<ProductoMongo> ProductosMongo => _database.GetCollection<ProductoMongo>(_collectionName);
        
        public class MongoDBSettings
        {
            public string ConnectionString { get; set; } = null!;
            public string DatabaseName { get; set; } = null!;
            public string CollectionName { get; set; } = null!;
        }

    }
}
