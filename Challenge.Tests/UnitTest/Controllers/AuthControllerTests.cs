using Challenge.Controllers;
using Challenge.Models.DTOs.Auth;
using Challenge.Models.Entities.User;
using Challenge.Repository.Auth;
using Challenge.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MongoDB.Bson;
using Xunit;
using BC = BCrypt.Net.BCrypt;

namespace Challenge.Tests.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthRepository> _mockAuthRepo;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthRepo = new Mock<IAuthRepository>();
        _mockJwtService = new Mock<IJwtService>();
        
        // Asegúrate de pasar los objetos mockeados (.Object)
        _controller = new AuthController(
            _mockAuthRepo.Object,
            _mockJwtService.Object
        );
    }

   [Fact]
public async Task Login_ValidCredentials_ReturnsToken()
{
    // Arrange
    var request = new LoginRequest 
    { 
        Email = "test@example.com", 
        Password = "Test123!" 
    };
    
    var user = new User 
    { 
        Id = ObjectId.GenerateNewId(),
        Email = request.Email, 
        Password = BC.HashPassword(request.Password),
        RefreshToken = null
    };
    
    _mockAuthRepo.Setup(x => x.GetUserByEmailAsync(request.Email))
               .ReturnsAsync(user);
               
    _mockJwtService.Setup(x => x.GenerateToken(
        It.IsAny<string>(), 
        It.IsAny<string>()))
    .Returns("fake-jwt-token");

    // Act
    var result = await _controller.Login(request);

    // Assert - Cambios aquí:
    var actionResult = Assert.IsType<ActionResult<LoginResponse>>(result);
    var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
    Assert.Equal("fake-jwt-token", (okResult.Value as LoginResponse)?.Token);
}
}