using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Challenge;
using Challenge.Models.DTOs.Auth;
using Challenge.Models.Entities.User;
using Challenge.Repository.Auth;
using Challenge.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

public class AuthControllerIntegrationTests 
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Configuramos autenticación fake
                services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>("Test", _ => { });

                // Sustitución de dependencias por versiones en memoria
                services.AddSingleton<IAuthRepository, InMemoryAuthRepository>();
                services.AddSingleton<IJwtService, FakeJwtService>();
            });
        });
    }

    [Fact]
    public async Task Full_Auth_Flow_Works()
    {
        var client = _factory.CreateClient();

        // 1️⃣ Register
        var registerRequest = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "123456"
        };

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // 2️⃣ Login
        var loginRequest = new LoginRequest
        {
            Email = "test@example.com",
            Password = "123456"
        };

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginData = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrEmpty(loginData?.Token));

        // 3️⃣ Logout
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginData!.Token);

        var logoutResponse = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);
    }
}

// Fake auth handler para saltar la validación JWT real
public class FakeAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public FakeAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Email, "test@example.com") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// Implementación fake de IAuthRepository
public class InMemoryAuthRepository : IAuthRepository
{
    private readonly List<User> _users = new();

    public Task<User?> GetUserByEmailAsync(string email) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Email == email));

    public Task RegisterAsync(User user)
    {
        user.Id = MongoDB.Bson.ObjectId.GenerateNewId();
        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateUserAsync(User user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing != null)
        {
            existing.Email = user.Email;
            existing.Password = user.Password;
            existing.RefreshToken = user.RefreshToken;
        }
        return Task.CompletedTask;
    }
}

// Implementación fake de IJwtService
public class FakeJwtService : IJwtService
{
    public string GenerateToken(string email, string userId)
    {
        // Token falso, pero el FakeAuthHandler lo ignorará
        return "fake-jwt-token";
    }
}
