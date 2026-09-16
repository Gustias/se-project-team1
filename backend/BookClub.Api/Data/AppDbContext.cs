using BookClub.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace BookClub.Api.Data;

public class AppDbContext : DbContext
{
    protected readonly IConfiguration Configuration;
    
    public AppDbContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(Configuration.GetConnectionString("ConnectionString"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Checks to prevent duplicates.
        modelBuilder.Entity<ReadingProgress>()
            .HasIndex(rp => new
            {
                rp.UserId,
                rp.BookId,
            })
            .IsUnique();

        modelBuilder.Entity<ReadingLog>()
            .HasIndex(rl => new
            {
                rl.UserId,
                rl.BookId,
                rl.Date
            })
            .IsUnique();
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ReadingProgress> ReadingProgresses => Set<ReadingProgress>();
    public DbSet<ReadingLog> ReadingLogs => Set<ReadingLog>();
}