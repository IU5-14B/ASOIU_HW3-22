using ASOIU_3.Services;

namespace ASOIU_3.WinForms;

internal sealed class MenuItemEditForm : Form
{
    private readonly TextBox _nameTextBox = new() { Dock = DockStyle.Fill };
    private readonly NumericUpDown _priceInput = new()
    {
        Dock = DockStyle.Fill,
        DecimalPlaces = 2,
        Maximum = 100_000_000,
        ThousandsSeparator = true,
    };
    private readonly ComboBox _restaurantComboBox = new()
    {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList,
        DisplayMember = nameof(RestaurantChoice.Name),
        ValueMember = nameof(RestaurantChoice.Id),
    };

    public MenuItemEditForm(
        IReadOnlyList<RestaurantChoice> restaurants,
        string name = "",
        double price = 0,
        int? restaurantId = null)
    {
        Text = string.IsNullOrEmpty(name) ? "Добавление блюда" : "Изменение блюда";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(480, 205);

        _nameTextBox.Text = name;
        _priceInput.Value = Convert.ToDecimal(price);
        _restaurantComboBox.DataSource = restaurants.ToList();
        if (restaurantId.HasValue)
        {
            _restaurantComboBox.SelectedValue = restaurantId.Value;
        }

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(12),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddField(layout, 0, "Название:", _nameTextBox);
        AddField(layout, 1, "Цена, руб.:", _priceInput);
        AddField(layout, 2, "Ресторан:", _restaurantComboBox);

        var buttons = CreateButtons();
        layout.Controls.Add(buttons, 0, 3);
        layout.SetColumnSpan(buttons, 2);
        Controls.Add(layout);
    }

    public string MenuItemName => _nameTextBox.Text.Trim();

    public double Price => Convert.ToDouble(_priceInput.Value);

    public int RestaurantId => (int)_restaurantComboBox.SelectedValue!;

    private static void AddField(
        TableLayoutPanel layout,
        int row,
        string label,
        Control control)
    {
        layout.Controls.Add(new Label
        {
            Text = label,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
        }, 0, row);
        layout.Controls.Add(control, 1, row);
    }

    private FlowLayoutPanel CreateButtons()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
        };
        var cancelButton = new Button
        {
            Text = "Отмена",
            DialogResult = DialogResult.Cancel,
            AutoSize = true,
        };
        var saveButton = new Button
        {
            Text = "Сохранить",
            AutoSize = true,
        };
        saveButton.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(MenuItemName))
            {
                MessageBox.Show(
                    this,
                    "Название блюда не может быть пустым.",
                    "Ошибка ввода",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            DialogResult = DialogResult.OK;
        };
        AcceptButton = saveButton;
        CancelButton = cancelButton;
        panel.Controls.Add(cancelButton);
        panel.Controls.Add(saveButton);
        return panel;
    }
}
