using Challenge.Models.Entities.User;

// Namespace para la definición de la interfaz de repositorio de autenticación
namespace Challenge.Repository.Auth
{
    /// <summary>
    /// Interfaz que define las operaciones básicas de autenticación para usuarios en el sistema.
  
    /// </summary>
    public interface IAuthRepository
    {
    
        /// Registra un nuevo usuario en el sistema de forma asíncrona.     
        Task RegisterAsync(User user);

     
        /// Busca un usuario por email de forma asíncrona.
       
        Task<User?> GetUserByEmailAsync(string email);

      
        /// Actualiza un usuario existente de forma asíncrona.
        Task UpdateUserAsync(User user);
    }
}