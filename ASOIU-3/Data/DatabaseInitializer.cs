using ASOIU_3.Models;

namespace ASOIU_3.Data;

/// <summary>
/// Создаёт базу данных и добавляет демонстрационные данные.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Создаёт базу данных при первом запуске и заполняет пустую базу.
    /// </summary>
    public static void Initialize()
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();

        if (context.Restaurants.Any())
        {
            return;
        }

        var restaurants = CreateRestaurants();
        context.Restaurants.AddRange(restaurants);
        context.SaveChanges();
    }

    private static Restaurant[] CreateRestaurants()
    {
        return
        [
            new Restaurant
            {
                Name = "Север",
                MenuItems =
                [
                    new MenuItem { Name = "Уха по-северному", Price = 590 },
                    new MenuItem { Name = "Пельмени с олениной", Price = 720 },
                    new MenuItem { Name = "Морошковый десерт", Price = 390 },
                ],
            },
            new Restaurant
            {
                Name = "Итальянский дворик",
                MenuItems =
                [
                    new MenuItem { Name = "Маргарита", Price = 650 },
                    new MenuItem { Name = "Лазанья болоньезе", Price = 790 },
                    new MenuItem { Name = "Тирамису", Price = 450 },
                ],
            },
            new Restaurant
            {
                Name = "Восточный базар",
                MenuItems =
                [
                    new MenuItem { Name = "Плов с бараниной", Price = 680 },
                    new MenuItem { Name = "Манты", Price = 560 },
                    new MenuItem { Name = "Пахлава", Price = 320 },
                ],
            },
            new Restaurant
            {
                Name = "Зелёная кухня",
                MenuItems =
                [
                    new MenuItem { Name = "Боул с киноа", Price = 610 },
                    new MenuItem { Name = "Крем-суп из брокколи", Price = 430 },
                    new MenuItem { Name = "Яблочный крамбл", Price = 370 },
                ],
            },
        ];
    }
}
