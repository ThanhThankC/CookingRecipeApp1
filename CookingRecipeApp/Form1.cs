using System;
using System.Drawing;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public partial class Form1 : Form
    {
        public readonly DatabaseManager _dbManager;
        private readonly UIManager _uiManager;
        public readonly UserManager _userManager;
        public readonly EventHandler _recipePanelClickHandler;
        public readonly Home _home;
        private readonly ShoppingList _shoppingList;
        private readonly MealPlanner _mealPlanner;
        private readonly RecentViewed _recentViewed;


        public bool isEnglish = true;

        public Form1()
        {
            InitializeComponent();
            _dbManager = new DatabaseManager();
            _userManager = new UserManager();

            _recipePanelClickHandler = RecipePanel_Click;

            // Initialize components first
            _home = new Home();
            _shoppingList = new ShoppingList();
            _mealPlanner = new MealPlanner();
            _recentViewed = new RecentViewed();

            // Setup UI Manager after components are initialized
            _uiManager = new UIManager(this);
            _uiManager.SetupLayout(_home);

            // Setup home panel
            _home.SetupPanel(this, _dbManager, _userManager.CurrentUserId);

            // Setup recent viewed panel if user is logged in
            if (_userManager.IsLoggedIn)
            {
                _recentViewed.SetupPanel(this, _dbManager, _userManager.CurrentUserId);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Load initial data
            _dbManager.LoadMealTypes(_home.SortComboBox);
            _dbManager.LoadRecipes(_home.RecipeContainer, _recipePanelClickHandler, null);
            _uiManager.UpdateLoginStatus(_userManager.IsLoggedIn, _userManager.CurrentUsername);

            // Ensure home panel is visible initially
            _home.ShowHomePanel();

            // Show recent viewed if logged in
            UpdateRecentViewedVisibility();
        }

        public void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            string keyword = _uiManager.SearchTextBox.Text.Trim();
            string selectedMealType = _home.SortComboBox.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(keyword) && (keyword != "Search recipes..." && keyword != "Tìm kiếm công thức..."))
            {
                _uiManager.ClearIcon.Visible = true;
                _dbManager.GetRecipeSuggestions(keyword, _uiManager.SuggestionsListBox, selectedMealType);
                _uiManager.SuggestionsListBox.Visible = true;
            }
            else
            {
                _uiManager.ClearIcon.Visible = false;
                _uiManager.SuggestionsListBox.Visible = false;

                _dbManager.LoadRecipes(_home.RecipeContainer, _recipePanelClickHandler, selectedMealType);
            }
        }

        public void SortComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedMealType = _home.SortComboBox.SelectedItem?.ToString();
            string keyword = _uiManager.SearchTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(keyword) && (keyword != "Search recipes..." && keyword != "Tìm kiếm công thức..."))
            {
                _dbManager.SearchRecipes(keyword, _home.RecipeContainer, _recipePanelClickHandler, selectedMealType);
            }
            else
            {
                _dbManager.LoadRecipes(_home.RecipeContainer, _recipePanelClickHandler, selectedMealType);
            }
        }

        public void AddRecipeButton_Click(object sender, EventArgs e)
        {
            // Hide other panels
            HideAllPanelsExcept(null);

            // TODO: Implement add recipe form
        }

        public void HomeButton_Click(object sender, EventArgs e)
        {
            ShowHomePanel();
        }

        public void ShoppingListButton_Click(object sender, EventArgs e)
        {
            HideAllPanelsExcept("shopping");
            _shoppingList.SetupPanel(this, _dbManager, _userManager.CurrentUserId);
        }

        public void MealPlannerButton_Click(object sender, EventArgs e)
        {
            HideAllPanelsExcept("mealplanner");
            _mealPlanner.SetupPanel(this, _dbManager, _userManager.CurrentUserId);
        }

        public void ShowLoginForm(object sender, EventArgs e)
        {
            using (LoginForm loginForm = new LoginForm(_userManager))
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    _uiManager.UpdateLoginStatus(_userManager.IsLoggedIn, _userManager.CurrentUsername);

                    // Setup recent viewed panel after successful login
                    if (_userManager.IsLoggedIn)
                    {
                        _recentViewed.SetupPanel(this, _dbManager, _userManager.CurrentUserId);
                    }

                    UpdateRecentViewedVisibility();
                    RefreshCurrentPanel();
                }
            }
        }

        public void Logout(object sender, EventArgs e)
        {
            _userManager.Logout();
            _uiManager.UpdateLoginStatus(_userManager.IsLoggedIn, _userManager.CurrentUsername);

            // Hide recent viewed panel when logging out
            _recentViewed.HideRecentPanel();

            ShowHomePanel();
        }

        public void PerformSearch()
        {
            string keyword = _uiManager.SearchTextBox.Text.Trim();
            string selectedMealType = _home.SortComboBox.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(keyword) && (keyword != "Search recipes..." && keyword != "Tìm kiếm công thức..."))
            {
                _dbManager.SearchRecipes(keyword, _home.RecipeContainer, _recipePanelClickHandler, selectedMealType);
            }
        }

        // Method to update recent viewed panel visibility based on login status
        private void UpdateRecentViewedVisibility()
        {
            if (_userManager.IsLoggedIn)
            {
                _recentViewed.ShowRecentPanel();
                // Adjust home panel width to accommodate recent viewed panel
                _home.AdjustForRecentPanel(true);
            }
            else
            {
                _recentViewed.HideRecentPanel();
                // Adjust home panel width to full width
                _home.AdjustForRecentPanel(false);
            }
        }

        // New method to show home panel consistently
        private void ShowHomePanel()
        {
            HideAllPanelsExcept("home");
            _home.ShowHomePanel();

            // Show recent viewed if logged in
            if (_userManager.IsLoggedIn)
            {
                _recentViewed.SetupPanel(this, _dbManager, _userManager.CurrentUserId);
                _recentViewed.ShowRecentPanel();
            }

            RefreshHomePanel();
        }

        // New method to refresh home panel data
        public void RefreshHomePanel()
        {
            string selectedMealType = _home.SortComboBox.SelectedItem?.ToString();
            string keyword = _uiManager.SearchTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(keyword) && (keyword != "Search recipes..." && keyword != "Tìm kiếm công thức..."))
            {
                _dbManager.SearchRecipes(keyword, _home.RecipeContainer, _recipePanelClickHandler, selectedMealType);
            }
            else
            {
                _dbManager.LoadRecipes(_home.RecipeContainer, _recipePanelClickHandler, selectedMealType);
            }
        }

        // Method to refresh recent viewed panel
        public void RefreshRecentViewed()
        {
            if (_userManager.IsLoggedIn)
            {
                _recentViewed.RefreshData();
            }
        }

        // New method to hide all panels except specified one
        private void HideAllPanelsExcept(string exceptPanel)
        {
            if (exceptPanel != "home")
            {
                _home.HideHomePanel();
                _recentViewed.HideRecentPanel();
            }
            if (exceptPanel != "shopping")
                _shoppingList.HideShoppingPanel();
            if (exceptPanel != "mealplanner")
                _mealPlanner.HideMealPlannerPanel();
        }

        // New method to refresh current active panel
        private void RefreshCurrentPanel()
        {
            // Determine which panel is currently active and refresh it
            if (_home.IsVisible())
            {
                RefreshHomePanel();
                if (_userManager.IsLoggedIn)
                {
                    RefreshRecentViewed();
                }
            }
            // Add similar logic for other panels if needed
        }

        private void RecipePanel_Click(object sender, EventArgs e)
        {
            int recipeId = -1;

            if (sender is Panel panel)
            {
                recipeId = (int)panel.Tag;
            }
            else if (sender is PictureBox pictureBox)
            {
                recipeId = (int)pictureBox.Tag;
            }
            else if (sender is Label label)
            {
                recipeId = (int)label.Tag;
            }

            if (recipeId != -1)
            {
                // Add to recently viewed before opening detail form
                if (_userManager.IsLoggedIn)
                {
                    _dbManager.AddToRecentlyViewed(recipeId, _userManager.CurrentUserId);
                }

                RecipeDetailForm detailForm = new RecipeDetailForm(recipeId, _dbManager, _userManager.CurrentUserId, _userManager.IsLoggedIn, _userManager.CurrentUserRole);

                detailForm.RecipeDeleted += (s, args) =>
                {
                    RefreshHomePanel();
                    RefreshRecentViewed();
                };
                detailForm.RecipeEdited += (s, args) =>
                {
                    RefreshHomePanel();
                    RefreshRecentViewed();
                };

                detailForm.ShowDialog();

                // Refresh recent viewed after closing detail form
                if (_userManager.IsLoggedIn)
                {
                    RefreshRecentViewed();
                }
            }
            else
            {
                MessageBox.Show("Cannot determine recipe ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
