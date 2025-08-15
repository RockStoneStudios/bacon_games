using AutoMapper;
using Challenge.Models.DTOs.Pokemon;
using Challenge.Models.Entities.Pokemon;



namespace Challenge.Models.Mappings
{
    public class PokemonProfile : Profile
    {
        public PokemonProfile()
        {
            // Mapeo de Pokemon (Entity) a PokemonResponse (DTO)
            CreateMap<Pokemon, PokemonResponse>()
            .ForMember(dest => dest.Types, opt => opt.MapFrom(src => src.Types));

            // Mapeo de PokemonType (Entity) a PokemonTypeResponse (DTO)
            CreateMap<PokemonType, PokemonTypeResponse>();
        }
    }
}