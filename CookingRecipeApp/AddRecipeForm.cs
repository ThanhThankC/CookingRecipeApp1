using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public class AddRecipeForm : Form
    {
        private readonly DatabaseManager _dbManager;
        private readonly int? _recipeId; // Null nếu thêm mới, có giá trị nếu chỉnh sửa
        private TextBox _titleTextBox;
        private ComboBox _mealTypeComboBox;
        private TextBox _imagePathTextBox;
        private Button _browseImageButton;
        private ListBox _ingredientsListBox;
        private TextBox _ingredientNameTextBox;
        private TextBox _ingredientQuantityTextBox;
        private Button _addIngredientButton;
        private ListBox _stepsListBox;
        private TextBox _stepDescriptionTextBox;
        private Button _addStepButton;
        private Button _saveButton;

        public AddRecipeForm(DatabaseManager dbManager, int? recipeId = null)
        {
            _dbManager = dbManager;
            _recipeId = recipeId;
            InitializeComponents();
            LoadMealTypes();
            if (_recipeId.HasValue)
            {
                LoadRecipeData(); // Load dữ liệu công thức nếu chỉnh sửa
            }
        }

        private void InitializeComponents()
        {
            this.Text = _recipeId.HasValue ? "Edit Recipe" : "Add New Recipe";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Title
            Label titleLabel = new Label { Text = "Title:", Location = new Point(30, 20), Size = new Size(100, 30) };
            _titleTextBox = new TextBox { Location = new Point(140, 20), Size = new Size(300, 30) };
            this.Controls.Add(titleLabel);
            this.Controls.Add(_titleTextBox);

            // Meal Type
            Label mealTypeLabel = new Label { Text = "Meal Type:", Location = new Point(30, 60), Size = new Size(100, 30) };
            _mealTypeComboBox = new ComboBox { Location = new Point(140, 60), Size = new Size(300, 30) };
            this.Controls.Add(mealTypeLabel);
            this.Controls.Add(_mealTypeComboBox);

            // Image
            Label imageLabel = new Label { Text = "Image Path:", Location = new Point(30, 100), Size = new Size(100, 30) };
            _imagePathTextBox = new TextBox { Location = new Point(140, 100), Size = new Size(300, 30), ReadOnly = true };
            _browseImageButton = new Button { Text = "Browse", Location = new Point(450, 100), Size = new Size(100, 30) };
            _browseImageButton.Click += BrowseImageButton_Click;
            this.Controls.Add(imageLabel);
            this.Controls.Add(_imagePathTextBox);
            this.Controls.Add(_browseImageButton);

            // Ingredients
            Label ingLabel = new Label { Text = "Ingredients:", Location = new Point(30, 140), Size = new Size(100, 30) };
            _ingredientsListBox = new ListBox { Location = new Point(30, 170), Size = new Size(400, 150) };
            Label ingNameLabel = new Label { Text = "Name:", Location = new Point(450, 170), Size = new Size(100, 30) };
            _ingredientNameTextBox = new TextBox { Location = new Point(550, 170), Size = new Size(200, 30) };
            Label ingQtyLabel = new Label { Text = "Quantity:", Location = new Point(450, 210), Size = new Size(100, 30) };
            _ingredientQuantityTextBox = new TextBox { Location = new Point(550, 210), Size = new Size(200, 30) };
            _addIngredientButton = new Button { Text = "Add Ingredient", Location = new Point(550, 250), Size = new Size(200, 30) };
            _addIngredientButton.Click += AddIngredientButton_Click;
            this.Controls.Add(ingLabel);
            this.Controls.Add(_ingredientsListBox);
            this.Controls.Add(ingNameLabel);
            this.Controls.Add(_ingredientNameTextBox);
            this.Controls.Add(ingQtyLabel);
            this.Controls.Add(_ingredientQuantityTextBox);
            this.Controls.Add(_addIngredientButton);

            // Steps
            Label stepsLabel = new Label { Text = "Steps:", Location = new Point(30, 330), Size = new Size(100, 30) };
            _stepsListBox = new ListBox { Location = new Point(30, 360), Size = new Size(400, 150) };
            Label stepDescLabel = new Label { Text = "Description:", Location = new Point(450, 360), Size = new Size(100, 30) };
            _stepDescriptionTextBox = new TextBox { Location = new Point(550, 360), Size = new Size(200, 30) };
            _addStepButton = new Button { Text = "Add Step", Location = new Point(550, 400), Size = new Size(200, 30) };
            _addStepButton.Click += AddStepButton_Click;
            this.Controls.Add(stepsLabel);
            this.Controls.Add(_stepsListBox);
            this.Controls.Add(stepDescLabel);
            this.Controls.Add(_stepDescriptionTextBox);
            this.Controls.Add(_addStepButton);

            // Save
            _saveButton = new Button { Text = "Save Recipe", Location = new Point(30, 520), Size = new Size(150, 35) };
            _saveButton.Click += SaveButton_Click;
            this.Controls.Add(_saveButton);
        }

        private void LoadMealTypes()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbManager._connectionString))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand("SELECT name FROM meal_types ORDER BY meal_type_id", connection);
                    MySqlDataReader reader = command.ExecuteReader();

                    _mealTypeComboBox.Items.Clear();

                    while (reader.Read())
                    {
                        _mealTypeComboBox.Items.Add(reader["name"].ToString());
                    }

                    _mealTypeComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải loại món ăn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRecipeData()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbManager._connectionString))
                {
                    connection.Open();

                    // Load title and image
                    MySqlCommand recipeCommand = new MySqlCommand(
                        "SELECT title, image FROM recipes WHERE recipe_id = @recipeId", connection);
                    recipeCommand.Parameters.AddWithValue("@recipeId", _recipeId);
                    MySqlDataReader reader = recipeCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        _titleTextBox.Text = reader["title"].ToString();
                        _imagePathTextBox.Text = Path.Combine(Application.StartupPath, "..", "..", "..", "Images", Path.GetFileName(reader["image"].ToString()));
                    }
                    reader.Close();

                    // Load meal type
                    MySqlCommand mealTypeCommand = new MySqlCommand(
                        "SELECT m.name FROM meal_types m JOIN recipe_meal_types rm ON m.meal_type_id = rm.meal_type_id WHERE rm.recipe_id = @recipeId", connection);
                    mealTypeCommand.Parameters.AddWithValue("@recipeId", _recipeId);
                    reader = mealTypeCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        _mealTypeComboBox.SelectedItem = reader["name"].ToString();
                    }
                    reader.Close();

                    // Load ingredients
                    MySqlCommand ingredientsCommand = new MySqlCommand(
                        "SELECT name, quantity FROM ingredients WHERE recipe_id = @recipeId ORDER BY ingredient_id", connection);
                    ingredientsCommand.Parameters.AddWithValue("@recipeId", _recipeId);
                    reader = ingredientsCommand.ExecuteReader();
                    _ingredientsListBox.Items.Clear();
                    while (reader.Read())
                    {
                        _ingredientsListBox.Items.Add($"{reader["name"]} - {reader["quantity"]}");
                    }
                    reader.Close();

                    // Load steps
                    MySqlCommand stepsCommand = new MySqlCommand(
                        "SELECT description FROM steps WHERE recipe_id = @recipeId ORDER BY step_order", connection);
                    stepsCommand.Parameters.AddWithValue("@recipeId", _recipeId);
                    reader = stepsCommand.ExecuteReader();
                    _stepsListBox.Items.Clear();
                    while (reader.Read())
                    {
                        _stepsListBox.Items.Add(reader["description"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải dữ liệu công thức: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BrowseImageButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _imagePathTextBox.Text = ofd.FileName;
                }
            }
        }

        private void AddIngredientButton_Click(object sender, EventArgs e)
        {
            string name = _ingredientNameTextBox.Text.Trim();
            string quantity = _ingredientQuantityTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(quantity))
            {
                _ingredientsListBox.Items.Add($"{name} - {quantity}");
                _ingredientNameTextBox.Clear();
                _ingredientQuantityTextBox.Clear();
            }
        }

        private void AddStepButton_Click(object sender, EventArgs e)
        {
            string desc = _stepDescriptionTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(desc))
            {
                _stepsListBox.Items.Add(desc);
                _stepDescriptionTextBox.Clear();
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string title = _titleTextBox.Text.Trim();
            string imagePath = _imagePathTextBox.Text.Trim();
            string mealType = _mealTypeComboBox.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(imagePath) || string.IsNullOrEmpty(mealType))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<(string name, string quantity)> ingredients = new List<(string, string)>();
            foreach (string item in _ingredientsListBox.Items)
            {
                string[] parts = item.Split(new string[] { " - " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    ingredients.Add((parts[0], parts[1]));
                }
            }

            List<string> steps = new List<string>();
            foreach (string step in _stepsListBox.Items)
            {
                steps.Add(step);
            }

            if (ingredients.Count == 0 || steps.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một nguyên liệu và một bước.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_recipeId.HasValue)
            {
                _dbManager.UpdateRecipe(_recipeId.Value, title, Path.GetFileName(imagePath), mealType, ingredients, steps);
            }
            else
            {
                _dbManager.AddRecipe(title, Path.GetFileName(imagePath), mealType, ingredients, steps);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}