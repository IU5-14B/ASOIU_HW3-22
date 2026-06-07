namespace ASOIU_3.UI;

internal static class ConsoleTable
{
    private const int MaximumColumnWidth = 40;

    public static void Print(
        IReadOnlyList<string> headers,
        IEnumerable<IReadOnlyList<string>> rows)
    {
        var materializedRows = rows.ToList();
        var widths = headers
            .Select((header, index) =>
                materializedRows
                    .Select(row => row[index])
                    .Append(header)
                    .Max(value => Math.Min(value.Length, MaximumColumnWidth)))
            .ToArray();

        PrintRow(headers, widths);
        Console.WriteLine(string.Join("-+-", widths.Select(width => new string('-', width))));

        foreach (var row in materializedRows)
        {
            PrintRow(row, widths);
        }
    }

    private static void PrintRow(IReadOnlyList<string> values, IReadOnlyList<int> widths)
    {
        var cells = values.Select(
            (value, index) => Truncate(value, widths[index]).PadRight(widths[index]));
        Console.WriteLine(string.Join(" | ", cells));
    }

    private static string Truncate(string value, int width)
    {
        if (value.Length <= width)
        {
            return value;
        }

        return width <= 3 ? value[..width] : $"{value[..(width - 3)]}...";
    }
}
