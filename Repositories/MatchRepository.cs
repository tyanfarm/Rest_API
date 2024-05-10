// using Microsoft.EntityFrameworkCore;
// using Rest_API.Data;
// using Rest_API.Models;
// using Rest_API.Models.DTO;

// namespace Rest_API.Repositories;

// public class MatchRepository : IMatchRepository {
//     private readonly RestapiContext _context;

//     public MatchRepository(RestapiContext context) {
//         _context = context;
//     }

//     public async Task<bool> Create(MatchDTO match)
//     {
//         var _match = new Match() {
//             AteamId = match.AteamId,
//             BteamId = match.BteamId,
//             Schedule = DateTime.Now.AddDays(7),
//             Stadium = match.Stadium,
//             Score = ""
//         };

//         await _context.Matches.AddAsync(_match);
//         await _context.SaveChangesAsync();

//         return true;
//     }

//     public async Task<bool> Delete(int id)
//     {
//         var match = _context.Matches.FirstOrDefault(x => x.Id == id);
        
//         if (match == null) {
//             return false;
//         }

//         _context.Matches.Remove(match);
//         await _context.SaveChangesAsync();

//         return true;
//     }

//     public async Task<List<Match>> GetAllMatches()
//     {
//         return await _context.Matches.ToListAsync();
//     }

//     public async Task<Match> GetById(int id)
//     {
//         return await _context.Matches.FirstOrDefaultAsync(x => x.Id == id);
//     }

//     public async Task<bool> Update(int id, string? country, string? teamPrincipal)
//     {
//         var team = _context.Teams.FirstOrDefault(x => x.Id == id);

//         if (team == null) {
//             return false;
//         }
        
//         if (country != null) {
//             team.Country = country;
//         }

//         if (teamPrincipal != null) {
//             team.TeamPrincipal = teamPrincipal;
//         }

//         _context.Update(team);
//         await _context.SaveChangesAsync();  
        
//         return true;
//     }

//     public async Task<bool> Update(int id, int? AteamId, int? BteamId, string? stadium, string? score)
//     {
//         var match = await _context.Matches.FirstOrDefaultAsync(x => x.Id == id);

//         if (match == null) {
//             return false;
//         }
        
//         if (AteamId != null) {
//             match.AteamId = AteamId.Value;
//         }

//         if (BteamId != null) {
//             match.BteamId = BteamId.Value;
//         }

//         if (stadium != null) {
//             match.Stadium = stadium;
//         }

//         if (score != null) {
//             match.Score = score;
//         }

//         _context.Update(match);
//         await _context.SaveChangesAsync();  
        
//         return true;
//     }

// } 