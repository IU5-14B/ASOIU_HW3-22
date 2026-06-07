namespace ASOIU_3.Models;

/// <summary>
/// Блюдо в меню из основной таблицы, представляющее сторону «много».
/// </summary>
public sealed class MenuItem
{
    /// <summary>
    /// Получает или задаёт идентификатор блюда.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Получает или задаёт идентификатор ресторана.
    /// </summary>
    public int RestaurantId { get; set; }

    /// <summary>
    /// Получает или задаёт ресторан, которому принадлежит блюдо.
    /// </summary>
    public Restaurant Restaurant { get; set; } = null!;

    /// <summary>
    /// Получает или задаёт название блюда.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Получает или задаёт цену блюда в рублях.
    /// </summary>
    public double Price { get; set; }
}
