using System;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public partial class Form1 : Form
    {
        public readonly DatabaseManager _dbManager;
        private readonly UIManager _uiManager;
        public readonly UserManager _userManager;
        public readonly EventHandler _recipePanelClickHandler;

        public Form1()
        {
            InitializeComponent();
            _dbManager = new DatabaseManager();
            _userManager = new UserManager();
            _uiManager = new UIManager(this);
            _recipePanelClickHandler = RecipePanel_Click;
            _uiManager.SetupLayout();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _dbManager.LoadRecipes(_uiManager.RecipeContainer, _recipePanelClickHandler, null);
            _dbManager.LoadMealTypes(_uiManager.SortComboBox);
            ShowRecipeListView();
            _uiManager.UpdateLoginStatus(_userManager.IsLoggedIn, _userManager.CurrentUsername);
        }

        public void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            string keyword = _uiManager.SearchTextBox.Text.Trim();
            string selectedMealType = _uiManager.SortComboBox.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(keyword) && keyword != "Search recipes...")
            {
                _uiManager.ClearIcon.Visible = true;
                _dbManager.GetRecipeSuggestions(keyword, _uiManager.SuggestionsListBox, selectedMealType);
                _uiManager.SuggestionsListBox.Visible = true;
            }
            else
            {
                _uiManager.ClearIcon.Visible = false;
                _uiManager.SuggestionsListBox.Visible = false;
                _dbManager.LoadRecipes(_uiManager.RecipeContainer, _recipePanelClickHandler, selectedMealType);
            }
        }

        public void SortComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedMealType = _uiManager.SortComboBox.SelectedItem?.ToString();
            string keyword = _uiManager.SearchTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(keyword) && keyword != "Search recipes...")
            {
                _dbManager.SearchRecipes(keyword, _uiManager.RecipeContainer, _recipePanelClickHandler, selectedMealType);
            }
            else
            {
                _dbManager.LoadRecipes(_uiManager.RecipeContainer, _recipePanelClickHandler, selectedMealType);
            }
        }

        public void NewRecipeButton_Click(object sender, EventArgs e)
        {
            ShowRecipeListView();
        }

        public void HomeButton_Click(object sender, EventArgs e)
        {
            ShowRecipeListView();
        }

        public void ShoppingListButton_Click(object sender, EventArgs e)
        {
            if (!_userManager.IsLoggedIn)
            {
                MessageBox.Show("Please log in to use the Shopping List feature.", "Login Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowLoginForm();
                return;
            }
            using (ShoppingListForm shoppingForm = new ShoppingListForm(_dbManager, _userManager.CurrentUserId))
            {
                shoppingForm.ShowDialog();
            }
            HideRecipeListView();
        }

        public void MealPlannerButton_Click(object sender, EventArgs e)
        {
            if (!_userManager.IsLoggedIn)
            {
                MessageBox.Show("Please log in to use the Meal Planner feature.", "Login Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowLoginForm();
                return;
            }
            HideRecipeListView();
            // TODO: Add logic for Meal Planner
        }

        public void CookbooksButton_Click(object sender, EventArgs e)
        {
            ShowRecipeListView();
        }

        public void AddRecipeButton_Click(object sender, EventArgs e)
        {
            if (!_userManager.IsLoggedIn)
            {
                MessageBox.Show("Please log in to add a recipe.", "Login Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ShowLoginForm();
                return;
            }
            using (AddRecipeForm addForm = new AddRecipeForm(_dbManager, null, _userManager.CurrentUserRole))
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    string selectedMealType = _uiManager.SortComboBox.SelectedItem?.ToString();
                    _dbManager.LoadRecipes(_uiManager.RecipeContainer, _recipePanelClickHandler, selectedMealType);
                }
            }
        }

        public void ShowLoginForm()
        {
            using (LoginForm loginForm = new LoginForm(_userManager))
            {
                if (loginForm.ShowDialog() == DialogResult.OK)
                {
                    _uiManager.UpdateLoginStatus(_userManager.IsLoggedIn, _userManager.CurrentUsername);
                }
            }
        }

        public void Logout()
        {
            _userManager.Logout();
            _uiManager.UpdateLoginStatus(_userManager.IsLoggedIn, _userManager.CurrentUsername);
            ShowRecipeListView();
        }

        public void PerformSearch()
        {
            string keyword = _uiManager.SearchTextBox.Text.Trim();
            string selectedMealType = _uiManager.SortComboBox.SelectedItem?.ToString();

            if (!string.IsNullOrEmpty(keyword) && keyword != "Search recipes...")
            {
                _dbManager.SearchRecipes(keyword, _uiManager.RecipeContainer, _recipePanelClickHandler, selectedMealType);
            }
        }

        private void ShowRecipeListView()
        {
            _uiManager.SortComboBox.Visible = true;
            _uiManager.RecipeContainer.Visible = true;
        }

        private void HideRecipeListView()
        {
            _uiManager.SortComboBox.Visible = false;
            _uiManager.RecipeContainer.Visible = false;
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
                RecipeDetailForm detailForm = new RecipeDetailForm(recipeId, _dbManager, _userManager.CurrentUserId, _userManager.IsLoggedIn, _userManager.CurrentUserRole);
                detailForm.RecipeDeleted += (s, args) =>
                {
                    string selectedMealType = _uiManager.SortComboBox.SelectedItem?.ToString();
                    _dbManager.LoadRecipes(_uiManager.RecipeContainer, _recipePanelClickHandler, selectedMealType);
                };
                detailForm.RecipeEdited += (s, args) =>
                {
                    string selectedMealType = _uiManager.SortComboBox.SelectedItem?.ToString();
                    _dbManager.LoadRecipes(_uiManager.RecipeContainer, _recipePanelClickHandler, selectedMealType);
                };
                detailForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Cannot determine recipe ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
