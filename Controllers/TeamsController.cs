using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rest_API.Models;
using Rest_API.Repositories;

namespace Rest_API.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
    public async Task<IActionResult> Update(int id, string? country, string? teamPrincipal) {
        try {
            var result = await _teamRepository.Update(id, country, teamPrincipal);

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
    public async Task<IActionResult> Delete(int id) {
        try {
            var result = await _teamRepository.Delete(id);

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