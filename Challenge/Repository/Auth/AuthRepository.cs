using MongoDB.Driver;
using Challenge.Models.Entities.User;
using Microsoft.Extensions.Options;
using Challenge.Settings;

// Namespace dedicado a las implementaciones de repositorios para operaciones de autenticación
namespace Challenge.Repository.Auth
{
   
    //Implementación del repositorio para gestionar operaciones de autenticación de usuarios en MongoDB.
    //Proporciona métodos para consultar, registrar y actualizar usuarios, utilizando una colección específica.
  
    public class AuthRepository : IAuthRepository
    {
        private readonly IMongoCollection<User> _users; // Colección MongoDB que almacena los documentos de usuario

       
        // Inicializa una nueva instancia del repositorio con la configuración de conexión a MongoDB.
        // Configura el cliente y la colección basada en las settings inyectadas, lanzando una excepción si falla.
     
        public AuthRepository(IOptions<MongoDbSettings> settings)
        {
            if (settings == null || settings.Value == null)
            {
                throw new ArgumentNullException(nameof(settings), "Las configuraciones de MongoDB no pueden ser nulas.");
            }

            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _users = database.GetCollection<User>("User"); // Asigna la colección "User" para operaciones CRUD
        }

       
        /// Busca un usuario en la colección por su dirección de email de forma asíncrona.
        /// Utiliza un filtro LINQ para encontrar el primer coincidencia o null si no existe.
          
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }


        /// Registra un nuevo usuario en la colección de MongoDB de forma asíncrona.
        /// Inserta el documento del usuario tal como se proporciona, asumiendo que la validación previa es correcta.
        
        public async Task RegisterAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }

        // Actualiza un usuario existente en la colección de MongoDB de forma asíncrona.
        // Reemplaza el documento completo basado en el ID del usuario, preservando la integridad del dato.

       
        public async Task UpdateUserAsync(User user)
        {
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }
    }
}