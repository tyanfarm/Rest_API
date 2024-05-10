using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Rest_API.Data;
using Rest_API.Models;
using Rest_API.Models.DTO;

namespace Rest_API.Repositories;

public class MatchRepository : IMatchRepository {
    private readonly RestapiContext _context;
    private readonly IMapper _mapper;

    public MatchRepository(RestapiContext context, IMapper mapper) {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Match> Create(MatchDTO match)
    {
        // Mapping MatchDTO -> Match
        var _match = _mapper.Map<Match>(match);

        await _context.Matches.AddAsync(_match);
        await _context.SaveChangesAsync();

        return _match;
    }

    public async Task<bool> Delete(Guid id)
    {
        var match = _context.Matches.FirstOrDefault(x => x.Id == id);
        
        if (match == null) {
            return false;
        }

        _context.Matches.Remove(match);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<Match>> GetAllMatches()
    {
        return await _context.Matches.ToListAsync();
    }

    public async Task<Match> GetById(Guid id)
    {
        return await _context.Matches.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> Update(Guid id, int? AteamId, int? BteamId, string? stadium, string? score)
    {
        var match = await _context.Matches.FirstOrDefaultAsync(x => x.Id == id);

        if (match == null) {
            return false;
        }
        
        if (AteamId != null) {
            match.AteamId = AteamId.Value;
        }

        if (BteamId != null) {
            match.BteamId = BteamId.Value;
        }

        if (stadium != null) {
            match.Stadium = stadium;
        }

        if (score != null) {
            match.Score = score;
        }

        _context.Update(match);
        await _context.SaveChangesAsync();  
        
        return true;
    }

} 