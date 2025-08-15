using System.Text.Json.Serialization;

namespace Challenge.Models.Entities.Pokemon
{
    public class Pokemon
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public  List<PokemonTypeSlot> Types { get; set; }
    }

   public class PokemonTypeSlot
    {
        public int Slot { get; set; }
        [JsonPropertyName("type")]
        public PokemonType Type { get; set; } // Tipo anidado
    }

    public class PokemonType
    {
        public string Name { get; set; }
        public string Url { get; set; }
    }
}