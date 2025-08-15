using PokemonEntity = Challenge.Models.Entities.Pokemon.Pokemon;


namespace Challenge.Repository.Pokemon
{

    //Interfaz que define las operaciones para consultar datos de Pokémon desde una fuente externa.
    //Proporciona métodos asíncronos para búsqueda por ID y nombre.

    public interface IPokemonRepository
    {
   
        // Obtiene un Pokémon por su ID de forma asíncrona.
        Task<PokemonEntity?> GetByIdAsync(int id);


        // Obtiene un Pokémon por su nombre de forma asíncrona.       
        Task<PokemonEntity?> GetByNameAsync(string name);
    }
}