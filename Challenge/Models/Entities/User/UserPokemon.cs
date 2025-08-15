using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Challenge.Models.Entities.User
{
    public class UserPokemon
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ObjectId UserId { get; set; }
        public int PokemonId { get; set; }
        public DateTime CapturedDate { get; set; } = DateTime.UtcNow;
    }
}