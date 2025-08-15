using Challenge.Controllers;
using Challenge.Models.Entities.Pokemon;
using Challenge.Repository.Pokemon;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Challenge.Tests.UnitTests.Controllers;

public class PokemonControllerTests
{
    private readonly Mock<IPokemonRepository> _mockRepo;
    private readonly PokemonController _controller;

    public PokemonControllerTests()
    {
        _mockRepo = new Mock<IPokemonRepository>();
        _controller = new PokemonController(_mockRepo.Object);
    }

    private Pokemon CreateTestPokemon(int id, string name, params (int slot, string typeName)[] types)
    {
        var pokemon = new Pokemon
        {
            Id = id,
            Name = name,
            Height = 70, // Valor por defecto
            Weight = 70, // Valor por defecto
            Types = new List<PokemonTypeSlot>()
        };

        foreach (var type in types)
        {
            pokemon.Types.Add(new PokemonTypeSlot
            {
                Slot = type.slot,
                Type = new PokemonType { Name = type.typeName, Url = $"https://pokeapi.co/api/v2/type/{type.slot}/" }
            });
        }

        return pokemon;
    }

    [Fact]
    public async Task GetPokemonById_WithValidId_ReturnsPokemon()
    {
        // Arrange
        var expectedPokemon = CreateTestPokemon(
            id: 1,
            name: "Bulbasaur",
            (1, "Grass"),
            (2, "Poison")
        );

        _mockRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(expectedPokemon);

        // Act
        var result = await _controller.GetPokemonById(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Pokemon>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPokemon = Assert.IsType<Pokemon>(okResult.Value);
        
        Assert.Equal("Bulbasaur", returnedPokemon.Name);
        Assert.Equal(2, returnedPokemon.Types.Count);
        Assert.Equal("Grass", returnedPokemon.Types[0].Type.Name);
    }

    [Fact]
    public async Task GetPokemonByName_WithValidName_ReturnsPokemon()
    {
        // Arrange
        var expectedPokemon = CreateTestPokemon(
            id: 25,
            name: "Pikachu",
            (1, "Electric")
        );

        _mockRepo.Setup(x => x.GetByNameAsync("Pikachu")).ReturnsAsync(expectedPokemon);

        // Act
        var result = await _controller.GetPokemonByName("Pikachu");

        // Assert
        var actionResult = Assert.IsType<ActionResult<Pokemon>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPokemon = Assert.IsType<Pokemon>(okResult.Value);
        
        Assert.Equal("Electric", returnedPokemon.Types[0].Type.Name);
    }

    [Theory]
    [InlineData(1, "Bulbasaur", new[] { "Grass", "Poison" })]
    [InlineData(25, "Pikachu", new[] { "Electric" })]
    [InlineData(6, "Charizard", new[] { "Fire", "Flying" })]
    public async Task GetPokemonById_WithVariousIds_ReturnsCorrectPokemon(int id, string name, string[] types)
    {
        // Arrange
        var typeSlots = types.Select((t, i) => (i + 1, t)).ToArray();
        var expectedPokemon = CreateTestPokemon(id, name, typeSlots);
        
        _mockRepo.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(expectedPokemon);

        // Act
        var result = await _controller.GetPokemonById(id);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Pokemon>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var returnedPokemon = Assert.IsType<Pokemon>(okResult.Value);
        
        Assert.Equal(name, returnedPokemon.Name);
        Assert.Equal(types.Length, returnedPokemon.Types.Count);
        for (int i = 0; i < types.Length; i++)
        {
            Assert.Equal(types[i], returnedPokemon.Types[i].Type.Name);
        }
    }
}