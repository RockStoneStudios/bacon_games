using Microsoft.AspNetCore.Mvc;
using Challenge.Models.DTOs.Auth;
using Challenge.Repository.Auth;
using BC = BCrypt.Net.BCrypt; // Alias para BCrypt.Net.BCrypt, optimizando la legibilidad del código
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Challenge.Models.Entities.User;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Challenge.Services;
using System;
using Challenge.Services.Auth;

// Namespace dedicado a las operaciones de autenticación de la API
namespace Challenge.Controllers
{

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        // Repositorio para operaciones CRUD de usuarios
        private readonly IAuthRepository _authRepository;
        private readonly IJwtService _jwtService;        // Servicio para generación y validación de tokens JWT


        /// Inicializa una nueva instancia del controlador con las dependencias requeridas.
        /// Lanza una excepción si alguna dependencia no está disponible para garantizar la integridad del sistema.

        public AuthController(IAuthRepository authRepo, IJwtService jwtService)
        {
            _authRepository = authRepo ?? throw new ArgumentNullException(nameof(authRepo), "El repositorio de autenticación no puede ser nulo.");
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService), "El servicio JWT no puede ser nulo.");
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema tras validar que el email no esté en uso.
        /// La contraseña se hashea con BCrypt antes de persistirla para cumplir con estándares de seguridad.
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] RegisterRequest request)
        {
            // 1. Automatic model validation (thanks to [ApiController])
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // 2. Additional manual validation
            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new { Message = "Invalid email format" });
            }

            if (request.Password.Length < 6)
            {
                return BadRequest(new { Message = "Password must be at least 6 characters long" });
            }

            // 3. Check for duplicate email
            var existingUser = await _authRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { Message = "Email is already registered" });
            }

            try
            {
                var user = new User
                {
                    Email = request.Email,
                    Password = BC.HashPassword(request.Password),
                    RefreshToken = null
                };

                await _authRepository.RegisterAsync(user);
                return Ok(new { Message = "User registered successfully" });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { Message = "Internal server error while registering user" });
            }
        }

        // Email validation helper method
        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Inicia sesión de un usuario validando sus credenciales y genera un token JWT junto con un refresh token.
        /// El refresh token permite renovaciones futuras sin requerir credenciales nuevamente.
        /// </summary>

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Busca al usuario por email y valida la contraseña hasheada
                var user = await _authRepository.GetUserByEmailAsync(request.Email);
                if (user == null || !BC.Verify(request.Password, user.Password))
                {
                    return Unauthorized(new { Message = "Invalid credentials" });
                }

                // Genera un token JWT con los claims del usuario (email e ID)
                var token = _jwtService.GenerateToken(user.Email, user.Id.ToString());
                if (string.IsNullOrEmpty(token))
                {
                    throw new SecurityTokenException("No se pudo generar el token JWT.");
                }

                // Actualiza el refresh token para permitir renovaciones seguras
                user.RefreshToken = Guid.NewGuid().ToString();
                await _authRepository.UpdateUserAsync(user);

                return Ok(new LoginResponse { Token = token });
            }
            catch (Exception ex) when (ex is SecurityTokenException)
            {
                throw; // Propaga excepciones de token sin envolver
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error durante el proceso de login. Verifique las credenciales o el servicio.", ex);
            }
        }

        /// <summary>
        /// Cierra la sesión de un usuario invalidando su refresh token.
        /// Requiere un token JWT válido para autenticar la solicitud.
        /// </summary>
        /// <returns>Respuesta HTTP 200 si la sesión se cierra, 401 si no se encuentra el usuario o el token es inválido</returns>

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromServices] ITokenBlacklist tokenBlacklist)
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var expUnix = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

            if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(expUnix))
            {
                return Unauthorized(new { Message = "Invalid token" });
            }

            var expDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expUnix)).UtcDateTime;

            tokenBlacklist.BlacklistToken(jti, expDate);

            return Ok(new { Message = "Logout successful" });

        }
    }
}
