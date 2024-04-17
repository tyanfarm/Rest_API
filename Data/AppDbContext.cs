using Microsoft.EntityFrameworkCore;
using Rest_API.Models;

namespace E_Commerce.Data;

public class AppDbContext : DbContext {
    public DbSet<Team> Teams {get; set;}

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
        
    }
}