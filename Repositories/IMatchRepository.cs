using Rest_API.Models;
using Rest_API.Models.DTO;

namespace Rest_API.Repositories;

public interface IMatchRepository {
    Task<List<Match>> GetAllMatches();
    Task<Match> GetById(int id);
    Task<int> Create(MatchDTO match);
    Task<bool> Update(int id, int? AteamId, int? BteamId, string? stadium, string? score);
    Task<bool> Delete(int id);
}