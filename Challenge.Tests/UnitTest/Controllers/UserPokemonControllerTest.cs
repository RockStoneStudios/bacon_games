using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using Challenge.Controllers;
using Challenge.Repository.UserPokemon;
using Challenge.Repository.Pokemon;
using Challenge.Models.Entities.Pokemon;
using System;

public class UserPokemonControllerTests
{
    private readonly Mock<IUserPokemonRepository> _userPokemonRepoMock;
    private readonly Mock<IPokemonRepository> _pokemonRepoMock;
    private readonly UserPokemonController _controller;

    public UserPokemonControllerTests()
    {
        _userPokemonRepoMock = new Mock<IUserPokemonRepository>();
        _pokemonRepoMock = new Mock<IPokemonRepository>();
        _controller = new UserPokemonController(_userPokemonRepoMock.Object, _pokemonRepoMock.Object);
    }

    private void SetUser(string? userId, bool authenticated)
    {
        var claims = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(userId))
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));

        var identity = authenticated ? new ClaimsIdentity(claims, "mock") : new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task AddPokemon_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        SetUser(null, false);

        var result = await _controller.AddPokemon(1);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Contains("Authentication required", unauthorized.Value!.ToString());
    }

    [Fact]
    public async Task AddPokemon_ShouldReturnUnauthorized_WhenUserIdIsMissing()
    {
        SetUser("", true);

        var result = await _controller.AddPokemon(1);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Contains("Invalid user identifier", unauthorized.Value!.ToString());
    }

    [Fact]
    public async Task AddPokemon_ShouldReturnBadRequest_WhenPokemonNotFound()
    {
        SetUser("user123", true);
        _pokemonRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Pokemon)null!);

        var result = await _controller.AddPokemon(1);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("does not exist", badRequest.Value!.ToString());
    }

    [Fact]
    public async Task AddPokemon_ShouldReturnOk_WhenPokemonExists()
    {
        SetUser("user123", true);
        var pokemon = new Pokemon { Id = 1, Name = "Pikachu", Height = 4, Weight = 60, Types = new List<PokemonTypeSlot>() };
        _pokemonRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pokemon);

        var result = await _controller.AddPokemon(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("Pokémon Pikachu added to inventory", okResult.Value!.ToString());
        _userPokemonRepoMock.Verify(r => r.AddPokemonAsync("user123", 1), Times.Once);
    }

    [Fact]
    public async Task AddPokemon_ShouldReturn500_WhenRepositoryThrowsException()
    {
        SetUser("user123", true);
        var pokemon = new Pokemon { Id = 1, Name = "Pikachu", Height = 4, Weight = 60, Types = new List<PokemonTypeSlot>() };
        _pokemonRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(pokemon);
        _userPokemonRepoMock.Setup(r => r.AddPokemonAsync("user123", 1))
            .ThrowsAsync(new Exception("DB error"));

        var result = await _controller.AddPokemon(1);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Contains("Failed to add Pokémon", statusResult.Value!.ToString());
    }

    [Fact]
    public async Task GetInventory_ShouldReturnUnauthorized_WhenUserNotAuthenticated()
    {
        SetUser(null, false);

        var result = await _controller.GetInventory();

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Contains("Authentication required", unauthorized.Value!.ToString());
    }

    [Fact]
    public async Task GetInventory_ShouldReturnUnauthorized_WhenUserIdIsMissing()
    {
        SetUser("", true);

        var result = await _controller.GetInventory();

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Contains("Invalid user identifier", unauthorized.Value!.ToString());
    }

    [Fact]
    public async Task GetInventory_ShouldReturnOk_WithPokemonList()
    {
        SetUser("user123", true);
        _userPokemonRepoMock.Setup(r => r.GetUserPokemonsAsync("user123"))
            .ReturnsAsync(new List<int> { 1, 2 });

        _pokemonRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Pokemon { Id = 1, Name = "Pikachu", Height = 4, Weight = 60, Types = new List<PokemonTypeSlot>() });
        _pokemonRepoMock.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new Pokemon { Id = 2, Name = "Charmander", Height = 6, Weight = 85, Types = new List<PokemonTypeSlot>() });

        var result = await _controller.GetInventory();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var inventory = Assert.IsType<List<Pokemon>>(okResult.Value);
        Assert.Equal(2, inventory.Count);
    }

    [Fact]
    public async Task GetInventory_ShouldReturn500_WhenRepositoryThrowsException()
    {
        SetUser("user123", true);
        _userPokemonRepoMock.Setup(r => r.GetUserPokemonsAsync("user123"))
            .ThrowsAsync(new Exception("DB error"));

        var result = await _controller.GetInventory();

        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Contains("Failed to retrieve inventory", statusResult.Value!.ToString());
    }
}
