// ApplicationDbContext.cs
using Microsoft.EntityFrameworkCore;
using MovieLibraryMonitor.Models;

namespace MovieLibraryMonitor.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<FileSystemEvent> FileSystemEvents { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileSystemEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.Property(e => e.FilePath)
                .IsRequired()
                .HasMaxLength(1000);
                
            entity.Property(e => e.OldFilePath)
                .HasMaxLength(1000);
                
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
