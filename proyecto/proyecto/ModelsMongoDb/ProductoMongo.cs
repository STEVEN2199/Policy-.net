using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace proyecto.ModelsMongoDb
{
    public class ProductoMongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("nombre")]
        public string Nombre { get; set; } = null!;

        [BsonElement("precio")]
        public decimal Precio { get; set; }

        [BsonElement("stock")]
        public int Stock { get; set; }
    }
}
