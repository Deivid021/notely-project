using Microsoft.EntityFrameworkCore;
using Notely.Api.Models;

namespace Notely.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<NoteGroup> NoteGroups => Set<NoteGroup>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.Property(u => u.PasswordHash).IsRequired();
        });

        builder.Entity<NoteGroup>(e =>
        {
            e.HasKey(g => g.Id);
            e.Property(g => g.Name).IsRequired().HasMaxLength(100);
            e.Property(g => g.Description).HasMaxLength(500);
            e.HasOne(g => g.User)
             .WithMany(u => u.NoteGroups)
             .HasForeignKey(g => g.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RefreshToken>(e =>
        {
            e.HasKey(rt => rt.Id);
            e.Property(rt => rt.TokenHash).IsRequired().HasMaxLength(64);
            e.HasIndex(rt => rt.TokenHash).IsUnique();
            e.HasOne(rt => rt.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(rt => rt.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Note>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Title).IsRequired().HasMaxLength(255);
            e.Property(n => n.Content).IsRequired();
            e.HasOne(n => n.User)
             .WithMany(u => u.Notes)
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(n => n.NoteGroup)
             .WithMany(g => g.Notes)
             .HasForeignKey(n => n.NoteGroupId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
