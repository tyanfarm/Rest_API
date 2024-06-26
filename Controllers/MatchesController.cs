using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rest_API.Models;
using Rest_API.Models.DTO;
using Rest_API.Repositories;

namespace Rest_API.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/v1/[controller]")]
[ApiController]
public class MatchesController : ControllerBase {
    private readonly IMatchRepository _matchRepository;

    public MatchesController(IMatchRepository matchRepository) {
        _matchRepository = matchRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get() {
        try {
            var matches = await _matchRepository.GetAllMatches();

            if (matches == null) {
                return NotFound();
            }

            return Ok(matches);
        }
        catch {
            return StatusCode(500, "ERROR !");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id) {
        try {
            var team = await _matchRepository.GetById(id);

            if (team == null) {
                return NotFound();
            }

            return Ok(team);
        }
        catch {
            return StatusCode(500, "ERROR !");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post(MatchDTO matchDTO) {
        try {
            var match = await _matchRepository.Create(matchDTO);

            if (match != null) {
                return CreatedAtAction("GetById", new {id = match.Id}, match);
            }
            else {
                return NotFound();
            }
        }
        catch {
            return StatusCode(500, "ERROR !");
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(Guid id, int? AteamId, int? BteamId, string? stadium, string? score) {
        try {
            var result = await _matchRepository.Update(id, AteamId, BteamId, stadium, score);

            if (result == true) {
                return Ok("Update successfully");
            }
            else {
                return BadRequest();
            }
        }
        catch {
            return StatusCode(500, "ERROR !");
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Delete(Guid id) {
        try {
            var result = await _matchRepository.Delete(id);

            if (result == true) {
                return Ok("Delete Successfully");
            }
            else {
                return BadRequest();
            }
        }
        catch {
            return StatusCode(500, "ERROR !");
        }
    }
}