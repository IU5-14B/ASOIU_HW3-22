using ASOIU_3.Models;
using Microsoft.EntityFrameworkCore;

namespace ASOIU_3.Data;

/// <summary>
/// Контекст Entity Framework Core для базы данных ресторанов.
/// </summary>
public sealed class AppDbContext : DbContext
{
    /// <summary>
    /// Инициализирует контекст с настройками подключения по умолчанию.
    /// </summary>
    public AppDbContext()
    {
    }

    /// <summary>
    /// Инициализирует контекст с переданными настройками.
    /// </summary>
    /// <param name="options">Настройки контекста базы данных.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Получает набор ресторанов.
    /// </summary>
    public DbSet<Restaurant> Restaurants => Set<Restaurant>();

    /// <summary>
    /// Получает набор блюд меню.
    /// </summary>
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();

    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=app.db");
        }
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.Property(restaurant => restaurant.Name)
                .HasMaxLength(120)
                .IsRequired();

            entity.HasIndex(restaurant => restaurant.Name)
                .IsUnique();
        });

        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.Property(menuItem => menuItem.Name)
                .HasMaxLength(120)
                .IsRequired();

            entity.HasOne(menuItem => menuItem.Restaurant)
                .WithMany(restaurant => restaurant.MenuItems)
                .HasForeignKey(menuItem => menuItem.RestaurantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
