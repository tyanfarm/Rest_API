using AutoMapper;
using Rest_API.Models;
using Rest_API.Models.DTO;

namespace Rest_API.Profiles;

public class MatchProfile : Profile {
    public MatchProfile() {
        CreateMap<MatchDTO, Match>()
            // mapping giữa nguồn (src) và đích (dest)
            .ForMember(
                dest => dest.Id,        // Match ID
                opt => opt.MapFrom(src => Guid.NewGuid())
            )
            .ForMember(
                dest => dest.AteamId,
                opt => opt.MapFrom(src => src.AteamId)
            )
            .ForMember(
                dest => dest.BteamId,
                opt => opt.MapFrom(src => src.BteamId)
            )
            .ForMember(
                dest => dest.Stadium,
                opt => opt.MapFrom(src => src.Stadium)
            )
            .ForMember(
                dest => dest.Schedule,
                opt => opt.MapFrom(src => DateTime.Now.AddDays(7))
            )
            .ForMember(
                dest => dest.Score,
                opt => opt.MapFrom(src => "")
            );
    }
}