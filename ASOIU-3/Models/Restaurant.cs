namespace ASOIU_3.Models;

/// <summary>
/// Ресторан из справочной таблицы, представляющий сторону «один».
/// </summary>
public sealed class Restaurant
{
    /// <summary>
    /// Получает или задаёт идентификатор ресторана.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Получает или задаёт название ресторана.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задаёт коллекцию блюд ресторана.
    /// </summary>
    public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
}
