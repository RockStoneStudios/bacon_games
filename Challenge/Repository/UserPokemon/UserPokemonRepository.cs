using MongoDB.Driver;
using Challenge.Models.Entities.User;
using Microsoft.Extensions.Options;
using Challenge.Settings;
using MongoDB.Bson;


namespace Challenge.Repository.UserPokemon
{

    /// Implementación del repositorio que gestiona el inventario de Pokémon de un usuario en MongoDB.
    /// Utiliza una colección de usuarios para almacenar y consultar los IDs de Pokémon.

    public class UserPokemonRepository : IUserPokemonRepository
    {
        private readonly IMongoCollection<User> _user; // Colección MongoDB que almacena los documentos de usuario

        /// Inicializa una nueva instancia del repositorio con la configuración de MongoDB.
        /// Configura el cliente y la colección basada en las settings inyectadas.
       
        public UserPokemonRepository(IOptions<MongoDbSettings> settings)
        {
            if (settings == null || settings.Value == null)
            {
                throw new ArgumentNullException(nameof(settings), "Las configuraciones de MongoDB no pueden ser nulas.");
            }

            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _user = database.GetCollection<User>("User");
        }

        // Añade un Pokémon al inventario de un usuario de forma asíncrona.
        // Utiliza un filtro por ID y actualiza la colección de Pokémon.
        
        public async Task AddPokemonAsync(string userId, int pokemonId)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, ObjectId.Parse(userId));
            var update = Builders<User>.Update.AddToSet(u => u.PokemonCollection, pokemonId);
            await _user.UpdateOneAsync(filter, update);
        }

       
        // Obtiene la lista de IDs de Pokémon del inventario de un usuario de forma asíncrona.
        // Retorna una lista vacía si el usuario no existe o no tiene Pokémon.
       
        // <exception cref="FormatException">Se lanza si userId no es un ObjectId válido</exception>
        public async Task<List<int>> GetUserPokemonsAsync(string userId)
        {
            var user = await _user
                .Find(u => u.Id == ObjectId.Parse(userId))
                .FirstOrDefaultAsync();
            return user?.PokemonCollection ?? new List<int>();
        }
    }
}