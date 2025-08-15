using Microsoft.AspNetCore.Authorization;
using Challenge.Repository.UserPokemon;
using System.Security.Claims;
using MongoDB.Bson;
using Microsoft.AspNetCore.Mvc;
using Challenge.Repository.Pokemon;
using Challenge.Models.Entities.Pokemon;

// Namespace dedicado a las operaciones de gestión de Pokémon de usuario en la API
namespace Challenge.Controllers
{
  
    // Controlador encargado de gestionar la interacción de un usuario autenticado con su inventario de Pokémon.
    // Requiere autenticación y utiliza repositorios para acceder a datos de Pokémon y el inventario del usuario.
  
    [Authorize]
    [ApiController]
    [Route("api/user/pokemon")]
    public class UserPokemonController : ControllerBase
    {
        private readonly IUserPokemonRepository _userPokemonRepository; // Repositorio para gestionar el inventario de Pokémon del usuario
        private readonly IPokemonRepository _pokemonRepository;        // Repositorio para consultar datos de Pokémon desde la PokeAPI

 
        /// Inicializa una nueva instancia del controlador con las dependencias requeridas.
        /// Lanza una excepción si alguna dependencia no está disponible para garantizar la integridad del sistema.

       
        public UserPokemonController(
            IUserPokemonRepository userPokemonRepository,
            IPokemonRepository pokemonRepository)
        {
            _userPokemonRepository = userPokemonRepository ?? throw new ArgumentNullException(nameof(userPokemonRepository), "El repositorio de inventario de usuario no puede ser nulo.");
            _pokemonRepository = pokemonRepository ?? throw new ArgumentNullException(nameof(pokemonRepository), "El repositorio de Pokémon no puede ser nulo.");
        }

      
        /// Método auxiliar para obtener el ID del usuario autenticado desde el token JWT.
        /// Retorna null si no se puede extraer el identificador, lo que indica un problema de autenticación.
       
      
        private string? GetAuthenticatedUserId()
        {
            return User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

  
        /// Añade un Pokémon al inventario del usuario autenticado basándose en su ID.
        /// Valida la autenticación, verifica la existencia del Pokémon y persiste la operación en el repositorio.
   
     
        [HttpPost("add")]
        public async Task<IActionResult> AddPokemon([FromBody] int pokemonId)
        {
            // Verifica que el usuario esté autenticado antes de proceder
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new { Message = "Authentication required" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { Message = "Invalid user identifier" });
            }

            // Comprueba la existencia del Pokémon en la PokeAPI para evitar inventarios inválidos
            var pokemon = await _pokemonRepository.GetByIdAsync(pokemonId);
            if (pokemon == null)
            {
                return BadRequest(new { Message = "The pokemon does not exist" });
            }

            try
            {
                // Añade el Pokémon al inventario del usuario; cualquier error se captura en el bloque try
                await _userPokemonRepository.AddPokemonAsync(userId, pokemonId);
                return Ok(new { Message = $"Pokémon {pokemon.Name} added to inventory" });
            }
            catch (Exception ex)
            {
                // Retorna un error interno con detalles para depuración; considera loggear este error en producción
                return StatusCode(500, new { Message = "Failed to add Pokémon", Error = ex.Message });
            }
        }

        /// Recupera la lista de Pokémon en el inventario del usuario autenticado.
        /// Utiliza consultas asíncronas para optimizar la obtención de datos desde la PokeAPI.       
        [HttpGet("inventory")]
        public async Task<ActionResult<List<Pokemon>>> GetInventory()
        {
            // Verifica la autenticación del usuario en pasos para mayor claridad y seguridad
            if (User?.Identity?.IsAuthenticated != true)
            {
                return Unauthorized(new { Message = "Authentication required" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { Message = "Invalid user identifier" });
            }

            try
            {
                // Obtiene los IDs de los Pokémon asociados al usuario
                var pokemonIds = await _userPokemonRepository.GetUserPokemonsAsync(userId);

                // Optimiza la recuperación paralela de Pokémon usando Task.WhenAll para mejorar el rendimiento
                var inventory = (await Task.WhenAll(
                    pokemonIds.Select(async id => await _pokemonRepository.GetByIdAsync(id))
                ))
                .Where(p => p != null) // Filtra resultados nulos por si la PokeAPI falla en alguna consulta
                .ToList();

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                // Retorna un error interno; en un entorno real, loggea este error para análisis posterior
                return StatusCode(500, new { 
                    Message = "Failed to retrieve inventory", 
                    Error = ex.Message 
                });
            }
        }
    }
}