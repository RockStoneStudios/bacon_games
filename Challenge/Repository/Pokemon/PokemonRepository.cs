using System.Net;
using System.Net.Http.Json;
using PokemonEntity = Challenge.Models.Entities.Pokemon.Pokemon;

namespace Challenge.Repository.Pokemon
{

    // Implementación del repositorio que consulta datos de Pokémon desde la PokeAPI.
    // Utiliza HttpClient para realizar solicitudes asíncronas y maneja errores de red.

    public class PokemonRepository : IPokemonRepository
    {
        private readonly HttpClient _httpClient; // Cliente HTTP para interactuar con la PokeAPI

        // Inicializa una nueva instancia del repositorio con un cliente HTTP configurado.
        // Establece la base URL de la PokeAPI como punto de partida para las solicitudes.
   
        public PokemonRepository(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient), "El cliente HTTP no puede ser nulo.");
            _httpClient.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
        }


        /// Obtiene un Pokémon por su ID desde la PokeAPI de forma asíncrona.
        /// Retorna null si no se encuentra y lanza excepción si hay un error de red.
        
        
        public async Task<PokemonEntity?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"pokemon/{id}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Error retrieving pokemon {id}: {response.StatusCode}");

            return await response.Content.ReadFromJsonAsync<PokemonEntity>();
        }

    
        //Obtiene un Pokémon por su nombre desde la PokeAPI de forma asíncrona.
        // Normaliza el nombre a minúsculas y retorna null si no se encuentra.
      public async Task<PokemonEntity?> GetByNameAsync(string name)
        {
            var response = await _httpClient.GetAsync($"pokemon/{name.ToLower()}");

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Error retrieving pokemon {name}: {response.StatusCode}");

            return await response.Content.ReadFromJsonAsync<PokemonEntity>();
        }
    }
}