using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public class UIManager
    {
        private readonly Form1 _form;
        private Panel _leftPanel;
        private Panel _searchPanel;
        private TextBox _searchTextBox;
        private PictureBox _searchIcon;
        private PictureBox _clearIcon;
        private ListBox _suggestionsListBox;
        private Button _newRecipeButton;
        private Button _homeButton;
        private Button _shoppingListButton;
        private Button _mealPlannerButton;
        private Button _loginButton;
        private Button _logoutButton;
        private Button _selectedButton;
        private Label _userLabel;

        private Panel _mainPanel;
        private ComboBox _sortComboBox;
        private FlowLayoutPanel _recipeContainer;

        public TextBox SearchTextBox => _searchTextBox;
        public FlowLayoutPanel RecipeContainer => _recipeContainer;
        public ComboBox SortComboBox { get => _sortComboBox; set => _sortComboBox = value; }
        public ListBox SuggestionsListBox => _suggestionsListBox;

        public PictureBox ClearIcon => _clearIcon;

        public UIManager(Form1 form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
        }

        #region UI Setup
        public void SetupLayout()
        {
            SetupLeftPanel();
            SetupMainPanel();
            _form.Resize += Form_Resize;
            SelectButton(_homeButton);
        }

        public void UpdateLoginStatus(bool isLoggedIn, string username)
        {
            _loginButton.Visible = !isLoggedIn;
            _logoutButton.Visible = isLoggedIn;
            _userLabel.Visible = isLoggedIn;
            _userLabel.Text = isLoggedIn ? $"Hello, {username}" : "";
        }

        private void SetupLeftPanel()
        {
            _leftPanel = new Panel
            {
                Name = "LeftPanel",
                Width = 300,
                Height = _form.ClientSize.Height,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(33, 37, 41),
                Dock = DockStyle.Left
            };
            _form.Controls.Add(_leftPanel);

            string placeholderText = "Search recipes...";

            // User Label
            _userLabel = new Label
            {
                Location = new Point(40, 20),
                Size = new Size(220, 30),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.White,
                Visible = false
            };
            _leftPanel.Controls.Add(_userLabel);

            _searchPanel = new Panel
            {
                Width = 220,
                Height = 35,
                Location = new Point(40, 60),
                BackColor = Color.FromArgb(52, 58, 64),
                BorderStyle = BorderStyle.None
            };
            _leftPanel.Controls.Add(_searchPanel);

            _searchIcon = new PictureBox
            {
                Size = new Size(20, 20),
                Location = new Point(10, 8),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };

            string projectPath = Application.StartupPath;
            string relativePath = Path.Combine("..", "..", "..", "Icons");
            string basePath = Path.GetFullPath(Path.Combine(projectPath, relativePath));
            string searchIconPath = Path.Combine(basePath, "icon1.png");
            if (File.Exists(searchIconPath))
            {
                _searchIcon.Image = Image.FromFile(searchIconPath);
            }
            _searchIcon.Click += SearchIcon_Click;
            _searchPanel.Controls.Add(_searchIcon);

            _searchTextBox = new TextBox
            {
                Name = "SearchTextBox",
                Text = placeholderText,
                Width = 150,
                Height = 35,
                Location = new Point(35, 5),
                BackColor = Color.FromArgb(52, 58, 64),
                ForeColor = Color.LightGray,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11)
            };

            _searchTextBox.GotFocus += (s, e) =>
            {
                if (_searchTextBox.Text == placeholderText)
                {
                    _searchTextBox.Text = "";
                    _searchTextBox.ForeColor = Color.White;
                }
            };

            _searchTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_searchTextBox.Text))
                {
                    _searchTextBox.Text = placeholderText;
                    _searchTextBox.ForeColor = Color.LightGray;
                    _clearIcon.Visible = false;
                    _suggestionsListBox.Visible = false;
                }
            };

            _searchTextBox.TextChanged += _form.SearchTextBox_TextChanged;
            _searchTextBox.KeyDown += SearchTextBox_KeyDown;
            _searchPanel.Controls.Add(_searchTextBox);

            _clearIcon = new PictureBox
            {
                Size = new Size(20, 20),
                Location = new Point(195, 8),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand,
                Visible = false
            };
            string clearIconPath = Path.Combine(basePath, "icon2.png");
            if (File.Exists(clearIconPath))
            {
                _clearIcon.Image = Image.FromFile(clearIconPath);
            }
            _clearIcon.Click += ClearIcon_Click;
            _searchPanel.Controls.Add(_clearIcon);

            _suggestionsListBox = new ListBox
            {
                Width = 220,
                Height = 150,
                Location = new Point(40, 100),
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(52, 58, 64),
                ForeColor = Color.White,
                Visible = false
            };
            _suggestionsListBox.Click += SuggestionsListBox_Click;
            _leftPanel.Controls.Add(_suggestionsListBox);

            
            // Login Button
            _loginButton = CreateMenuButton("Login", 160, _form.ShowLoginForm, Path.Combine(basePath, "loginIcon.png"));
            _loginButton.Visible = true;

            // Logout Button
            _logoutButton = CreateMenuButton("Logout", 200, _form.Logout, Path.Combine(basePath, "logoutIcon.png"));
            _logoutButton.Visible = false;

            // New Recipe Button
            _newRecipeButton = CreateMenuButton("New Recipe", 280, _form.AddRecipeButton_Click, Path.Combine(basePath, "icon6.png"));

            // Home Button
            _homeButton = CreateMenuButton("Home", 340, _form.HomeButton_Click, Path.Combine(basePath, "icon7.png"));

            // Shopping List Button
            _shoppingListButton = CreateMenuButton("Shopping List", 400, _form.ShoppingListButton_Click, Path.Combine(basePath, "icon8.png"));

            // Meal Planner Button
            _mealPlannerButton = CreateMenuButton("Meal Planner", 460, _form.MealPlannerButton_Click, Path.Combine(basePath, "icon9.png"));

            _leftPanel.Controls.AddRange(new Control[] {
                _loginButton, _logoutButton, _newRecipeButton, _homeButton, _shoppingListButton, _mealPlannerButton
            });
        }

        private Button CreateMenuButton(string text, int top, EventHandler clickHandler, string iconPath)
        {
            var button = new Button
            {
                Text = text,
                Width = 240,
                Height = 40,
                Location = new Point(40, top),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.TopLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                ImageAlign = ContentAlignment.TopLeft,
                Padding = new Padding(10, 5, 40, 0) // Space for icon on left, text on right
            };

            // Load and resize icon for button
            if (File.Exists(iconPath))
            {
                using (var originalImage = Image.FromFile(iconPath))
                {
                    // Resize icon to 20x20 pixels to prevent stretching
                    var resizedImage = new Bitmap(originalImage, new Size(20, 20));
                    button.Image = resizedImage;
                }
            }

            // Hover effect for menu buttons
            button.MouseEnter += (s, e) => button.BackColor = Color.FromArgb(73, 80, 87);
            button.MouseLeave += (s, e) => button.BackColor = _selectedButton == button ? Color.FromArgb(0, 123, 255) : Color.Transparent;

            if (clickHandler != null)
            {
                button.Click += (s, e) =>
                {
                    SelectButton(button);
                    clickHandler(s, e);
                };
            }

            return button;
        }

        private void SetupMainPanel()
        {
            _mainPanel = new Panel
            {
                Name = "MainPanel",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(320, 20, 20, 20)
            };
            _form.Controls.Add(_mainPanel);

            _sortComboBox = new ComboBox
            {
                Name = "SortComboBox",
                Width = 150,
                Height = 30,
                Location = new Point(340, 20),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(248, 249, 250),
                Font = new Font("Segoe UI", 10)
            };
            _sortComboBox.SelectedIndexChanged += _form.SortComboBox_SelectedIndexChanged;
            _mainPanel.Controls.Add(_sortComboBox);

            _recipeContainer = new FlowLayoutPanel
            {
                Name = "RecipeContainer",
                Location = new Point(330, 60),
                Size = new Size(1000, 800),
                BackColor = Color.White,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };
            _mainPanel.Controls.Add(_recipeContainer);
        }
        #endregion

        #region Button Selection
        private void SelectButton(Button button)
        {
            if (_selectedButton != null)
            {
                _selectedButton.BackColor = Color.Transparent;
                _selectedButton.ForeColor = Color.White;
            }

            _selectedButton = button;
            _selectedButton.BackColor = Color.FromArgb(0, 123, 255);
            _selectedButton.ForeColor = Color.White;
        }
        #endregion

        #region Search Handlers
        private void SearchIcon_Click(object sender, EventArgs e)
        {
            _form.PerformSearch();
            _suggestionsListBox.Visible = false;
        }

        private void ClearIcon_Click(object sender, EventArgs e)
        {
            _searchTextBox.Text = "";
            _clearIcon.Visible = false;
            _suggestionsListBox.Visible = false;
            string selectedMealType = _sortComboBox.SelectedItem?.ToString();
            _form._dbManager.LoadRecipes(_recipeContainer, _form._recipePanelClickHandler, selectedMealType);
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _form.PerformSearch();
                _suggestionsListBox.Visible = false;
                e.SuppressKeyPress = true;
            }
        }

        private void SuggestionsListBox_Click(object sender, EventArgs e)
        {
            if (_suggestionsListBox.SelectedItem != null)
            {
                _searchTextBox.Text = _suggestionsListBox.SelectedItem.ToString();
                _form.PerformSearch();
                _suggestionsListBox.Visible = false;
            }
        }
        #endregion

        private void Form_Resize(object sender, EventArgs e)
        {
            if (_form.ClientSize.Width < 1000)
            {
                _leftPanel.Width = 60;
                _searchPanel.Visible = false;
                _suggestionsListBox.Visible = false;
                _userLabel.Visible = false;
                _loginButton.Visible = false;
                _logoutButton.Visible = false;
                _mainPanel.Padding = new Padding(70, 20, 20, 20);
                _recipeContainer.Location = new Point(70, 60);
                _sortComboBox.Location = new Point(80, 20);
            }
            else
            {
                _leftPanel.Width = 300;
                _searchPanel.Visible = true;
                _userLabel.Visible = _form._userManager.IsLoggedIn;
                _loginButton.Visible = !_form._userManager.IsLoggedIn;
                _logoutButton.Visible = _form._userManager.IsLoggedIn;
                _mainPanel.Padding = new Padding(320, 20, 20, 20);
                _recipeContainer.Location = new Point(330, 60);
                _sortComboBox.Location = new Point(340, 20);
            }
        }
    }
}
