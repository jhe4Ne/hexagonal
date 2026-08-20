using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Galaxy.Lol.Infraestructure.Configuration.Mongo
{

    public class RawPayloadDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("version")]
        public string Version { get; set; } = string.Empty;

        [BsonElement("locale")]
        public string Locale { get; set; } = string.Empty;

        [BsonElement("championId")]
        public string? ChampionId { get; set; }

        [BsonElement("payload")]
        public string Payload { get; set; } = string.Empty;

        [BsonElement("fetchedAt")]
        public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    }
}
