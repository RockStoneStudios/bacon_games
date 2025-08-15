using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Challenge.Models.Entities.Pokemon;
using Challenge.Repository.Pokemon;
using Moq;
using Moq.Protected;
using Xunit;

public class PokemonRepositoryTests
{
    [Fact]
    public async Task GetByIdAsync_ReturnsPokemon_WhenExists()
    {
        // Arrange
        var fakePokemon = new Pokemon
        {
            Id = 1,
            Name = "bulbasaur",
            Height = 7,
            Weight = 69,
            Types = new List<PokemonTypeSlot>
            {
                new PokemonTypeSlot
                {
                    Slot = 1,
                    Type = new PokemonType { Name = "grass", Url = "https://pokeapi.co/api/v2/type/12/" }
                }
            }
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(fakePokemon)
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://pokeapi.co/api/v2/")
        };

        var repository = new PokemonRepository(httpClient);

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("bulbasaur", result.Name);
        Assert.Single(result.Types);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://pokeapi.co/api/v2/")
        };

        var repository = new PokemonRepository(httpClient);

        // Act
        var result = await repository.GetByIdAsync(999999);

        // Assert
        Assert.Null(result);
    }
}
