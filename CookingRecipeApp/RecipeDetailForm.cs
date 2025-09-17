using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public partial class RecipeDetailForm : Form
    {
        private readonly DatabaseManager _dbManager;
        private readonly int _recipeId;
        private readonly int _userId;
        private readonly string _userRole;
        private readonly bool _isLoggedIn;

        private Label _titleLabel;
        private PictureBox _recipeImage;
        private ListBox _ingredientsListBox;
        private ListBox _stepsListBox;
        private Button _deleteButton;
        private Button _editButton;
        private Button _favoriteButton;
        private Button _printButton;
        private Label ingredientsLabel;
        private Label stepsLabel;
        private FlowLayoutPanel _buttonPanel;

        private string _recipeTitle;
        private bool _isFavorited;

        public event EventHandler RecipeDeleted;
        public event EventHandler RecipeEdited;

        public RecipeDetailForm(int recipeId, DatabaseManager dbManager, int userId, bool isLoggedIn, string userRole = "registered")
        {
            _recipeId = recipeId;
            _dbManager = dbManager;
            _userId = userId;
            _isLoggedIn = isLoggedIn;
            _userRole = userRole;
            InitializeComponents();
            LoadRecipeDetails();
        }

        private void InitializeComponents()
        {
            this.Text = !AppState.IsEnglish ? "Chi tiết công thức" : "Recipe Details";
            this.Size = new Size(960, 720); // Tăng kích thước form cho giao diện rộng rãi hơn
            this.StartPosition = FormStartPosition.CenterScreen; // Căn giữa màn hình
            this.BackColor = Color.FromArgb(245, 245, 245); // Màu nền sáng nhẹ
            this.AutoScroll = true;
            this.Font = new Font("Segoe UI", 11); // Tăng kích thước phông chữ mặc định

            // Title Label
            _titleLabel = new Label
            {
                Location = new Point(40, 30),
                Size = new Size(880, 50),
                Font = new Font("Segoe UI", 18, FontStyle.Bold), // Phông chữ lớn hơn
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(17, 24, 39)
            };
            this.Controls.Add(_titleLabel);

            // Recipe Image
            _recipeImage = new PictureBox
            {
                Location = new Point(40, 90),
                Size = new Size(420, 280), // Tăng kích thước hình ảnh
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle, // Thêm viền nhẹ
                BackColor = Color.White
            };
            this.Controls.Add(_recipeImage);

            // Ingredients Label
            ingredientsLabel = new Label
            {
                Location = new Point(40, 390),
                Size = new Size(420, 35),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(31, 41, 55),
                Text = !AppState.IsEnglish ? "Nguyên liệu:" : "Ingredients:"
            };
            this.Controls.Add(ingredientsLabel);

            // Ingredients ListBox
            _ingredientsListBox = new ListBox
            {
                Location = new Point(40, 430),
                Size = new Size(420, 200), // Tăng chiều cao
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11),
                IntegralHeight = false,
                BackColor = Color.FromArgb(255, 255, 255)
            };
            this.Controls.Add(_ingredientsListBox);

            // Steps Label
            stepsLabel = new Label
            {
                Location = new Point(480, 90),
                Size = new Size(420, 35),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(31, 41, 55),
                Text = !AppState.IsEnglish ? "Các bước thực hiện:" : "Steps:"
            };
            this.Controls.Add(stepsLabel);

            // Steps ListBox
            _stepsListBox = new ListBox
            {
                Location = new Point(480, 130),
                Size = new Size(420, 360), // Tăng chiều cao
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11),
                IntegralHeight = false,
                BackColor = Color.FromArgb(255, 255, 255)
            };
            this.Controls.Add(_stepsListBox);

            // Button Panel
            _buttonPanel = new FlowLayoutPanel
            {
                Location = new Point(480, 510),
                Size = new Size(420, 120),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                AutoSize = false
            };
            this.Controls.Add(_buttonPanel);

            CreateActionButtons();
        }

        private void CreateActionButtons()
        {
            string projectPath = Application.StartupPath;
            string relativePath = Path.Combine("..", "..", "..", "Icons");
            string iconsPath = Path.GetFullPath(Path.Combine(projectPath, relativePath));

            // Delete Button
            _deleteButton = CreateActionButton(
                !AppState.IsEnglish ? "  Xóa công thức" : "  Delete Recipe",
                Color.FromArgb(239, 68, 68), // Red-500
                Path.Combine(iconsPath, "deleteIcon.png")
            );
            _deleteButton.Click += DeleteButton_Click;
            _deleteButton.Visible = _isLoggedIn;
            _buttonPanel.Controls.Add(_deleteButton);

            // Edit Button
            _editButton = CreateActionButton(
                !AppState.IsEnglish ? "  Sửa công thức" : "  Edit Recipe",
                Color.FromArgb(34, 197, 94), // Green-500
                Path.Combine(iconsPath, "editIcon.png")
            );
            _editButton.Click += EditButton_Click;
            _buttonPanel.Controls.Add(_editButton);

            // Favorite Button
            _favoriteButton = CreateActionButton(
                !AppState.IsEnglish ? "  Thêm yêu thích" : "  Add to Favorites",
                Color.FromArgb(234, 179, 8), // Yellow-500
                Path.Combine(iconsPath, "blankloveIcon.png")
            );
            _favoriteButton.Click += FavoriteButton_Click;
            _favoriteButton.Visible = _isLoggedIn;
            _buttonPanel.Controls.Add(_favoriteButton);

            // Print Button
            _printButton = CreateActionButton(
                !AppState.IsEnglish ? "  In công thức" : "  Print Recipe",
                Color.FromArgb(107, 114, 128), // Gray-500
                Path.Combine(iconsPath, "printIcon.png")
            );
            _printButton.Click += PrintButton_Click;
            _buttonPanel.Controls.Add(_printButton);
        }

        private Button CreateActionButton(string text, Color backColor, string iconPath)
        {
            var button = new Button
            {
                Text = text,
                Size = new Size(190, 45), // Tăng kích thước nút
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                BackColor = backColor,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                ImageAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };

            // Rounded corners
            button.FlatAppearance.BorderSize = 0;
            button.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, button.Width, button.Height, 10, 10));

            // Hover effects
            Color originalColor = backColor;
            Color hoverColor = ControlPaint.Dark(backColor, 0.15f);
            button.MouseEnter += (s, e) => button.BackColor = hoverColor;
            button.MouseLeave += (s, e) => button.BackColor = originalColor;

            // Load icon if exists
            if (File.Exists(iconPath))
            {
                try
                {
                    using (var originalImage = Image.FromFile(iconPath))
                    {
                        var resizedImage = new Bitmap(originalImage, new Size(24, 24));
                        button.Image = resizedImage;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading icon {iconPath}: {ex.Message}");
                }
            }

            return button;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (!_isLoggedIn)
            {
                MessageBox.Show(
                    !AppState.IsEnglish ? "Vui lòng đăng nhập để xóa công thức." : "Please log in to delete a recipe.",
                    !AppState.IsEnglish ? "Yêu cầu đăng nhập" : "Login Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_userRole != "admin" && _userRole != "registered")
            {
                MessageBox.Show(
                    !AppState.IsEnglish ? "Chỉ quản trị viên và người dùng đã đăng ký mới có thể xóa công thức." : "Only admins and registered users can delete recipes.",
                    !AppState.IsEnglish ? "Không có quyền" : "Permission Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string message = _userRole == "admin"
                ? !AppState.IsEnglish ? "Bạn có chắc chắn muốn xóa vĩnh viễn công thức này không?" : "Are you sure you want to permanently delete this recipe?"
                : !AppState.IsEnglish ? "Bạn có chắc chắn muốn hủy kích hoạt công thức này không?" : "Are you sure you want to deactivate this recipe?";

            string title = _userRole == "admin"
                ? !AppState.IsEnglish ? "Xóa công thức" : "Delete Recipe"
                : !AppState.IsEnglish ? "Hủy kích hoạt công thức" : "Deactivate Recipe";

            if (MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    _dbManager.DeleteRecipe(_recipeId, _userRole);
                    MessageBox.Show(
                        !AppState.IsEnglish ? "Công thức đã được xóa thành công." : "Recipe successfully removed.",
                        !AppState.IsEnglish ? "Thành công" : "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    RecipeDeleted?.Invoke(this, EventArgs.Empty);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        !AppState.IsEnglish ? $"Lỗi khi xóa công thức: {ex.Message}" : $"Error removing recipe: {ex.Message}",
                        !AppState.IsEnglish ? "Lỗi" : "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (!_isLoggedIn)
            {
                MessageBox.Show(
                    !AppState.IsEnglish ? "Vui lòng đăng nhập để chỉnh sửa công thức." : "Please log in to edit a recipe.",
                    !AppState.IsEnglish ? "Yêu cầu đăng nhập" : "Login Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_userRole != "admin")
            {
                MessageBox.Show(
                    !AppState.IsEnglish ? "Chỉ quản trị viên mới có thể chỉnh sửa công thức." : "Only admins can edit recipes.",
                    !AppState.IsEnglish ? "Không có quyền" : "Permission Denied",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Code for edit form is commented out in original, so keeping it as is
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    !AppState.IsEnglish ? $"Lỗi khi chỉnh sửa công thức: {ex.Message}" : $"Error editing recipe: {ex.Message}",
                    !AppState.IsEnglish ? "Lỗi" : "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FavoriteButton_Click(object sender, EventArgs e)
        {
            if (!_isLoggedIn)
            {
                MessageBox.Show(
                    !AppState.IsEnglish ? "Vui lòng đăng nhập để thêm công thức vào yêu thích." : "Please log in to favorite a recipe.",
                    !AppState.IsEnglish ? "Yêu cầu đăng nhập" : "Login Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                if (_isFavorited)
                {
                    _dbManager.RemoveFavorite(_recipeId, _userId);
                    MessageBox.Show(
                        !AppState.IsEnglish ? "Công thức đã được gỡ khỏi yêu thích." : "Recipe removed from favorites.",
                        !AppState.IsEnglish ? "Thành công" : "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _dbManager.AddFavorite(_recipeId, _userId);
                    MessageBox.Show(
                        !AppState.IsEnglish ? "Công thức đã được thêm vào yêu thích!" : "Recipe added to favorites!",
                        !AppState.IsEnglish ? "Thành công" : "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                UpdateFavoriteButton();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    !AppState.IsEnglish ? $"Lỗi khi cập nhật yêu thích: {ex.Message}" : $"Error updating favorites: {ex.Message}",
                    !AppState.IsEnglish ? "Lỗi" : "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            if (!_isLoggedIn)
            {
                MessageBox.Show(
                    !AppState.IsEnglish ? "Vui lòng đăng nhập để in công thức." : "Please log in to print a recipe.",
                    !AppState.IsEnglish ? "Yêu cầu đăng nhập" : "Login Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                PrintDocument printDoc = new PrintDocument();
                printDoc.PrintPage += PrintDoc_PrintPage;

                PrintDialog printDialog = new PrintDialog();
                printDialog.Document = printDoc;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDoc.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    !AppState.IsEnglish ? $"Lỗi khi in công thức: {ex.Message}" : $"Error printing recipe: {ex.Message}",
                    !AppState.IsEnglish ? "Lỗi in" : "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDoc_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font titleFont = new Font("Arial", 18, FontStyle.Bold); // Tăng kích thước phông chữ
            Font headerFont = new Font("Arial", 14, FontStyle.Bold);
            Font bodyFont = new Font("Arial", 11);

            float yPos = 80; // Giảm margin trên
            float leftMargin = 80;
            float pageWidth = e.PageBounds.Width - 160;

            // Print title
            g.DrawString(_recipeTitle, titleFont, Brushes.Black, leftMargin, yPos);
            yPos += 60;

            // Print ingredients
            g.DrawString(!AppState.IsEnglish ? "Nguyên liệu:" : "Ingredients:", headerFont, Brushes.Black, leftMargin, yPos);
            yPos += 30;

            foreach (string ingredient in _ingredientsListBox.Items)
            {
                g.DrawString($"• {ingredient}", bodyFont, Brushes.Black, leftMargin + 20, yPos);
                yPos += 25;
            }

            yPos += 30;

            // Print steps
            g.DrawString(!AppState.IsEnglish ? "Hướng dẫn:" : "Instructions:", headerFont, Brushes.Black, leftMargin, yPos);
            yPos += 30;

            int stepNumber = 1;
            foreach (string step in _stepsListBox.Items)
            {
                string stepText = $"{stepNumber}. {step}";

                // Word wrap for long steps
                string[] words = stepText.Split(' ');
                StringBuilder line = new StringBuilder();

                foreach (string word in words)
                {
                    if (g.MeasureString(line.ToString() + word, bodyFont).Width < pageWidth - 40)
                    {
                        line.Append(word + " ");
                    }
                    else
                    {
                        g.DrawString(line.ToString().Trim(), bodyFont, Brushes.Black, leftMargin + 20, yPos);
                        yPos += 25;
                        line.Clear();
                        line.Append(word + " ");
                    }
                }

                if (line.Length > 0)
                {
                    g.DrawString(line.ToString().Trim(), bodyFont, Brushes.Black, leftMargin + 20, yPos);
                    yPos += 25;
                }

                stepNumber++;
                yPos += 15;
            }

            titleFont.Dispose();
            headerFont.Dispose();
            bodyFont.Dispose();
        }

        private void UpdateFavoriteButton()
        {
            if (_isLoggedIn)
            {
                _isFavorited = _dbManager.IsRecipeFavorited(_recipeId, _userId);

                if (_isFavorited)
                {
                    _favoriteButton.Text = !AppState.IsEnglish ? "  Bỏ yêu thích" : "  Remove favorite";
                    string iconsPath = Path.GetFullPath(Path.Combine(Application.StartupPath, "..", "..", "..", "Icons"));
                    string iconPath = Path.Combine(iconsPath, "fillLoveIcon.png");
                    UpdateButtonIcon(_favoriteButton, iconPath);
                }
                else
                {
                    _favoriteButton.Text = !AppState.IsEnglish ? "  Thêm yêu thích" : "  Add to Favorites";
                    string iconsPath = Path.GetFullPath(Path.Combine(Application.StartupPath, "..", "..", "..", "Icons"));
                    string iconPath = Path.Combine(iconsPath, "blankLoveIcon.png");
                    UpdateButtonIcon(_favoriteButton, iconPath);
                }
            }
        }

        private void UpdateButtonIcon(Button button, string iconPath)
        {
            if (File.Exists(iconPath))
            {
                try
                {
                    if (button.Image != null)
                        button.Image.Dispose();

                    using (var originalImage = Image.FromFile(iconPath))
                    {
                        var resizedImage = new Bitmap(originalImage, new Size(24, 24));
                        button.Image = resizedImage;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        !AppState.IsEnglish ? $"Lỗi khi cập nhật biểu tượng nút: {ex.Message}" : $"Error updating button icon: {ex.Message}");
                }
            }
        }

        private void LoadRecipeDetails()
        {
            if (!AppState.isDarkMode)
            {
                this.BackColor = Color.FromArgb(245, 245, 245); // Màu nền sáng
                _ingredientsListBox.BackColor = Color.FromArgb(255, 255, 255);
                _stepsListBox.BackColor = Color.FromArgb(255, 255, 255);
                ingredientsLabel.ForeColor = Color.FromArgb(31, 41, 55);
                stepsLabel.ForeColor = Color.FromArgb(31, 41, 55);
                _titleLabel.ForeColor = Color.FromArgb(17, 24, 39);
                _recipeImage.BackColor = Color.White;
            }
            else
            {
                this.BackColor = Color.FromArgb(17, 24, 39); // Màu nền tối
                _ingredientsListBox.BackColor = Color.FromArgb(31, 41, 55);
                _stepsListBox.BackColor = Color.FromArgb(31, 41, 55);
                ingredientsLabel.ForeColor = Color.FromArgb(243, 244, 246);
                stepsLabel.ForeColor = Color.FromArgb(243, 244, 246);
                _titleLabel.ForeColor = Color.FromArgb(243, 244, 246);
                _recipeImage.BackColor = Color.FromArgb(31, 41, 55);
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbManager._connectionString))
                {
                    connection.Open();

                    MySqlCommand recipeCommand = new MySqlCommand(
                        "SELECT title, image FROM recipes WHERE recipe_id = @recipeId AND is_active = TRUE", connection);
                    recipeCommand.Parameters.AddWithValue("@recipeId", _recipeId);
                    MySqlDataReader reader = recipeCommand.ExecuteReader();
                    if (reader.Read())
                    {
                        _recipeTitle = reader["title"].ToString();
                        _titleLabel.Text = _recipeTitle;
                        string imagePath = Path.Combine(Application.StartupPath, "..", "..", "..", "Images", Path.GetFileName(reader["image"].ToString()));
                        if (File.Exists(imagePath))
                        {
                            _recipeImage.Image = Image.FromFile(imagePath);
                        }
                    }
                    else
                    {
                        MessageBox.Show(
                            !AppState.IsEnglish ? "Không tìm thấy công thức hoặc công thức không hoạt động." : "Recipe not found or inactive.",
                            !AppState.IsEnglish ? "Lỗi" : "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        this.Close();
                        return;
                    }
                    reader.Close();

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
                UpdateFavoriteButton();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    !AppState.IsEnglish ? $"Lỗi khi tải chi tiết công thức: {ex.Message}" : $"Error loading recipe details: {ex.Message}",
                    !AppState.IsEnglish ? "Lỗi cơ sở dữ liệu" : "Database Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
