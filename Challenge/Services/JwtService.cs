using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Challenge.Services
{
    // Servicio encargado de generar y validar tokens JWT para autenticación.
    // Utiliza configuraciones desde appsettings.json para claves y emisor.
    public class JwtService : IJwtService
    {
        private readonly string _secretKey; // Clave secreta para firmar los tokens
        private readonly string _issuer;    // Emisor del token, definido en la configuración

        // Inicializa el servicio con las configuraciones de JWT desde IConfiguration.
        // Asume que las claves existen en appsettings.json.
        public JwtService(IConfiguration config)
        {
            _secretKey = config["Jwt:SecretKey"] ?? throw new KeyNotFoundException("Jwt:SecretKey no encontrada en la configuración.");
            _issuer = config["Jwt:Issuer"] ?? throw new KeyNotFoundException("Jwt:Issuer no encontrada en la configuración.");
        }

        // Genera un token JWT con los claims proporcionados.
        // El token expira en 1 hora y usa HMAC SHA256 para la firma.
        public string GenerateToken(string email, string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.NameIdentifier, userId), // Identificador único del usuario
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Identificador único del token (para invalidación)
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // Fecha de emisión
                new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64) // Fecha de expiración
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Expiración en 1 hora
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _issuer,
                Audience = "default" // Opcional: Define una audiencia si es necesario
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // Valida un token JWT y verifica su firma, issuer y expiración.
        // Retorna false si la validación falla por cualquier motivo.
        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = false, // Cambiar a true si se usa audiencia
                    ClockSkew = TimeSpan.Zero, // Sin tolerancia para la expiración
                    ValidateLifetime = true // Verificar fecha de expiración
                }, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Método opcional: Extrae el JTI (ID del token) desde un token JWT
        public string GetJtiFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        }
    }
}