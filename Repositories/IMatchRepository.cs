using Rest_API.Models;
using Rest_API.Models.DTO;

namespace Rest_API.Repositories;

public interface IMatchRepository {
    Task<List<Match>> GetAllMatches();
    Task<Match> GetById(Guid id);
    Task<Match> Create(MatchDTO match);
    Task<bool> Update(Guid id, int? AteamId, int? BteamId, string? stadium, string? score);
    Task<bool> Delete(Guid id);
}