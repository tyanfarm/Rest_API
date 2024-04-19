// using E_Commerce.Data;
// using Microsoft.EntityFrameworkCore;
// using Rest_API.Models;

// namespace E_Commerce.Repositories;

// public class TeamRepository : ITeamRepository {
//     private readonly AppDbContext _context;

//     public TeamRepository(AppDbContext context) {
//         _context = context;
//     }

//     public async Task<int> Create(Team team)
//     {
//         await _context.Teams.AddAsync(team);
//         await _context.SaveChangesAsync();

//         return team.Id;
//     }

//     public async Task<bool> Delete(int id)
//     {
//         var team = _context.Teams.FirstOrDefault(x => x.Id == id);
        
//         if (team == null) {
//             return false;
//         }

//         _context.Teams.Remove(team);
//         await _context.SaveChangesAsync();

//         return true;
//     }

//     public async Task<List<Team>> GetAllTeams()
//     {
//         return await _context.Teams.ToListAsync();
//     }

//     public async Task<Team> GetById(int id)
//     {
//         return await _context.Teams.FirstOrDefaultAsync(x => x.Id == id);
//     }

//     public async Task<bool> UpdateCountry(int id, string country)
//     {
//         var team = _context.Teams.FirstOrDefault(x => x.Id == id);

//         if (team == null) {
//             return false;
//         }

//         team.Country = country;
    
//         return true;
//     }
// }