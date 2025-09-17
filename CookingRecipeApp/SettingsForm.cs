using System;
using System.Drawing;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public partial class SettingsForm : Form
    {
        public string SelectedTheme { get; private set; } = "Dark";
        public string SelectedLanguage { get; private set; } = "English";

        private ComboBox _themeComboBox;
        private ComboBox _languageComboBox;
        private Button _saveButton;
        private Button _cancelButton;
        private Label _themeLabel;
        private Label _languageLabel;
        private Label _titleLabel;

        public SettingsForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Settings";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Title Label
            _titleLabel = new Label
            {
                Text = "Application Settings",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(20, 20),
                Size = new Size(350, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(_titleLabel);

            // Theme Section
            _themeLabel = new Label
            {
                Text = "Theme:",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(30, 70),
                Size = new Size(100, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(_themeLabel);

            _themeComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Location = new Point(150, 70),
                Size = new Size(200, 25),
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            _themeComboBox.Items.AddRange(new string[] { "Dark", "Light" });
            _themeComboBox.SelectedItem = "Dark";
            this.Controls.Add(_themeComboBox);

            // Language Section
            _languageLabel = new Label
            {
                Text = "Language:",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(30, 110),
                Size = new Size(100, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };
            this.Controls.Add(_languageLabel);

            _languageComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Location = new Point(150, 110),
                Size = new Size(200, 25),
                BackColor = Color.White,
                ForeColor = Color.Black
            };
            _languageComboBox.Items.AddRange(new string[] { "English", "Vietnamese" });
            _languageComboBox.SelectedItem = "English";
            this.Controls.Add(_languageComboBox);

            // Buttons
            _saveButton = new Button
            {
                Text = "Save",
                Font = new Font("Segoe UI", 10),
                Size = new Size(80, 35),
                Location = new Point(190, 170),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _saveButton.FlatAppearance.BorderSize = 0;
            _saveButton.Click += SaveButton_Click;
            this.Controls.Add(_saveButton);

            _cancelButton = new Button
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 10),
                Size = new Size(80, 35),
                Location = new Point(280, 170),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _cancelButton.FlatAppearance.BorderSize = 0;
            _cancelButton.Click += CancelButton_Click;
            this.Controls.Add(_cancelButton);

            // Button hover effects
            _saveButton.MouseEnter += (s, e) => _saveButton.BackColor = Color.FromArgb(0, 105, 217);
            _saveButton.MouseLeave += (s, e) => _saveButton.BackColor = Color.FromArgb(0, 123, 255);

            _cancelButton.MouseEnter += (s, e) => _cancelButton.BackColor = Color.FromArgb(90, 98, 104);
            _cancelButton.MouseLeave += (s, e) => _cancelButton.BackColor = Color.FromArgb(108, 117, 125);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            SelectedTheme = _themeComboBox.SelectedItem?.ToString() ?? "Dark";
            SelectedLanguage = _languageComboBox.SelectedItem?.ToString() ?? "English";

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
