using ASOIU_3.Data;
using ASOIU_3.Models;
using ASOIU_3.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ASOIU_3.Tests;

public sealed class ApplicationTests
{
    [Fact]
    public void InitializerCreatesRequiredSeedDataAndIsIdempotent()
    {
        using var database = new TestDatabase();

        DatabaseInitializer.Initialize(database.CreateContext);
        DatabaseInitializer.Initialize(database.CreateContext);

        using var context = database.CreateContext();
        Assert.Equal(4, context.Restaurants.Count());
        Assert.Equal(12, context.MenuItems.Count());
    }

    [Fact]
    public void RestaurantCrudWorksAndLinkedRestaurantCannotBeDeleted()
    {
        using var database = new TestDatabase();
        var service = new RestaurantService(database.CreateContext);

        var addResult = service.Add("Тестовый ресторан");
        var restaurant = Assert.Single(service.GetAll());
        var updateResult = service.Update(restaurant.Id, "Новое название");
        var deleteResult = service.Delete(restaurant.Id);

        Assert.True(addResult.IsSuccess);
        Assert.True(updateResult.IsSuccess);
        Assert.True(deleteResult.IsSuccess);
        Assert.Empty(service.GetAll());

        DatabaseInitializer.Initialize(database.CreateContext);
        var linkedRestaurant = service.GetAll().First();
        var blockedResult = service.Delete(linkedRestaurant.Id);

        Assert.False(blockedResult.IsSuccess);
        Assert.Contains("связанные блюда", blockedResult.Message);
    }

    [Fact]
    public void MenuItemServiceRejectsNegativePrice()
    {
        using var database = new TestDatabase();
        DatabaseInitializer.Initialize(database.CreateContext);
        var restaurantId = new RestaurantService(database.CreateContext)
            .GetAll()
            .First()
            .Id;
        var service = new MenuItemService(database.CreateContext);

        var result = service.Add("Некорректное блюдо", -1, restaurantId);

        Assert.False(result.IsSuccess);
        Assert.Equal(12, service.GetAll().Count);
    }

    [Fact]
    public void ReportContainsSortedListCountsAndDescendingAverages()
    {
        using var database = new TestDatabase();
        using (var context = database.CreateContext())
        {
            var alpha = new Restaurant
            {
                Name = "Альфа",
                MenuItems =
                [
                    new MenuItem { Name = "Блюдо Б", Price = 100 },
                    new MenuItem { Name = "Блюдо А", Price = 300 },
                ],
            };
            var beta = new Restaurant
            {
                Name = "Бета",
                MenuItems =
                [
                    new MenuItem { Name = "Блюдо В", Price = 500 },
                ],
            };

            context.Restaurants.AddRange(alpha, beta);
            context.SaveChanges();
        }

        var report = new ReportService(database.CreateContext).Generate();

        Assert.Equal(
            ["Блюдо А", "Блюдо Б", "Блюдо В"],
            report.FullList.Select(row => row.MenuItemName));
        Assert.Equal([2, 1], report.Counts.Select(row => row.Count));
        Assert.Equal(
            ["Бета", "Альфа"],
            report.AveragePrices.Select(row => row.RestaurantName));
        Assert.Equal(500, report.AveragePrices[0].AveragePrice);
        Assert.Equal(200, report.AveragePrices[1].AveragePrice);
    }

    private sealed class TestDatabase : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<AppDbContext> _options;

        public TestDatabase()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = CreateContext();
            context.Database.EnsureCreated();
        }

        public AppDbContext CreateContext() => new(_options);

        public void Dispose() => _connection.Dispose();
    }
}
