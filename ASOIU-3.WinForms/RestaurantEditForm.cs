namespace ASOIU_3.WinForms;

internal sealed class RestaurantEditForm : Form
{
    private readonly TextBox _nameTextBox = new() { Dock = DockStyle.Fill };

    public RestaurantEditForm(string name = "")
    {
        Text = string.IsNullOrEmpty(name) ? "Добавление ресторана" : "Изменение ресторана";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(430, 125);
        _nameTextBox.Text = name;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(12),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.Controls.Add(new Label
        {
            Text = "Название:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
        }, 0, 0);
        layout.Controls.Add(_nameTextBox, 1, 0);
        layout.Controls.Add(CreateButtons(), 0, 1);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 1)!, 2);
        Controls.Add(layout);
    }

    public string RestaurantName => _nameTextBox.Text.Trim();

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
            if (string.IsNullOrWhiteSpace(RestaurantName))
            {
                MessageBox.Show(
                    this,
                    "Название не может быть пустым.",
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
