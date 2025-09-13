using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public partial class RecipeDetailForm : Form
    {
        private readonly DatabaseManager _dbManager;
        private readonly int _recipeId;

        private Label _titleLabel;
        private PictureBox _recipeImage;
        private ListBox _ingredientsListBox;
        private ListBox _stepsListBox;

        // Sự kiện thông báo xóa và chỉnh sửa công thức
        public event EventHandler RecipeDeleted;
        public event EventHandler RecipeEdited;

        public RecipeDetailForm(int recipeId)
        {
            _recipeId = recipeId;
            _dbManager = new DatabaseManager();
            InitializeComponents();
            LoadRecipeDetails();
        }

        #region UI Initialization
        private void InitializeComponents()
        {
            // Thiết lập tiêu đề và kích thước form
            this.Text = "Recipe Details";
            this.Size = new Size(900, 600);  // Giảm height cho compact, thêm scroll nếu cần
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(320, 50);
            this.BackColor = Color.White;  // White for clean modern look
            this.AutoScroll = true;  // UX: Auto scroll nếu content dài

            // Tạo tiêu đề công thức
            _titleLabel = new Label
            {
                Location = new Point(30, 20),
                Size = new Size(840, 40),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),  // Larger font for title
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(33, 37, 41)
            };
            this.Controls.Add(_titleLabel);

            // Tạo hình ảnh công thức
            _recipeImage = new PictureBox
            {
                Location = new Point(30, 70),
                Size = new Size(400, 250),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None  // Flat
            };
            this.Controls.Add(_recipeImage);

            // Tạo nhãn và danh sách nguyên liệu
            Label ingredientsLabel = new Label
            {
                Text = "Ingredients:",
                Location = new Point(30, 340),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(52, 58, 64)
            };
            this.Controls.Add(ingredientsLabel);

            _ingredientsListBox = new ListBox
            {
                Location = new Point(30, 380),
                Size = new Size(400, 150),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),  // Readable font
                IntegralHeight = false  // UX: Allow partial items for smooth scroll
            };
            this.Controls.Add(_ingredientsListBox);

            // Tạo nhãn và danh sách các bước thực hiện
            Label stepsLabel = new Label
            {
                Text = "Steps:",
                Location = new Point(450, 70),
                Size = new Size(400, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(52, 58, 64)
            };
            this.Controls.Add(stepsLabel);

            _stepsListBox = new ListBox
            {
                Location = new Point(450, 110),
                Size = new Size(400, 300),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                IntegralHeight = false
            };
            this.Controls.Add(_stepsListBox);

            // Tạo nút Delete
            Button deleteButton = new Button
            {
                Text = "Delete Recipe",
                Location = new Point(500, 500),
                Size = new Size(150, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),  // Red for delete
                ForeColor = Color.White
            };
            deleteButton.Click += DeleteButton_Click;
            this.Controls.Add(deleteButton);

            // Tạo nút Edit
            Button editButton = new Button
            {
                Text = "Edit Recipe",
                Location = new Point(650, 500),
                Size = new Size(150, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),  // Blue for edit
                ForeColor = Color.White
            };
            editButton.Click += EditButton_Click;
            this.Controls.Add(editButton);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc muốn xóa công thức này?", "Xác Nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _dbManager.DeleteRecipe(_recipeId);
                RecipeDeleted?.Invoke(this, EventArgs.Empty);
                this.Close();
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            using (AddRecipeForm editForm = new AddRecipeForm(_dbManager, _recipeId))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadRecipeDetails(); // Reload chi tiết công thức
                    RecipeEdited?.Invoke(this, EventArgs.Empty); // Kích hoạt sự kiện chỉnh sửa
                }
            }
        }
        #endregion

        #region Data Loading
        private void LoadRecipeDetails()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbManager._connectionString))
                {
                    connection.Open();

                    // Tải tiêu đề và hình ảnh công thức
                    MySqlCommand recipeCommand = new MySqlCommand(
                        "SELECT title, image FROM recipes WHERE recipe_id = @recipeId", connection);
                    recipeCommand.Parameters.AddWithValue("@recipeId", _recipeId);
                    MySqlDataReader reader = recipeCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        _titleLabel.Text = reader["title"].ToString();
                        string imagePath = Path.Combine(Application.StartupPath, "..", "..", "..", "Images", Path.GetFileName(reader["image"].ToString()));
                        if (File.Exists(imagePath))
                        {
                            _recipeImage.Image = Image.FromFile(imagePath);
                        }
                    }
                    reader.Close();

                    // Load list 
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

                    // Tải danh sách các bước thực hiện
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
                MessageBox.Show($"Lỗi khi tải chi tiết công thức: {ex.Message}", "Lỗi Cơ Sở Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }

}

