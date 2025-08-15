

namespace Challenge.Models.DTOs.Pokemon
{
    public class PokemonResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required List<PokemonTypeResponse> Types { get; set; }
    }

    
}