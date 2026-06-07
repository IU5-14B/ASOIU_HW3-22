using System.Globalization;
using ASOIU_3.Services;

namespace ASOIU_3.UI;

internal sealed class ConsoleApplication
{
    private static readonly CultureInfo RussianCulture =
        CultureInfo.GetCultureInfo("ru-RU");

    private readonly RestaurantService _restaurantService = new();
    private readonly MenuItemService _menuItemService = new();
    private readonly ReportService _reportService = new();

    public void Run()
    {
        Console.WriteLine("АСОИУ. Домашнее задание №3. Вариант 22.");

        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Главное меню");
            Console.WriteLine("1. Рестораны");
            Console.WriteLine("2. Блюда в меню");
            Console.WriteLine("3. Отчёт");
            Console.WriteLine("0. Выход");

            var choice = ReadLine("Выберите раздел: ");
            switch (choice)
            {
                case null:
                case "0":
                    Console.WriteLine("Работа завершена.");
                    return;
                case "1":
                    RunSafely(RestaurantMenu);
                    break;
                case "2":
                    RunSafely(MenuItemMenu);
                    break;
                case "3":
                    RunSafely(ShowReport);
                    break;
                default:
                    WriteError("Неизвестный пункт меню.");
                    break;
            }
        }
    }

    private void RestaurantMenu()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Рестораны");
            Console.WriteLine("1. Показать список");
            Console.WriteLine("2. Добавить");
            Console.WriteLine("3. Изменить");
            Console.WriteLine("4. Удалить");
            Console.WriteLine("0. Назад");

            var choice = ReadLine("Выберите действие: ");
            switch (choice)
            {
                case null:
                case "0":
                    return;
                case "1":
                    RunSafely(ShowRestaurants);
                    break;
                case "2":
                    RunSafely(AddRestaurant);
                    break;
                case "3":
                    RunSafely(UpdateRestaurant);
                    break;
                case "4":
                    RunSafely(DeleteRestaurant);
                    break;
                default:
                    WriteError("Неизвестный пункт меню.");
                    break;
            }
        }
    }

    private void MenuItemMenu()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Блюда в меню");
            Console.WriteLine("1. Показать список");
            Console.WriteLine("2. Добавить");
            Console.WriteLine("3. Изменить");
            Console.WriteLine("4. Удалить");
            Console.WriteLine("0. Назад");

            var choice = ReadLine("Выберите действие: ");
            switch (choice)
            {
                case null:
                case "0":
                    return;
                case "1":
                    RunSafely(ShowMenuItems);
                    break;
                case "2":
                    RunSafely(AddMenuItem);
                    break;
                case "3":
                    RunSafely(UpdateMenuItem);
                    break;
                case "4":
                    RunSafely(DeleteMenuItem);
                    break;
                default:
                    WriteError("Неизвестный пункт меню.");
                    break;
            }
        }
    }

    private void ShowRestaurants()
    {
        var restaurants = _restaurantService.GetAll();
        Console.WriteLine();
        Console.WriteLine("Справочник ресторанов");
        ConsoleTable.Print(
            ["ID", "Название", "Блюд"],
            restaurants.Select(restaurant => (IReadOnlyList<string>)
            [
                restaurant.Id.ToString(CultureInfo.InvariantCulture),
                restaurant.Name,
                restaurant.MenuItemCount.ToString(CultureInfo.InvariantCulture),
            ]));
    }

    private void AddRestaurant()
    {
        var name = ReadLine("Название ресторана: ");
        if (name is null)
        {
            return;
        }

        WriteResult(_restaurantService.Add(name));
    }

    private void UpdateRestaurant()
    {
        ShowRestaurants();
        if (!TryReadPositiveInt("ID ресторана: ", out var id))
        {
            return;
        }

        var name = ReadLine("Новое название: ");
        if (name is null)
        {
            return;
        }

        WriteResult(_restaurantService.Update(id, name));
    }

    private void DeleteRestaurant()
    {
        ShowRestaurants();
        if (!TryReadPositiveInt("ID ресторана: ", out var id))
        {
            return;
        }

        if (!Confirm("Удалить ресторан?"))
        {
            Console.WriteLine("Удаление отменено.");
            return;
        }

        WriteResult(_restaurantService.Delete(id));
    }

    private void ShowMenuItems()
    {
        var menuItems = _menuItemService.GetAll();
        Console.WriteLine();
        Console.WriteLine("Список блюд");
        ConsoleTable.Print(
            ["ID", "Блюдо", "Ресторан", "Цена, руб."],
            menuItems.Select(menuItem => (IReadOnlyList<string>)
            [
                menuItem.Id.ToString(CultureInfo.InvariantCulture),
                menuItem.Name,
                menuItem.RestaurantName,
                FormatPrice(menuItem.Price),
            ]));
    }

    private void AddMenuItem()
    {
        var name = ReadLine("Название блюда: ");
        if (name is null
            || !TryReadPrice("Цена, руб.: ", out var price)
            || !TrySelectRestaurant(out var restaurantId))
        {
            return;
        }

        WriteResult(_menuItemService.Add(name, price, restaurantId));
    }

    private void UpdateMenuItem()
    {
        ShowMenuItems();
        if (!TryReadPositiveInt("ID блюда: ", out var id))
        {
            return;
        }

        var current = _menuItemService.GetById(id);
        if (current is null)
        {
            WriteError("Блюдо с указанным идентификатором не найдено.");
            return;
        }

        Console.WriteLine("Оставьте поле пустым, чтобы сохранить текущее значение.");
        var nameInput = ReadLine($"Название [{current.Name}]: ");
        if (nameInput is null)
        {
            return;
        }

        var name = string.IsNullOrWhiteSpace(nameInput) ? current.Name : nameInput;
        if (!TryReadOptionalPrice(current.Price, out var price)
            || !TrySelectRestaurant(out var restaurantId, current.RestaurantId))
        {
            return;
        }

        WriteResult(_menuItemService.Update(id, name, price, restaurantId));
    }

    private void DeleteMenuItem()
    {
        ShowMenuItems();
        if (!TryReadPositiveInt("ID блюда: ", out var id))
        {
            return;
        }

        if (!Confirm("Удалить блюдо?"))
        {
            Console.WriteLine("Удаление отменено.");
            return;
        }

        WriteResult(_menuItemService.Delete(id));
    }

    private void ShowReport()
    {
        var report = _reportService.Generate();

        Console.WriteLine();
        Console.WriteLine("Отчёт");
        Console.WriteLine();
        Console.WriteLine("1. Полный список блюд");
        ConsoleTable.Print(
            ["Блюдо", "Ресторан", "Цена, руб."],
            report.FullList.Select(row => (IReadOnlyList<string>)
            [
                row.MenuItemName,
                row.RestaurantName,
                FormatPrice(row.Price),
            ]));

        Console.WriteLine();
        Console.WriteLine("2. Количество блюд по ресторанам");
        ConsoleTable.Print(
            ["Ресторан", "Количество"],
            report.Counts.Select(row => (IReadOnlyList<string>)
            [
                row.RestaurantName,
                row.Count.ToString(CultureInfo.InvariantCulture),
            ]));

        Console.WriteLine();
        Console.WriteLine("3. Средняя цена по ресторанам (по убыванию)");
        ConsoleTable.Print(
            ["Ресторан", "Средняя цена, руб."],
            report.AveragePrices.Select(row => (IReadOnlyList<string>)
            [
                row.RestaurantName,
                FormatPrice(row.AveragePrice),
            ]));
    }

    private bool TrySelectRestaurant(out int restaurantId, int? currentId = null)
    {
        var restaurants = _menuItemService.GetRestaurantChoices();
        if (restaurants.Count == 0)
        {
            WriteError("Сначала добавьте хотя бы один ресторан.");
            restaurantId = 0;
            return false;
        }

        Console.WriteLine("Доступные рестораны:");
        foreach (var restaurant in restaurants)
        {
            Console.WriteLine($"{restaurant.Id}. {restaurant.Name}");
        }

        var prompt = currentId.HasValue
            ? $"ID ресторана [{currentId.Value}]: "
            : "ID ресторана: ";
        var input = ReadLine(prompt);
        if (input is null)
        {
            restaurantId = 0;
            return false;
        }

        if (currentId.HasValue && string.IsNullOrWhiteSpace(input))
        {
            restaurantId = currentId.Value;
            return true;
        }

        if (!int.TryParse(input, out var parsedRestaurantId)
            || restaurants.All(restaurant => restaurant.Id != parsedRestaurantId))
        {
            WriteError("Введите ID ресторана из списка.");
            restaurantId = 0;
            return false;
        }

        restaurantId = parsedRestaurantId;
        return true;
    }

    private static bool TryReadPositiveInt(string prompt, out int value)
    {
        var input = ReadLine(prompt);
        if (!int.TryParse(input, out value) || value <= 0)
        {
            WriteError("Введите положительное целое число.");
            value = 0;
            return false;
        }

        return true;
    }

    private static bool TryReadPrice(string prompt, out double price)
    {
        var input = ReadLine(prompt);
        return TryParsePrice(input, out price);
    }

    private static bool TryReadOptionalPrice(double currentPrice, out double price)
    {
        var input = ReadLine($"Цена [{FormatPrice(currentPrice)}]: ");
        if (input is null)
        {
            price = 0;
            return false;
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            price = currentPrice;
            return true;
        }

        return TryParsePrice(input, out price);
    }

    private static bool TryParsePrice(string? input, out double price)
    {
        var normalizedInput = input?.Trim().Replace(',', '.');
        if (!double.TryParse(
                normalizedInput,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out price)
            || !double.IsFinite(price)
            || price < 0)
        {
            WriteError("Введите неотрицательную цену, например 450,50.");
            price = 0;
            return false;
        }

        return true;
    }

    private static bool Confirm(string prompt)
    {
        var answer = ReadLine($"{prompt} [д/Н]: ");
        return string.Equals(answer, "д", StringComparison.OrdinalIgnoreCase)
            || string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ReadLine(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()?.Trim();
    }

    private static string FormatPrice(double price) => price.ToString("N2", RussianCulture);

    private static void WriteResult(ServiceResult result)
    {
        if (result.IsSuccess)
        {
            Console.WriteLine(result.Message);
        }
        else
        {
            WriteError(result.Message);
        }
    }

    private static void WriteError(string message)
    {
        Console.WriteLine($"Ошибка: {message}");
    }

    private static void RunSafely(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            WriteError($"операция не выполнена. {exception.Message}");
        }
    }
}
