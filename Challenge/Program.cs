using Challenge.Settings;
using Challenge.Models.Mappings;
using Challenge.Repository.Auth;
using Challenge.Repository.UserPokemon;
using Challenge.Repository.Pokemon;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Challenge.Services;
using System.Text; // ¡Este es el namespace que falta!
using Microsoft.OpenApi.Models;
using Challenge.Services.Auth;
using System.IdentityModel.Tokens.Jwt;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));
// Add services to the container.



//Add Product Repository
// builder.Services.AddScoped<IProductRepository, ProductRepository>();
// builder.Services.AddHttpClient<IPokemonRepository,PokemonRepository>();
// Añade esto junto a tus otros servicios:
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUserPokemonRepository, UserPokemonRepository>();
builder.Services.AddHttpClient<IPokemonRepository, PokemonRepository>();
// Asegúrate de registrar JwtService como IJwtService
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddSingleton<ITokenBlacklist, MemoryTokenBlacklist>();

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(PokemonProfile), typeof(UserProfile));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Swagger
builder.Services.AddEndpointsApiExplorer(); // Necesario
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bacon Games", Version = "v1" });

    // Configuración de seguridad para JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT con el prefijo 'Bearer'. Ejemplo: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});




builder.Services.AddSingleton<JwtService>();

// Configuración de autenticación JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
            ),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = false
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var tokenBlacklist = context.HttpContext.RequestServices
                    .GetRequiredService<ITokenBlacklist>();

                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                if (!string.IsNullOrEmpty(jti) && tokenBlacklist.IsTokenBlacklisted(jti))
                {
                    context.Fail("Token has been revoked");
                }

                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

}


app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
