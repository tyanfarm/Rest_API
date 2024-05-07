using Rest_API.Models;

namespace Rest_API.Repositories;

public interface IPlayerRepository {
    Task<List<Player>> GetAllPlayers();
    Task<Player> GetById(int id);
    Task<int> Create(Player player);
    Task<bool> Update(int id, string? name, string? position);
    Task<bool> Delete(int id);
}