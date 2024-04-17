using E_Commerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rest_API.Models;

namespace Rest_API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class TeamsController : ControllerBase {
    private readonly AppDbContext _context;

    public TeamsController(AppDbContext context) {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get() {
        var teams = await _context.Teams.ToListAsync();

        return Ok(teams);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id) {
        var team = await _context.Teams.FirstOrDefaultAsync(x => x.Id == id);

        if (team == null) {
            return BadRequest("Invalid ID");
        }

        return Ok(team);
    }

    [HttpPost]
    public async Task<IActionResult> Post(Team team) {
        await _context.Teams.AddAsync(team);
        await _context.SaveChangesAsync();

        return CreatedAtAction("Get", team.Id, team);
    }

    [HttpPatch]
    public async Task<IActionResult> Patch(int id, string country) {
        var team = _context.Teams.FirstOrDefault(x => x.Id == id);

        if (team == null) {
            return BadRequest("INVALID ID");
        }

        team.Country = country;
    
        return Ok("update successfully");
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(int id) {
        var team = _context.Teams.FirstOrDefault(x => x.Id == id);
        
        if (team == null) {
            return BadRequest("INVALID ID");
        }

        _context.Teams.Remove(team);
        await _context.SaveChangesAsync();

        return Ok("Delete successfully");
    }
}