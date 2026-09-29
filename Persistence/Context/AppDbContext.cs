using Application.Common.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Paper> Papers => Set<Paper>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(200);
            entity.HasMany(b => b.Papers)
                  .WithOne(p => p.Book)
                  .HasForeignKey(p => p.BookId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Paper>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Title).IsRequired().HasMaxLength(300);
        });
    }
}