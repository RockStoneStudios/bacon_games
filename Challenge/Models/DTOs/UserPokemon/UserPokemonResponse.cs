

namespace Challenge.Models.DTOs.UserPokemon
{

    public class UserPokemonResponse
    {
        public required int PokemonId { get; set; }
        public required string Name { get; set; }
        public DateTime CapturedDate { get; set; }
    }
}
