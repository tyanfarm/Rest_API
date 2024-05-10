using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rest_API.Models;

namespace Rest_API.Data;

public partial class RestapiContext : IdentityDbContext
{
    public RestapiContext()
    {
    }

    public RestapiContext(DbContextOptions<RestapiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Match> Matches { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<Refreshtoken> Refreshtokens { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=127.0.0.1;uid=root;pwd=tyan;database=restapi;port=3306", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.36-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("matches");

            entity.HasIndex(e => e.AteamId, "ATeamId_idx");

            entity.HasIndex(e => e.BteamId, "BTeamId_idx");

            entity.Property(e => e.AteamId).HasColumnName("ATeamId");
            entity.Property(e => e.BteamId).HasColumnName("BTeamId");
            entity.Property(e => e.Schedule).HasMaxLength(6);
            entity.Property(e => e.Score)
                .HasMaxLength(10)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Stadium)
                .HasMaxLength(20)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.Ateam).WithMany(p => p.MatchAteams)
                .HasForeignKey(d => d.AteamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("ATeamId");

            entity.HasOne(d => d.Bteam).WithMany(p => p.MatchBteams)
                .HasForeignKey(d => d.BteamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BTeamId");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("players");

            entity.HasIndex(e => e.TeamId, "Id_idx");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Position)
                .HasMaxLength(20)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");

            entity.HasOne(d => d.Team).WithMany(p => p.Players)
                .HasForeignKey(d => d.TeamId)
                .HasConstraintName("TeamId");
        });

        modelBuilder.Entity<Refreshtoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("refreshtokens");

            entity.Property(e => e.AddedDate).HasMaxLength(6);
            entity.Property(e => e.ExpiryDate).HasMaxLength(6);
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("teams");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
