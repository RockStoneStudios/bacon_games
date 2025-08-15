using Microsoft.AspNetCore.Mvc;
using Challenge.Repository.Pokemon;
using Challenge.Models.Entities.Pokemon;
using System;

// Namespace dedicado a las operaciones relacionadas con Pokémon en la API
namespace Challenge.Controllers
{
 
    // Controlador encargado de gestionar las operaciones de consulta de Pokémon.
    // Obtiene datos desde un repositorio que interactúa con la PokeAPI, proporcionando endpoints para búsqueda por ID y nombre.
 
    [ApiController]
    [Route("api/pokemon")]
    public class PokemonController : ControllerBase
    {
        private readonly IPokemonRepository _pokemonRepository; // Repositorio para acceder a datos de Pokémon desde la PokeAPI

       
        // Inicializa una nueva instancia del controlador con la dependencia requerida.
        // Lanza una excepción si el repositorio no está disponible para garantizar la funcionalidad del controlador.
           
        public PokemonController(IPokemonRepository pokemonRepository)
        {
            _pokemonRepository = pokemonRepository ?? throw new ArgumentNullException(nameof(pokemonRepository), "El repositorio de Pokémon no puede ser nulo.");
        }

        
        /// Obtiene un Pokémon específico desde la PokeAPI utilizando su identificador único (ID).
        /// Retorna el Pokémon si existe, o un código 404 si no se encuentra.
               
        [HttpGet("{id}")]
        public async Task<ActionResult<Pokemon>> GetPokemonById(int id)
        {
            try
            {
                // Valida que el ID sea positivo para evitar consultas inválidas
                if (id <= 0)
                {
                    return BadRequest(new { Message = "El ID del Pokémon debe ser un valor positivo." });
                }

                // Consulta el Pokémon por ID desde la PokeAPI a través del repositorio
                var pokemon = await _pokemonRepository.GetByIdAsync(id);
                if (pokemon == null)
                {
                    return NotFound(new { Message = $"No se encontró un Pokémon con el ID {id}." });
                }

                return Ok(pokemon);
            }
            catch (HttpRequestException ex)
            {
                // Captura errores de red o fallos en la PokeAPI
                throw new HttpRequestException("Error al conectar con la PokeAPI. Verifique su conexión o el estado del servicio.", ex);
            }
            catch (Exception ex)
            {
                // Maneja errores inesperados del repositorio o procesamiento de datos
                throw new InvalidOperationException("Error al obtener el Pokémon. Revise los logs para más detalles.", ex);
            }
        }


        // Obtiene un Pokémon específico desde la PokeAPI utilizando su nombre.
        // Retorna el Pokémon si existe, o un código 404 si no se encuentra.
       
        [HttpGet("name/{name}")]
        public async Task<ActionResult<Pokemon>> GetPokemonByName(string name)
        {
            try
            {
                // Valida que el nombre no esté vacío para evitar consultas inválidas
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest(new { Message = "El nombre del Pokémon no puede estar vacío." });
                }

                // Consulta el Pokémon por nombre desde la PokeAPI a través del repositorio
                var pokemon = await _pokemonRepository.GetByNameAsync(name);
                if (pokemon == null)
                {
                    return NotFound(new { Message = $"No se encontró un Pokémon con el nombre '{name}'." });
                }

                return Ok(pokemon);
            }
            catch (HttpRequestException ex)
            {
                // Captura errores de red o fallos en la PokeAPI
                throw new HttpRequestException("Error al conectar con la PokeAPI. Verifique su conexión o el estado del servicio.", ex);
            }
            catch (Exception ex)
            {
                // Maneja errores inesperados del repositorio o procesamiento de datos
                throw new InvalidOperationException("Error al obtener el Pokémon por nombre. Revise los logs para más detalles.", ex);
            }
        }
    }
}