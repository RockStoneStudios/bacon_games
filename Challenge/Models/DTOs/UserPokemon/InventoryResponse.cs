

namespace Challenge.Models.DTOs.UserPokemon
{
    public class InventoryResponse
    {
        public required List<UserPokemonResponse> Pokemons { get; set; }
    }
}