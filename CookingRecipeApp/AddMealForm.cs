using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public partial class AddMealForm : Form
    {
        #region Properties
        public int? SelectedRecipeId { get; private set; }
        public string CustomMealName { get; private set; }
        public string Notes { get; private set; }
        #endregion

        #region Private Fields
        private readonly DatabaseManager _dbManager;
        private readonly DateTime _selectedDate;
        private readonly string _selectedMealType;
        private readonly int? _mealPlanId;
        private readonly int _userId;

        private TabControl _tabControl;
        private TabPage _recipeTab;
        private TabPage _customTab;
        private TextBox _searchTextBox;
        private ListBox _recipesListBox;
        private Label _selectedRecipeLabel;
        private TextBox _customMealTextBox;
        private TextBox _notesTextBox;
        private Button _addButton;
        private Button _deleteButton;
        private Button _cancelButton;
        #endregion

        #region Constructors
        public AddMealForm(DatabaseManager dbManager, DateTime date, string mealType)
            : this(dbManager, date, mealType, null, 0)
        {
        }

        public AddMealForm(DatabaseManager dbManager, int mealPlanId, int userId)
            : this(dbManager, DateTime.Now, "", mealPlanId, userId)
        {
            LoadExistingMealData();
        }

        private AddMealForm(DatabaseManager dbManager, DateTime date, string mealType, int? mealPlanId, int userId)
        {
            ValidateConstructorParameters(dbManager, date, mealType);

            _dbManager = dbManager;
            _selectedDate = date;
            _selectedMealType = mealType;
            _mealPlanId = mealPlanId;
            _userId = userId;

            InitializeComponent();

            if (!_mealPlanId.HasValue)
            {
                LoadRecipes();
            }
        }
        #endregion

        #region Initialization
        private void InitializeComponent()
        {
            ConfigureForm();
            CreateTabControl();
            CreateButtons();
        }

        private void ConfigureForm()
        {
            bool isEditing = _mealPlanId.HasValue;

            this.Text = isEditing
                ? "View/Edit Meal"
                : $"Add {_selectedMealType} for {_selectedDate:MMM dd, yyyy}";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 249, 250);
        }

        private void CreateTabControl()
        {
            _tabControl = new TabControl
            {
                Location = new Point(20, 20),
                Size = new Size(440, 350),
                Font = new Font("Segoe UI", 10)
            };

            CreateRecipeTab();
            CreateCustomTab();

            _tabControl.Controls.Add(_recipeTab);
            _tabControl.Controls.Add(_customTab);
            this.Controls.Add(_tabControl);
        }

        private void CreateRecipeTab()
        {
            _recipeTab = new TabPage("From Recipe")
            {
                BackColor = Color.FromArgb(248, 249, 250)
            };

            AddSearchControls();
            AddRecipeListControls();
        }

        private void AddSearchControls()
        {
            var searchLabel = CreateLabel("Search Recipes:", new Point(20, 20), FontStyle.Bold);
            _recipeTab.Controls.Add(searchLabel);

            _searchTextBox = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(380, 25),
                Font = new Font("Segoe UI", 10),
                Enabled = !_mealPlanId.HasValue
            };
            _searchTextBox.TextChanged += SearchTextBox_TextChanged;
            _recipeTab.Controls.Add(_searchTextBox);
        }

        private void AddRecipeListControls()
        {
            var recipesLabel = CreateLabel("Available Recipes:", new Point(20, 90), FontStyle.Bold);
            _recipeTab.Controls.Add(recipesLabel);

            _recipesListBox = new ListBox
            {
                Location = new Point(20, 120),
                Size = new Size(380, 150),
                Font = new Font("Segoe UI", 10),
                DisplayMember = "title",
                ValueMember = "recipe_id"
            };
            _recipesListBox.SelectedIndexChanged += RecipesListBox_SelectedIndexChanged;
            _recipeTab.Controls.Add(_recipesListBox);

            _selectedRecipeLabel = new Label
            {
                Text = "No recipe selected",
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Location = new Point(20, 280),
                Size = new Size(380, 25),
                ForeColor = Color.FromArgb(108, 117, 125)
            };
            _recipeTab.Controls.Add(_selectedRecipeLabel);
        }

        private void CreateCustomTab()
        {
            _customTab = new TabPage("Custom Meal")
            {
                BackColor = Color.FromArgb(248, 249, 250)
            };

            AddCustomMealControls();
        }

        private void AddCustomMealControls()
        {
            var mealNameLabel = CreateLabel("Meal Name:", new Point(20, 20), FontStyle.Bold);
            _customTab.Controls.Add(mealNameLabel);

            _customMealTextBox = new TextBox
            {
                Location = new Point(20, 50),
                Size = new Size(380, 25),
                Font = new Font("Segoe UI", 10)
            };
            _customTab.Controls.Add(_customMealTextBox);

            var notesLabel = CreateLabel("Notes (Optional):", new Point(20, 90), FontStyle.Bold);
            _customTab.Controls.Add(notesLabel);

            _notesTextBox = new TextBox
            {
                Location = new Point(20, 120),
                Size = new Size(380, 120),
                Font = new Font("Segoe UI", 10),
                Multiline = true
            };
            _customTab.Controls.Add(_notesTextBox);
        }

        private void CreateButtons()
        {
            bool isEditing = _mealPlanId.HasValue;

            CreateAddButton(isEditing);

            if (isEditing)
            {
                CreateDeleteButton();
            }

            CreateCancelButton(isEditing);
            SetupButtonEventHandlers(isEditing);
        }

        private void CreateAddButton(bool isEditing)
        {
            _addButton = new Button
            {
                Text = isEditing ? "Update Meal" : "Add Meal",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(100, 40),
                Location = new Point(isEditing ? 170 : 270, 380),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private void CreateDeleteButton()
        {
            _deleteButton = new Button
            {
                Text = "Delete",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(100, 40),
                Location = new Point(280, 380),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private void CreateCancelButton(bool isEditing)
        {
            _cancelButton = new Button
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 11),
                Size = new Size(100, 40),
                Location = new Point(isEditing ? 390 : 380, 380),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
        }

        private void SetupButtonEventHandlers(bool isEditing)
        {
            _addButton.Click += AddButton_Click;
            _cancelButton.Click += CancelButton_Click;

            if (isEditing && _deleteButton != null)
            {
                _deleteButton.Click += DeleteButton_Click;
                this.Controls.Add(_deleteButton);
            }

            this.Controls.Add(_addButton);
            this.Controls.Add(_cancelButton);

            _tabControl.SelectedIndexChanged += (s, e) => UpdateAddButtonState();
            _customMealTextBox.TextChanged += (s, e) => UpdateAddButtonState();
            _recipesListBox.SelectedIndexChanged += (s, e) => UpdateAddButtonState();
        }
        #endregion

        #region Data Loading
        private void LoadExistingMealData()
        {
            try
            {
                if (!_mealPlanId.HasValue)
                    return;

                DataTable mealData = _dbManager.GetMealPlanById(_mealPlanId.Value);

                if (mealData.Rows.Count == 0)
                {
                    ShowError("Meal plan not found.");
                    this.Close();
                    return;
                }

                ProcessExistingMealData(mealData.Rows[0]);
            }
            catch (Exception ex)
            {
                ShowError($"Error loading meal data: {ex.Message}");
                this.Close();
            }
        }

        private void ProcessExistingMealData(DataRow meal)
        {
            if (meal["recipe_id"] != DBNull.Value)
            {
                LoadRecipeBasedMeal(meal);
            }
            else
            {
                LoadCustomMeal(meal);
            }
        }

        private void LoadRecipeBasedMeal(DataRow meal)
        {
            _tabControl.SelectedTab = _recipeTab;
            LoadRecipes(true);

            int recipeId = Convert.ToInt32(meal["recipe_id"]);
            SelectRecipeInList(recipeId);
        }

        private void LoadCustomMeal(DataRow meal)
        {
            _tabControl.SelectedTab = _customTab;
            _customMealTextBox.Text = meal["custom_meal_name"]?.ToString() ?? string.Empty;
            _notesTextBox.Text = meal["notes"]?.ToString() ?? string.Empty;
            UpdateAddButtonState();
        }

        private void SelectRecipeInList(int recipeId)
        {
            for (int i = 0; i < _recipesListBox.Items.Count; i++)
            {
                if (_recipesListBox.Items[i] is DataRowView row &&
                    Convert.ToInt32(row["recipe_id"]) == recipeId)
                {
                    _recipesListBox.SelectedIndex = i;
                    break;
                }
            }
        }

        private void LoadRecipes(bool includeInactive = false)
        {
            try
            {
                DataTable recipes = _mealPlanId.HasValue && includeInactive
                    ? _dbManager.GetAllRecipes()
                    : _dbManager.GetAllActiveRecipes();
                _recipesListBox.DataSource = recipes;
                _recipesListBox.DisplayMember = "title";
                _recipesListBox.ValueMember = "recipe_id";
            }
            catch (Exception ex)
            {
                ShowError($"Error loading recipes: {ex.Message}");
            }
        }
        #endregion

        #region Event Handlers
        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_mealPlanId.HasValue)
                return;

            string searchTerm = _searchTextBox.Text?.Trim() ?? string.Empty;

            try
            {
                DataTable recipes = string.IsNullOrEmpty(searchTerm)
                    ? _dbManager.GetAllActiveRecipes()
                    : _dbManager.SearchActiveRecipes(searchTerm);

                _recipesListBox.DataSource = recipes;
            }
            catch (Exception ex)
            {
                ShowError($"Error searching recipes: {ex.Message}");
            }
        }

        private void RecipesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateSelectedRecipeLabel();
            UpdateAddButtonState();
        }

        private void UpdateSelectedRecipeLabel()
        {
            if (_recipesListBox.SelectedItem is DataRowView selectedRow)
            {
                _selectedRecipeLabel.Text = $"Selected: {selectedRow["title"]}";
                _selectedRecipeLabel.ForeColor = Color.FromArgb(40, 167, 69);
            }
            else
            {
                _selectedRecipeLabel.Text = "No recipe selected";
                _selectedRecipeLabel.ForeColor = Color.FromArgb(108, 117, 125);
            }
        }

        private void UpdateAddButtonState()
        {
            bool canAdd = CanAddMeal();
            _addButton.Enabled = canAdd;
        }

        private bool CanAddMeal()
        {
            if (_tabControl.SelectedTab == _recipeTab)
            {
                return _recipesListBox.SelectedItem != null;
            }

            if (_tabControl.SelectedTab == _customTab)
            {
                return !string.IsNullOrWhiteSpace(_customMealTextBox.Text);
            }

            return false;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (_mealPlanId.HasValue)
                {
                    UpdateExistingMeal();
                }
                else
                {
                    PrepareNewMealData();
                }

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError($"Error saving meal: {ex.Message}");
            }
        }

        private void PrepareNewMealData()
        {
            if (_tabControl.SelectedTab == _recipeTab)
            {
                PrepareRecipeBasedMeal();
            }
            else if (_tabControl.SelectedTab == _customTab)
            {
                PrepareCustomMeal();
            }
        }

        private void PrepareRecipeBasedMeal()
        {
            if (_recipesListBox.SelectedItem is DataRowView selectedRow)
            {
                SelectedRecipeId = Convert.ToInt32(selectedRow["recipe_id"]);
            }
        }

        private void PrepareCustomMeal()
        {
            CustomMealName = _customMealTextBox.Text?.Trim();
            Notes = _notesTextBox.Text?.Trim();
        }

        private void UpdateExistingMeal()
        {
            int? recipeId = null;
            string customMealName = null;
            string notes = null;

            if (_tabControl.SelectedTab == _recipeTab)
            {
                if (_recipesListBox.SelectedItem is DataRowView selectedRow)
                {
                    recipeId = Convert.ToInt32(selectedRow["recipe_id"]);
                }
            }
            else if (_tabControl.SelectedTab == _customTab)
            {
                customMealName = _customMealTextBox.Text?.Trim();
                notes = _notesTextBox.Text?.Trim();
            }

            _dbManager.UpdateMealPlan(_mealPlanId.Value, recipeId, customMealName, notes);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (!ConfirmDeletion())
                return;

            try
            {
                _dbManager.DeleteMealPlan(_mealPlanId.Value);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError($"Error deleting meal: {ex.Message}");
            }
        }

        private bool ConfirmDeletion()
        {
            var result = MessageBox.Show(
                "Are you sure you want to delete this meal?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            return result == DialogResult.Yes;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        #endregion

        #region Helper Methods
        private Label CreateLabel(string text, Point location, FontStyle style)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10, style),
                Location = location,
                Size = new Size(150, 25)
            };
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ValidateConstructorParameters(DatabaseManager dbManager, DateTime date, string mealType)
        {
            if (dbManager == null)
                throw new ArgumentNullException(nameof(dbManager));

            if (date == default(DateTime))
                throw new ArgumentException("Invalid date provided", nameof(date));
        }
        #endregion
    }
}