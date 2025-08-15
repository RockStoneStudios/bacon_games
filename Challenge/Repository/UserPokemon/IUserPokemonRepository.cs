
namespace Challenge.Repository.UserPokemon
{
   
    /// Interfaz que define las operaciones para gestionar el inventario de Pokémon de un usuario.
    /// Proporciona métodos asíncronos para añadir y consultar Pokémon.

    public interface IUserPokemonRepository
    {
       
        // Añade un Pokémon al inventario de un usuario de forma asíncrona.
        Task AddPokemonAsync(string userId, int pokemonId);

        // Obtiene la lista de IDs de Pokémon del inventario de un usuario de forma asíncrona.       
        Task<List<int>> GetUserPokemonsAsync(string userId);
    }
}