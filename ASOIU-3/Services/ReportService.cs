using ASOIU_3.Data;
using Microsoft.EntityFrameworkCore;

namespace ASOIU_3.Services;

internal sealed class ReportService
{
    public ReportData Generate()
    {
        using var context = new AppDbContext();

        var menuItems = context.MenuItems
            .AsNoTracking()
            .Include(menuItem => menuItem.Restaurant)
            .OrderBy(menuItem => menuItem.Name)
            .ToList();

        var fullList = menuItems
            .Select(menuItem => new FullListReportRow(
                menuItem.Name,
                menuItem.Restaurant.Name,
                menuItem.Price))
            .ToList();

        var countValues = context.MenuItems
            .AsNoTracking()
            .GroupBy(menuItem => menuItem.Restaurant.Name)
            .Select(group => new
            {
                RestaurantName = group.Key,
                Count = group.Count(),
            })
            .OrderBy(row => row.RestaurantName)
            .ToList();

        var counts = countValues
            .Select(row => new CategoryCountReportRow(
                row.RestaurantName,
                row.Count))
            .ToList();

        var averagePriceValues = context.MenuItems
            .AsNoTracking()
            .GroupBy(menuItem => menuItem.Restaurant.Name)
            .Select(group => new
            {
                RestaurantName = group.Key,
                AveragePrice = group.Average(menuItem => menuItem.Price),
            })
            .OrderByDescending(row => row.AveragePrice)
            .ToList();

        var averagePrices = averagePriceValues
            .Select(row => new AveragePriceReportRow(
                row.RestaurantName,
                row.AveragePrice))
            .ToList();

        return new ReportData(fullList, counts, averagePrices);
    }
}

internal sealed record ReportData(
    IReadOnlyList<FullListReportRow> FullList,
    IReadOnlyList<CategoryCountReportRow> Counts,
    IReadOnlyList<AveragePriceReportRow> AveragePrices);

internal sealed record FullListReportRow(
    string MenuItemName,
    string RestaurantName,
    double Price);

internal sealed record CategoryCountReportRow(
    string RestaurantName,
    int Count);

internal sealed record AveragePriceReportRow(
    string RestaurantName,
    double AveragePrice);
