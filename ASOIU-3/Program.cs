using System.Text;
using ASOIU_3.Data;
using ASOIU_3.UI;

namespace ASOIU_3;

/// <summary>
/// Точка входа приложения для работы с ресторанами и блюдами.
/// </summary>
public static class Program
{
    /// <summary>
    /// Создаёт базу данных, заполняет её начальными данными и запускает приложение.
    /// </summary>
    /// <returns>Код завершения приложения.</returns>
    public static int Main()
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            DatabaseInitializer.Initialize();
            var application = new ConsoleApplication();
            application.Run();
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(
                $"Не удалось запустить приложение: {exception.Message}");
            return 1;
        }
    }
}
