using Rest_API.Models;

namespace Rest_API.Repositories;

public interface ITeamRepository {
    Task<List<Team>> GetAllTeams();
    Task<Team> GetById(int id);
    Task<int> Create(Team team);
    Task<bool> Update(int id, string? country, string? teamPrincipal);
    Task<bool> Delete(int id);
}