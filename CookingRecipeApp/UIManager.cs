using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace CookingRecipeApp
{
    public class UIManager
    {
        private readonly Form1 _form;
        private Home _home;

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
        private Button _menuToggleButton;
        private Button _settingsButton;
        private Label _userLabel;

        private bool _isMenuCollapsed = false;
        private int _fullPanelWidth = 300;
        private int _collapsedPanelWidth = 60;

        private string placeholderText;

        public TextBox SearchTextBox => _searchTextBox;
        public ListBox SuggestionsListBox => _suggestionsListBox;
        public PictureBox ClearIcon => _clearIcon;

        public UIManager(Form1 form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
        }

        public void SetupLayout(Home home)
        {
            _home = home;
            SetupLeftPanel();
            _form.Resize += Form_Resize;
            SelectButton(_homeButton);
        }

        public void UpdateLoginStatus(bool isLoggedIn, string username)
        {
            if (_loginButton != null) _loginButton.Visible = !isLoggedIn && !_isMenuCollapsed;
            if (_logoutButton != null) _logoutButton.Visible = isLoggedIn && !_isMenuCollapsed;
            if (_userLabel != null)
            {
                _userLabel.Visible = isLoggedIn && !_isMenuCollapsed;
                if (!AppState.IsEnglish)
                    _userLabel.Text = isLoggedIn ? $"      Xin chào, {username}" : "";
                else
                    _userLabel.Text = isLoggedIn ? $"      Hello, {username}" : "";
            }
        }

        private void SetupLeftPanel()
        {
            // Remove existing left panel if it exists
            if (_leftPanel != null)
            {
                _form.Controls.Remove(_leftPanel);
                _leftPanel.Dispose();
            }

            _leftPanel = new Panel
            {
                Name = "LeftPanel",
                Width = _fullPanelWidth,
                Height = _form.ClientSize.Height,
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(33, 37, 41),
                Dock = DockStyle.Left
            };
            //if (AppState.isDarkMode)
            //{
            //    _leftPanel.BackColor = Color.Red;
            //}
            //else
            //{
            //    _leftPanel.BackColor = Color.Red;
            //}
                _form.Controls.Add(_leftPanel);

            SetupMenuToggleButton();
            SetupSearchComponents();
            SetupMenuButtons();
            SetupSettingsButton();
        }

        private void SetupMenuToggleButton()
        {
            _menuToggleButton = new Button
            {
                Text = "☰",
                Width = 40,
                Height = 40,
                Location = new Point(10, 10),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };

            _menuToggleButton.FlatAppearance.BorderSize = 1;
            _menuToggleButton.FlatAppearance.BorderColor = Color.FromArgb(73, 80, 87);
            _menuToggleButton.MouseEnter += (s, e) => _menuToggleButton.BackColor = Color.FromArgb(73, 80, 87);
            _menuToggleButton.MouseLeave += (s, e) => _menuToggleButton.BackColor = Color.Transparent;
            _menuToggleButton.Click += MenuToggleButton_Click;

            _leftPanel.Controls.Add(_menuToggleButton);
        }

        private void SetupSearchComponents()
        {
            placeholderText = "Search recipes...";

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

            // Search Panel
            _searchPanel = new Panel
            {
                Width = 220,
                Height = 35,
                Location = new Point(40, 60),
                BackColor = Color.FromArgb(52, 58, 64),
                BorderStyle = BorderStyle.None
            };
            _leftPanel.Controls.Add(_searchPanel);

            // Search Icon
            _searchIcon = new PictureBox
            {
                Size = new Size(20, 20),
                Location = new Point(10, 8),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };

            // Load icon from file
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

            // Search TextBox
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
                    if (_clearIcon != null) _clearIcon.Visible = false;
                    if (_suggestionsListBox != null) _suggestionsListBox.Visible = false;
                }
            };

            _searchTextBox.TextChanged += _form.SearchTextBox_TextChanged;
            _searchTextBox.KeyDown += SearchTextBox_KeyDown;
            _searchPanel.Controls.Add(_searchTextBox);

            // Clear Icon
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

            // Suggestions ListBox
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
        }

        private void SetupMenuButtons()
        {
            string basePath = GetIconBasePath();

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

        private void SetupSettingsButton()
        {
            string basePath = GetIconBasePath();

            _settingsButton = new Button
            {
                Text = "   Settings",
                Width = 240,
                Height = 40,
                Location = new Point(30, _form.ClientSize.Height - 60),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.TopLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                ImageAlign = ContentAlignment.TopLeft,
                Padding = new Padding(10, 5, 40, 0),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            // Load settings icon
            string settingsIconPath = Path.Combine(basePath, "settingsIcon.png");
            if (File.Exists(settingsIconPath))
            {
                try
                {
                    using (var originalImage = Image.FromFile(settingsIconPath))
                    {
                        var resizedImage = new Bitmap(originalImage, new Size(20, 20));
                        _settingsButton.Image = resizedImage;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading settings icon: {ex.Message}");
                }
            }
            else
            {
                // Fallback text icon if image doesn't exist
                _settingsButton.Text = "⚙  Settings";
            }

            // Hover effect
            _settingsButton.MouseEnter += (s, e) => _settingsButton.BackColor = Color.FromArgb(73, 80, 87);
            _settingsButton.MouseLeave += (s, e) => _settingsButton.BackColor = Color.Transparent;

            _settingsButton.Click += SettingsButton_Click;

            _leftPanel.Controls.Add(_settingsButton);
        }

        private void MenuToggleButton_Click(object sender, EventArgs e)
        {
            _isMenuCollapsed = !_isMenuCollapsed;

            if (_isMenuCollapsed)
            {
                // Collapse menu
                _leftPanel.Width = _collapsedPanelWidth;
                _searchPanel.Visible = false;
                _suggestionsListBox.Visible = false;
                _userLabel.Visible = false;
                _loginButton.Visible = false;
                _logoutButton.Visible = false;
                _settingsButton.Visible = false;

                // Show only icons for main buttons
                HideButtonTexts();
            }
            else
            {
                // Expand menu
                _leftPanel.Width = _fullPanelWidth;
                _searchPanel.Visible = true;
                _userLabel.Visible = _form._userManager.IsLoggedIn;
                _loginButton.Visible = !_form._userManager.IsLoggedIn;
                _logoutButton.Visible = _form._userManager.IsLoggedIn;
                _settingsButton.Visible = true;

                // Show button texts
                ShowButtonTexts();
            }
        }

        private void HideButtonTexts()
        {
            if (_newRecipeButton != null) UpdateButtonForCollapsed(_newRecipeButton);
            if (_homeButton != null) UpdateButtonForCollapsed(_homeButton);
            if (_shoppingListButton != null) UpdateButtonForCollapsed(_shoppingListButton);
            if (_mealPlannerButton != null) UpdateButtonForCollapsed(_mealPlannerButton);
        }

        private void ShowButtonTexts()
        {
            if (_newRecipeButton != null) UpdateButtonForExpanded(_newRecipeButton, "   New Recipe");
            if (_homeButton != null) UpdateButtonForExpanded(_homeButton, "   Home");
            if (_shoppingListButton != null) UpdateButtonForExpanded(_shoppingListButton, "   Shopping List");
            if (_mealPlannerButton != null) UpdateButtonForExpanded(_mealPlannerButton, "   Meal Planner");
        }

        private void UpdateButtonForCollapsed(Button button)
        {
            button.Text = "";
            button.Width = 40;
            button.Location = new Point(10, button.Location.Y);
            button.TextImageRelation = TextImageRelation.Overlay;
            button.ImageAlign = ContentAlignment.MiddleCenter;
        }

        private void UpdateButtonForExpanded(Button button, string text)
        {
            button.Text = text;
            button.Width = 240;
            button.Location = new Point(30, button.Location.Y);
            button.TextImageRelation = TextImageRelation.ImageBeforeText;
            button.ImageAlign = ContentAlignment.TopLeft;
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            using (SettingsForm settingsForm = new SettingsForm())
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Apply settings changes
                    ApplyTheme(settingsForm.SelectedTheme);
                    ApplyLanguage(settingsForm.SelectedLanguage);
                }
            }
        }

        private void ApplyTheme(string theme)
        {

            //Color backgroundColor, textColor;
            _form.ChangeTheme(theme == "Dark");

            if (theme == "Dark")
            {
                _leftPanel.BackColor = Color.FromArgb(33, 37, 41);
                _loginButton.BackColor = Color.FromArgb(33, 37, 41);
                _logoutButton.BackColor = Color.FromArgb(33, 37, 41);
                _newRecipeButton.BackColor = Color.FromArgb(33, 37, 41);
                _homeButton.BackColor = Color.FromArgb(33, 37, 41);
                _shoppingListButton.BackColor = Color.FromArgb(33, 37, 41);
                _mealPlannerButton.BackColor = Color.FromArgb(33, 37, 41);
                _settingsButton.BackColor = Color.FromArgb(33, 37, 41);
                _menuToggleButton.BackColor = Color.FromArgb(33, 37, 41);
                _searchTextBox.BackColor = Color.FromArgb(52, 58, 64);
                _searchPanel.BackColor = Color.FromArgb(52, 58, 64);

                
            }
            else
            {
                _leftPanel.BackColor = Color.Orange;
                _loginButton.BackColor = Color.DarkOrange;
                _logoutButton.BackColor = Color.DarkOrange;
                _newRecipeButton.BackColor = Color.DarkOrange;
                _homeButton.BackColor = Color.DarkOrange;
                _shoppingListButton.BackColor = Color.DarkOrange;
                _mealPlannerButton.BackColor = Color.DarkOrange;
                _settingsButton.BackColor = Color.DarkOrange;
                _menuToggleButton.BackColor = Color.DarkOrange;
                _searchTextBox.BackColor = Color.DarkOrange;
                _searchPanel.BackColor = Color.DarkOrange;

            }
        }

        private void ApplyLanguage(string language)
        {
            if (language == "Vietnamese")
            {
                UpdateButtonTexts_Vietnamese();
                _form.ChangeLanguage(false);
            }
            else
            {
                UpdateButtonTexts_English();
                _form.ChangeLanguage(true);
            }
        }

        private void UpdateButtonTexts_Vietnamese()
        {
            if (!_isMenuCollapsed)
            {
                if (_loginButton != null) _loginButton.Text = "   Đăng nhập";
                if (_logoutButton != null) _logoutButton.Text = "   Đăng xuất";
                if (_newRecipeButton != null) _newRecipeButton.Text = "   Công thức mới";
                if (_homeButton != null) _homeButton.Text = "   Trang chủ";
                if (_shoppingListButton != null) _shoppingListButton.Text = "   Danh sách mua sắm";
                if (_mealPlannerButton != null) _mealPlannerButton.Text = "   Lập kế hoạch bữa ăn";
                if (_settingsButton != null) _settingsButton.Text = "⚙  Cài đặt";
            }

            placeholderText = "Tìm kiếm công thức...";

            if (_searchTextBox != null && _searchTextBox.Text == "Search recipes...")
            {
                _searchTextBox.Text = "Tìm kiếm công thức...";
                _searchTextBox.ForeColor = Color.LightGray;
            }
        }

        private void UpdateButtonTexts_English()
        {
            if (!_isMenuCollapsed)
            {
                if (_loginButton != null) _loginButton.Text = "   Login";
                if (_logoutButton != null) _logoutButton.Text = "   Logout";
                if (_newRecipeButton != null) _newRecipeButton.Text = "   New Recipe";
                if (_homeButton != null) _homeButton.Text = "   Home";
                if (_shoppingListButton != null) _shoppingListButton.Text = "   Shopping List";
                if (_mealPlannerButton != null) _mealPlannerButton.Text = "   Meal Planner";
                if (_settingsButton != null) _settingsButton.Text = "⚙  Settings";
            }

            placeholderText = "Search recipes...";

            if (_searchTextBox != null && _searchTextBox.Text == "Tìm kiếm công thức...")
            {
                _searchTextBox.Text = "Search recipes...";
                _searchTextBox.ForeColor = Color.LightGray;
            }
        }

        private string GetIconBasePath()
        {
            string projectPath = Application.StartupPath;
            string relativePath = Path.Combine("..", "..", "..", "Icons");
            return Path.GetFullPath(Path.Combine(projectPath, relativePath));
        }

        private Button CreateMenuButton(string text, int top, EventHandler clickHandler, string iconPath)
        {
            text = "   " + text; // Add spaces for better alignment
            var button = new Button
            {
                Text = text,
                Width = 240,
                Height = 40,
                Location = new Point(30, top),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11),
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.TopLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                ImageAlign = ContentAlignment.TopLeft,
                Padding = new Padding(10, 5, 40, 0)
            };

            // Load and resize icon for button
            if (File.Exists(iconPath))
            {
                try
                {
                    using (var originalImage = Image.FromFile(iconPath))
                    {
                        var resizedImage = new Bitmap(originalImage, new Size(20, 20));
                        button.Image = resizedImage;
                    }
                }
                catch (Exception ex)
                {
                    // Handle icon loading error gracefully
                    System.Diagnostics.Debug.WriteLine($"Error loading icon {iconPath}: {ex.Message}");
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

        public void SetSelectedButton(string buttonName)
        {
            Button targetButton = null;
            switch (buttonName.ToLower())
            {
                case "home":
                    targetButton = _homeButton;
                    break;
                case "shopping":
                    targetButton = _shoppingListButton;
                    break;
                case "mealplanner":
                    targetButton = _mealPlannerButton;
                    break;
                case "newrecipe":
                    targetButton = _newRecipeButton;
                    break;
            }

            if (targetButton != null)
            {
                SelectButton(targetButton);
            }
        }

        private void SearchIcon_Click(object sender, EventArgs e)
        {
            _form.PerformSearch();
            if (_suggestionsListBox != null) _suggestionsListBox.Visible = false;
        }

        private void ClearIcon_Click(object sender, EventArgs e)
        {
            if (_searchTextBox != null)
            {
                _searchTextBox.Text = "Search recipes...";
                _searchTextBox.ForeColor = Color.LightGray;
            }
            if (_clearIcon != null) _clearIcon.Visible = false;
            if (_suggestionsListBox != null) _suggestionsListBox.Visible = false;

            // Refresh recipes to show all
            if (_home != null && _home.SortComboBox != null)
            {
                string selectedMealType = _home.SortComboBox.SelectedItem?.ToString();
                _form._dbManager.LoadRecipes(_home.RecipeContainer, _form._recipePanelClickHandler, selectedMealType);
            }
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                _form.PerformSearch();
                if (_suggestionsListBox != null) _suggestionsListBox.Visible = false;
                e.SuppressKeyPress = true;
            }
        }

        private void SuggestionsListBox_Click(object sender, EventArgs e)
        {
            if (_suggestionsListBox?.SelectedItem != null && _searchTextBox != null)
            {
                _searchTextBox.Text = _suggestionsListBox.SelectedItem.ToString();
                _searchTextBox.ForeColor = Color.White;
                _form.PerformSearch();
                _suggestionsListBox.Visible = false;
            }
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            if (_leftPanel == null) return;

            if (_form.ClientSize.Width < 1000 && !_isMenuCollapsed)
            {
                MenuToggleButton_Click(null, null); // Auto-collapse on small screens
            }
        }

    }
}
