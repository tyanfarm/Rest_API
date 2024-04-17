using E_Commerce.Data;
using E_Commerce.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rest_API.Models;

namespace Rest_API.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
public class TeamsController : ControllerBase {
    private readonly ITeamRepository _teamRepository;

    public TeamsController(ITeamRepository teamRepository) {
        _teamRepository = teamRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get() {
        try {
            var teams = await _teamRepository.GetAllTeams();

            if (teams == null) {
                return NotFound();
            }

            return Ok(teams);
        }
        catch {
            return StatusCode(500, "ERROR !");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) {
        try {
            var team = await _teamRepository.GetById(id);

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
    public async Task<IActionResult> Post([FromBody] Team team) {
        try {
            var status = await _teamRepository.Create(team);

            if (status > 0) {
                return CreatedAtAction("GetById", new {id = team.Id}, team);
            }
            else {
                return NotFound();
            }
        }
        catch {
            return StatusCode(500, "ERROR !");
        }
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateCountry(int id, string country) {
        try {
            var result = await _teamRepository.UpdateCountry(id, country);

            if (result == true) {
                return Ok();
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
    public async Task<IActionResult> Delete(int id) {
        try {
            var result = await _teamRepository.Delete(id);

            if (result == true) {
                return Ok();
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