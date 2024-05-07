using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rest_API.Data;
using Rest_API.Models;
using Rest_API.Repositories;
using Rest_API.Services;

namespace Rest_API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PlayersController : ControllerBase 
{
    private readonly IPlayerRepository _playerRepository;
    public PlayersController(IPlayerRepository playerRepository) {
        _playerRepository = playerRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Get() {
        try {
            var players = await _playerRepository.GetAllPlayers();

            if (players == null) {
                return NotFound();
            }

            return Ok(players);
        }
        catch {
            return StatusCode(500, "ERROR !");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id) {
        try {
            var player = await _playerRepository.GetById(id);
            
            if (player == null) {
                return NotFound();
            }

            return Ok(player);
        }
        catch {
            return StatusCode(500, "ERROR !");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Player player) {
        try {
            var status = await _playerRepository.Create(player);

            if (status > 0) {
                return CreatedAtAction("GetById", new {id = player.Id}, player);
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
    public async Task<IActionResult> Update(int id, string? name, string? position) {
        try {
            var result = await _playerRepository.Update(id, name, position);

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
            var result = await _playerRepository.Delete(id);

            if (result == true) {
                return Ok("Delete successfully");
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