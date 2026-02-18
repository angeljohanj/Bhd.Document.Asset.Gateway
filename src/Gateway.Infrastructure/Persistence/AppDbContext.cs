using Gateway.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gateway.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DocumentAsset> Documents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentAsset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Filename).IsRequired();
            entity.Property(e => e.ContentType).IsRequired();
            entity.Property(e => e.DocumentType).HasConversion<string>();
            entity.Property(e => e.Channel).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
        });
    }
}
