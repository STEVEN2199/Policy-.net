using MongoDB.Driver;
using proyecto.Context;
using proyecto.ModelsMongoDb;

namespace proyecto.ServicesMongo
{
    public class ProductoServiceMongo
    {
        private readonly IMongoCollection<ProductoMongo> _productosMongo;

        public ProductoServiceMongo(MongoDbContext mongoDbContext)
        {
            _productosMongo = mongoDbContext.ProductosMongo;
        }

        public async Task<List<ProductoMongo>> GetAllAsync() =>
            await _productosMongo.Find(_ => true).ToListAsync();

        public async Task<ProductoMongo?> GetByIdAsync(string id) =>
            await _productosMongo.Find(p => p.Id == id).FirstOrDefaultAsync();

        public async Task CreateAsync(ProductoMongo productosMongo) =>
            await _productosMongo.InsertOneAsync(productosMongo);

        public async Task UpdateAsync(string id, ProductoMongo productosMongo) =>
            await _productosMongo.ReplaceOneAsync(p => p.Id == id, productosMongo);

        public async Task DeleteAsync(string id) =>
            await _productosMongo.DeleteOneAsync(p => p.Id == id);
    }
}
