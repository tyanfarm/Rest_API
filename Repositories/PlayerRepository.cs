using Microsoft.EntityFrameworkCore;
using Rest_API.Data;
using Rest_API.Models;
using Rest_API.Services;

namespace Rest_API.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly RestapiContext _context;
    private readonly ICacheService _cacheService;

    public PlayerRepository(RestapiContext context, ICacheService cacheService) {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<int> Create(Player player)
    {
        // EntityEntry<Player>
        var playerAdded = await _context.Players.AddAsync(player);

        // Set Data in Cache
        var expiryTime = DateTimeOffset.Now.AddSeconds(30);
        _cacheService.SetData<Player>($"player{player.Id}", playerAdded.Entity, expiryTime);

        await _context.SaveChangesAsync();

        return player.Id;
    }

    public async Task<bool> Delete(int id)
    {
        var existed = await _context.Players.FirstOrDefaultAsync(p => p.Id == id);

        if (existed != null) {
            _context.Players.Remove(existed);
            _cacheService.RemoveData($"player{id}");

            await _context.SaveChangesAsync();    

            return true;        
        }

        return false;
    }

    public async Task<List<Player>> GetAllPlayers()
    {
        var cachePlayers = _cacheService.GetData<IEnumerable<Player>>("players");

        if (cachePlayers != null && cachePlayers.Count() > 0) {
            return cachePlayers.ToList();
        }

        var players = await _context.Players.AsNoTracking()
                                            .Include(p => p.Team)
                                            .OrderBy(p => p.Id)
                                            .ToListAsync();

        // Set Data in Cache
        var expiryTime = DateTimeOffset.Now.AddSeconds(30);
        _cacheService.SetData<IEnumerable<Player>>("players", players, expiryTime);

        return players;
    }

    public async Task<Player> GetById(int id)
    {
        var cachePlayer = _cacheService.GetData<Player>($"player{id}");

        if (cachePlayer != null) {
            return cachePlayer;
        }

        // Caching not available
        var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == id);

        // Set Data in Cache
        var expiryTime = DateTimeOffset.Now.AddSeconds(30);
        _cacheService.SetData<Player>($"player{id}", player, expiryTime);

        return player;
    }

    public async Task<bool> Update(int id, string? name, string? position)
    {
        var player = _context.Players.FirstOrDefault(p => p.Id == id);

        if (player == null) {
            return false;
        }
        
        if (name != null) {
            player.Name = name;
        }

        if (position != null) {
            player.Position = position;
        }

        var playerUpdated = _context.Players.Update(player);

        // Set Data in Cache
        var expiryTime = DateTimeOffset.Now.AddSeconds(30);
        _cacheService.SetData<Player>($"player{id}", playerUpdated.Entity, expiryTime);

        await _context.SaveChangesAsync();  
        
        return true;
    }
}