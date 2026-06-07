using ASOIU_3.Data;
using ASOIU_3.Models;
using Microsoft.EntityFrameworkCore;

namespace ASOIU_3.Services;

internal sealed class MenuItemService
{
    private const int MaximumNameLength = 120;

    public IReadOnlyList<MenuItemListItem> GetAll()
    {
        using var context = new AppDbContext();

        var menuItems = context.MenuItems
            .AsNoTracking()
            .Include(menuItem => menuItem.Restaurant)
            .OrderBy(menuItem => menuItem.Name)
            .ToList();

        return menuItems
            .Select(menuItem => Map(menuItem))
            .ToList();
    }

    public MenuItemListItem? GetById(int id)
    {
        using var context = new AppDbContext();

        var menuItem = context.MenuItems
            .AsNoTracking()
            .Include(item => item.Restaurant)
            .FirstOrDefault(item => item.Id == id);

        return menuItem is null ? null : Map(menuItem);
    }

    public IReadOnlyList<RestaurantChoice> GetRestaurantChoices()
    {
        using var context = new AppDbContext();

        return context.Restaurants
            .AsNoTracking()
            .OrderBy(restaurant => restaurant.Name)
            .Select(restaurant => new RestaurantChoice(
                restaurant.Id,
                restaurant.Name))
            .ToList();
    }

    public ServiceResult Add(string? name, double price, int restaurantId)
    {
        var normalizedName = NormalizeName(name);
        var validationResult = Validate(normalizedName, price);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        using var context = new AppDbContext();
        if (!context.Restaurants.Any(restaurant => restaurant.Id == restaurantId))
        {
            return ServiceResult.Fail("Выбранный ресторан не найден.");
        }

        context.MenuItems.Add(new MenuItem
        {
            Name = normalizedName,
            Price = price,
            RestaurantId = restaurantId,
        });
        context.SaveChanges();
        return ServiceResult.Ok("Блюдо добавлено.");
    }

    public ServiceResult Update(
        int id,
        string? name,
        double price,
        int restaurantId)
    {
        var normalizedName = NormalizeName(name);
        var validationResult = Validate(normalizedName, price);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        using var context = new AppDbContext();
        var menuItem = context.MenuItems.Find(id);
        if (menuItem is null)
        {
            return ServiceResult.Fail("Блюдо с указанным идентификатором не найдено.");
        }

        if (!context.Restaurants.Any(restaurant => restaurant.Id == restaurantId))
        {
            return ServiceResult.Fail("Выбранный ресторан не найден.");
        }

        menuItem.Name = normalizedName;
        menuItem.Price = price;
        menuItem.RestaurantId = restaurantId;
        context.MenuItems.Update(menuItem);
        context.SaveChanges();
        return ServiceResult.Ok("Блюдо изменено.");
    }

    public ServiceResult Delete(int id)
    {
        using var context = new AppDbContext();
        var menuItem = context.MenuItems.Find(id);
        if (menuItem is null)
        {
            return ServiceResult.Fail("Блюдо с указанным идентификатором не найдено.");
        }

        context.MenuItems.Remove(menuItem);
        context.SaveChanges();
        return ServiceResult.Ok("Блюдо удалено.");
    }

    private static MenuItemListItem Map(MenuItem menuItem)
    {
        return new MenuItemListItem(
            menuItem.Id,
            menuItem.Name,
            menuItem.Price,
            menuItem.RestaurantId,
            menuItem.Restaurant.Name);
    }

    private static string NormalizeName(string? name) => name?.Trim() ?? string.Empty;

    private static ServiceResult Validate(string name, double price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult.Fail("Название блюда не может быть пустым.");
        }

        if (name.Length > MaximumNameLength)
        {
            return ServiceResult.Fail(
                $"Название не может быть длиннее {MaximumNameLength} символов.");
        }

        if (!double.IsFinite(price) || price < 0)
        {
            return ServiceResult.Fail(
                "Цена должна быть конечным неотрицательным числом.");
        }

        return ServiceResult.Ok(string.Empty);
    }
}

internal sealed record MenuItemListItem(
    int Id,
    string Name,
    double Price,
    int RestaurantId,
    string RestaurantName);

internal sealed record RestaurantChoice(int Id, string Name);
