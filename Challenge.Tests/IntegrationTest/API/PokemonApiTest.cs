using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace Challenge.Tests.IntegrationTests.API;

public class PokemonApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PokemonApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPokemon_ValidId_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var pokemonId = 1;  // Bulbasaur

        // Act
        var response = await client.GetAsync($"/api/pokemon/{pokemonId}");

        // Assert
        response.EnsureSuccessStatusCode();  // Status 200-299
        Assert.Equal("application/json; charset=utf-8", 
            response.Content.Headers.ContentType?.ToString());
    }
}