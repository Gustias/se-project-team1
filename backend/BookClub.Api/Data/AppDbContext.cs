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
            });
        modelBuilder.Entity<ReadingLog>()
            .ToTable(t => t.HasCheckConstraint(
                "CK_ReadingLogs_Pages",
                "\"EndPage\" >= \"StartPage\""));

        modelBuilder.Entity<Book>()
            .HasIndex(b => b.ExternalId)
            .IsUnique();

        modelBuilder.Entity<BookRatingEntry>(entity =>
        {
            entity.Property(r => r.Rating)
                .HasConversion(
                    rating => rating.Value,
                    value => new BookRating(value));

            entity.HasIndex(r => new
            {
                r.UserId,
                r.BookId
            })
            .IsUnique();

            entity.ToTable(table => table.HasCheckConstraint(
                "CK_BookRatingEntries_Rating",
                "\"Rating\" >= 1 AND \"Rating\" <= 5"));
        });

        modelBuilder.Entity<ReadingProgress>()
            .Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
    }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ReadingProgress> ReadingProgresses => Set<ReadingProgress>();
    public DbSet<ReadingLog> ReadingLogs => Set<ReadingLog>();
    public DbSet<BookRatingEntry> BookRatingEntries => Set<BookRatingEntry>();
}
