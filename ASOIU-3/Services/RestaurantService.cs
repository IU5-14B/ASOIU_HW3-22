using ASOIU_3.Data;
using Microsoft.EntityFrameworkCore;

namespace ASOIU_3.Services;

internal sealed class RestaurantService
{
    private const int MaximumNameLength = 120;

    public IReadOnlyList<RestaurantListItem> GetAll()
    {
        using var context = new AppDbContext();

        return context.Restaurants
            .AsNoTracking()
            .OrderBy(restaurant => restaurant.Name)
            .Select(restaurant => new RestaurantListItem(
                restaurant.Id,
                restaurant.Name,
                restaurant.MenuItems.Count))
            .ToList();
    }

    public ServiceResult Add(string? name)
    {
        var normalizedName = NormalizeName(name);
        var validationResult = ValidateName(normalizedName);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        using var context = new AppDbContext();
        if (RestaurantNameExists(context, normalizedName))
        {
            return ServiceResult.Fail("Ресторан с таким названием уже существует.");
        }

        context.Restaurants.Add(new Models.Restaurant { Name = normalizedName });
        context.SaveChanges();
        return ServiceResult.Ok("Ресторан добавлен.");
    }

    public ServiceResult Update(int id, string? name)
    {
        var normalizedName = NormalizeName(name);
        var validationResult = ValidateName(normalizedName);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        using var context = new AppDbContext();
        var restaurant = context.Restaurants.Find(id);
        if (restaurant is null)
        {
            return ServiceResult.Fail("Ресторан с указанным идентификатором не найден.");
        }

        if (RestaurantNameExists(context, normalizedName, id))
        {
            return ServiceResult.Fail("Ресторан с таким названием уже существует.");
        }

        restaurant.Name = normalizedName;
        context.Restaurants.Update(restaurant);
        context.SaveChanges();
        return ServiceResult.Ok("Название ресторана изменено.");
    }

    public ServiceResult Delete(int id)
    {
        using var context = new AppDbContext();
        var restaurant = context.Restaurants.Find(id);
        if (restaurant is null)
        {
            return ServiceResult.Fail("Ресторан с указанным идентификатором не найден.");
        }

        var hasMenuItems = context.MenuItems
            .Any(menuItem => menuItem.RestaurantId == id);
        if (hasMenuItems)
        {
            return ServiceResult.Fail(
                "Удаление запрещено: у ресторана есть связанные блюда.");
        }

        context.Restaurants.Remove(restaurant);
        context.SaveChanges();
        return ServiceResult.Ok("Ресторан удалён.");
    }

    private static string NormalizeName(string? name) => name?.Trim() ?? string.Empty;

    private static ServiceResult ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return ServiceResult.Fail("Название не может быть пустым.");
        }

        return name.Length > MaximumNameLength
            ? ServiceResult.Fail(
                $"Название не может быть длиннее {MaximumNameLength} символов.")
            : ServiceResult.Ok(string.Empty);
    }

    private static bool RestaurantNameExists(
        AppDbContext context,
        string name,
        int? excludedId = null)
    {
        var normalizedName = name.ToLower();

        return context.Restaurants.Any(
            restaurant =>
                restaurant.Name.ToLower() == normalizedName
                && (!excludedId.HasValue || restaurant.Id != excludedId.Value));
    }
}

internal sealed record RestaurantListItem(int Id, string Name, int MenuItemCount);
