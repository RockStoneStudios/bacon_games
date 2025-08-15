using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace Challenge.Models.Entities.User
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string? RefreshToken { get; set; }
        public List<int> PokemonCollection { get; set; } = new List<int>();
    }
}