using System.ComponentModel;
using System.Globalization;
using ASOIU_3.Services;

namespace ASOIU_3.WinForms;

internal sealed class MainForm : Form
{
    private readonly RestaurantService _restaurantService = new();
    private readonly MenuItemService _menuItemService = new();
    private readonly ReportService _reportService = new();
    private readonly DataGridView _restaurantGrid = CreateGrid();
    private readonly DataGridView _menuItemGrid = CreateGrid();
    private readonly DataGridView _fullReportGrid = CreateGrid();
    private readonly DataGridView _countReportGrid = CreateGrid();
    private readonly DataGridView _averageReportGrid = CreateGrid();

    public MainForm()
    {
        Text = "АСОИУ. Домашнее задание №3. Вариант 22";
        MinimumSize = new Size(900, 620);
        StartPosition = FormStartPosition.CenterScreen;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateRestaurantsTab());
        tabs.TabPages.Add(CreateMenuItemsTab());
        tabs.TabPages.Add(CreateReportTab());
        Controls.Add(tabs);

        Shown += (_, _) => Execute(RefreshAll);
    }

    private TabPage CreateRestaurantsTab()
    {
        var buttons = CreateButtonPanel(
            ("Обновить", (_, _) => Execute(LoadRestaurants)),
            ("Добавить", (_, _) => Execute(AddRestaurant)),
            ("Изменить", (_, _) => Execute(EditRestaurant)),
            ("Удалить", (_, _) => Execute(DeleteRestaurant)));

        return CreateTabPage("Рестораны", _restaurantGrid, buttons);
    }

    private TabPage CreateMenuItemsTab()
    {
        var buttons = CreateButtonPanel(
            ("Обновить", (_, _) => Execute(LoadMenuItems)),
            ("Добавить", (_, _) => Execute(AddMenuItem)),
            ("Изменить", (_, _) => Execute(EditMenuItem)),
            ("Удалить", (_, _) => Execute(DeleteMenuItem)));

        return CreateTabPage("Блюда в меню", _menuItemGrid, buttons);
    }

    private TabPage CreateReportTab()
    {
        var page = new TabPage("Отчёт");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            Padding = new Padding(8),
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(CreateSectionLabel("1. Полный список блюд"), 0, 0);
        layout.Controls.Add(_fullReportGrid, 0, 1);
        layout.Controls.Add(CreateSectionLabel("2. Количество блюд по ресторанам"), 0, 2);
        layout.Controls.Add(_countReportGrid, 0, 3);
        layout.Controls.Add(
            CreateSectionLabel("3. Средняя цена по ресторанам (по убыванию)"),
            0,
            4);
        layout.Controls.Add(_averageReportGrid, 0, 5);

        var refreshButton = new Button
        {
            Text = "Сформировать отчёт",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
        };
        refreshButton.Click += (_, _) => Execute(LoadReport);
        layout.Controls.Add(refreshButton, 0, 6);
        page.Controls.Add(layout);
        return page;
    }

    private void RefreshAll()
    {
        LoadRestaurants();
        LoadMenuItems();
        LoadReport();
    }

    private void LoadRestaurants()
    {
        _restaurantGrid.DataSource = new BindingList<RestaurantGridRow>(
            _restaurantService.GetAll()
                .Select(item => new RestaurantGridRow(
                    item.Id,
                    item.Name,
                    item.MenuItemCount))
                .ToList());
        ConfigureHeaders(
            _restaurantGrid,
            ("Id", "ID"),
            ("Name", "Название"),
            ("MenuItemCount", "Количество блюд"));
    }

    private void LoadMenuItems()
    {
        _menuItemGrid.DataSource = new BindingList<MenuItemGridRow>(
            _menuItemService.GetAll()
                .Select(item => new MenuItemGridRow(
                    item.Id,
                    item.Name,
                    item.RestaurantName,
                    item.Price))
                .ToList());
        ConfigureHeaders(
            _menuItemGrid,
            ("Id", "ID"),
            ("Name", "Блюдо"),
            ("RestaurantName", "Ресторан"),
            ("Price", "Цена, руб."));
        FormatPriceColumn(_menuItemGrid, "Price");
    }

    private void LoadReport()
    {
        var report = _reportService.Generate();

        _fullReportGrid.DataSource = report.FullList
            .Select(row => new FullReportGridRow(
                row.MenuItemName,
                row.RestaurantName,
                row.Price))
            .ToList();
        ConfigureHeaders(
            _fullReportGrid,
            ("MenuItemName", "Блюдо"),
            ("RestaurantName", "Ресторан"),
            ("Price", "Цена, руб."));
        FormatPriceColumn(_fullReportGrid, "Price");

        _countReportGrid.DataSource = report.Counts
            .Select(row => new CountReportGridRow(row.RestaurantName, row.Count))
            .ToList();
        ConfigureHeaders(
            _countReportGrid,
            ("RestaurantName", "Ресторан"),
            ("Count", "Количество"));

        _averageReportGrid.DataSource = report.AveragePrices
            .Select(row => new AverageReportGridRow(
                row.RestaurantName,
                row.AveragePrice))
            .ToList();
        ConfigureHeaders(
            _averageReportGrid,
            ("RestaurantName", "Ресторан"),
            ("AveragePrice", "Средняя цена, руб."));
        FormatPriceColumn(_averageReportGrid, "AveragePrice");
    }

    private void AddRestaurant()
    {
        using var dialog = new RestaurantEditForm();
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        ShowResult(_restaurantService.Add(dialog.RestaurantName));
        RefreshAll();
    }

    private void EditRestaurant()
    {
        var selected = GetSelectedRow<RestaurantGridRow>(_restaurantGrid);
        if (selected is null)
        {
            ShowSelectionWarning();
            return;
        }

        using var dialog = new RestaurantEditForm(selected.Name);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        ShowResult(_restaurantService.Update(selected.Id, dialog.RestaurantName));
        RefreshAll();
    }

    private void DeleteRestaurant()
    {
        var selected = GetSelectedRow<RestaurantGridRow>(_restaurantGrid);
        if (selected is null)
        {
            ShowSelectionWarning();
            return;
        }

        var answer = MessageBox.Show(
            this,
            $"Удалить ресторан «{selected.Name}»?",
            "Подтверждение удаления",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (answer != DialogResult.Yes)
        {
            return;
        }

        ShowResult(_restaurantService.Delete(selected.Id));
        RefreshAll();
    }

    private void AddMenuItem()
    {
        var restaurants = _menuItemService.GetRestaurantChoices();
        if (restaurants.Count == 0)
        {
            MessageBox.Show(
                this,
                "Сначала добавьте хотя бы один ресторан.",
                "Добавление блюда",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        using var dialog = new MenuItemEditForm(restaurants);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        ShowResult(_menuItemService.Add(
            dialog.MenuItemName,
            dialog.Price,
            dialog.RestaurantId));
        RefreshAll();
    }

    private void EditMenuItem()
    {
        var selected = GetSelectedRow<MenuItemGridRow>(_menuItemGrid);
        if (selected is null)
        {
            ShowSelectionWarning();
            return;
        }

        var menuItem = _menuItemService.GetById(selected.Id);
        if (menuItem is null)
        {
            MessageBox.Show(
                this,
                "Выбранное блюдо больше не существует.",
                "Изменение блюда",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            RefreshAll();
            return;
        }

        using var dialog = new MenuItemEditForm(
            _menuItemService.GetRestaurantChoices(),
            menuItem.Name,
            menuItem.Price,
            menuItem.RestaurantId);
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        ShowResult(_menuItemService.Update(
            menuItem.Id,
            dialog.MenuItemName,
            dialog.Price,
            dialog.RestaurantId));
        RefreshAll();
    }

    private void DeleteMenuItem()
    {
        var selected = GetSelectedRow<MenuItemGridRow>(_menuItemGrid);
        if (selected is null)
        {
            ShowSelectionWarning();
            return;
        }

        var answer = MessageBox.Show(
            this,
            $"Удалить блюдо «{selected.Name}»?",
            "Подтверждение удаления",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (answer != DialogResult.Yes)
        {
            return;
        }

        ShowResult(_menuItemService.Delete(selected.Id));
        RefreshAll();
    }

    private void ShowResult(ServiceResult result)
    {
        MessageBox.Show(
            this,
            result.Message,
            result.IsSuccess ? "Операция выполнена" : "Ошибка",
            MessageBoxButtons.OK,
            result.IsSuccess ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
    }

    private void Execute(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                $"Операция не выполнена. {exception.Message}",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static TabPage CreateTabPage(
        string title,
        DataGridView grid,
        Control buttons)
    {
        var page = new TabPage(title);
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(grid, 0, 0);
        layout.Controls.Add(buttons, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private static FlowLayoutPanel CreateButtonPanel(
        params (string Text, EventHandler Handler)[] buttons)
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(8),
            FlowDirection = FlowDirection.LeftToRight,
        };

        foreach (var (text, handler) in buttons)
        {
            var button = new Button { Text = text, AutoSize = true };
            button.Click += handler;
            panel.Controls.Add(button);
        }

        return panel;
    }

    private static DataGridView CreateGrid()
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoGenerateColumns = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        };
    }

    private static Label CreateSectionLabel(string text)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold),
            Padding = new Padding(0, 5, 0, 3),
        };
    }

    private static void ConfigureHeaders(
        DataGridView grid,
        params (string Property, string Header)[] headers)
    {
        foreach (var (property, header) in headers)
        {
            if (grid.Columns[property] is { } column)
            {
                column.HeaderText = header;
            }
        }
    }

    private static void FormatPriceColumn(DataGridView grid, string property)
    {
        if (grid.Columns[property] is { } column)
        {
            column.DefaultCellStyle.Format = "N2";
            column.DefaultCellStyle.FormatProvider =
                CultureInfo.GetCultureInfo("ru-RU");
        }
    }

    private static T? GetSelectedRow<T>(DataGridView grid)
        where T : class
    {
        return grid.CurrentRow?.DataBoundItem as T;
    }

    private void ShowSelectionWarning()
    {
        MessageBox.Show(
            this,
            "Выберите запись в таблице.",
            "Запись не выбрана",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
    }

    private sealed record RestaurantGridRow(int Id, string Name, int MenuItemCount);

    private sealed record MenuItemGridRow(
        int Id,
        string Name,
        string RestaurantName,
        double Price);

    private sealed record FullReportGridRow(
        string MenuItemName,
        string RestaurantName,
        double Price);

    private sealed record CountReportGridRow(string RestaurantName, int Count);

    private sealed record AverageReportGridRow(
        string RestaurantName,
        double AveragePrice);
}
