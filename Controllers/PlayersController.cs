using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rest_API.Data;
using Rest_API.Models;
using Rest_API.Services;

namespace Rest_API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PlayersController : ControllerBase {
    private readonly RestapiContext _context;
    private readonly ICacheService _cacheService;

    public PlayersController(RestapiContext context, ICacheService cacheService) {
        _context = context;
        _cacheService = cacheService;
    }

    [HttpGet]
    public async Task<IActionResult> Get() {
        var cachePlayers = _cacheService.GetData<IEnumerable<Player>>("players");

        if (cachePlayers != null && cachePlayers.Count() > 0) {
            return Ok(cachePlayers);
        }

        var players = await _context.Players.AsNoTracking()
                                            .Include(p => p.Team)
                                            .OrderBy(p => p.Id)
                                            .ToListAsync();

        var expiryTime = DateTimeOffset.Now.AddMinutes(2);
        _cacheService.SetData<IEnumerable<Player>>("players", players, expiryTime);

        return Ok(players);
    }

    [HttpPost]
    public async Task<IActionResult> Post(Player player) {
        await _context.Players.AddAsync(player);
        await _context.SaveChangesAsync();

        return Ok(player);
    }


}