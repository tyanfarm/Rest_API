using Rest_API.Models;

namespace E_Commerce.Repositories;

public interface ITeamRepository {
    Task<List<Team>> GetAllTeams();
    Task<Team> GetById(int id);
    Task<int> Create(Team team);
    Task<bool> UpdateCountry(int id, string country);
    Task<bool> Delete(int id);
}