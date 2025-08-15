using Challenge.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Challenge.Tests.UnitTests.Services;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly Mock<IConfiguration> _mockConfig;

    public JwtServiceTests()
    {
        // 1. Configura el mock de IConfiguration
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(x => x["Jwt:SecretKey"]).Returns("una-clave-secreta-muy-larga-1234567890-32");
        _mockConfig.Setup(x => x["Jwt:Issuer"]).Returns("test-issuer");

        // 2. Instancia el servicio con el mock
        _jwtService = new JwtService(_mockConfig.Object);
    }

    [Fact]
    public void GenerateToken_ShouldReturnValidToken()
    {
        // Arrange
        var userId = "123";
        var username = "test-user";

        // Act
        var token = _jwtService.GenerateToken(userId, username);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }
}