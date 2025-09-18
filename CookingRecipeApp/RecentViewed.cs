using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace CookingRecipeApp
{
    public class RecentViewed
    {
        private DatabaseManager _dbManager;
        private int _userId;
        private Form1 _form;
        private Panel _mainPanel;
        private FlowLayoutPanel _recentContainer;
        private Label _titleLabel;
        private Panel _headerPanel;
        private bool _isInitialized = false;

        public FlowLayoutPanel RecentContainer => _recentContainer;

        public void SetupPanel(Form1 form, DatabaseManager dbManager, int userId)
        {
            _dbManager = dbManager;
            _userId = userId;
            _form = form;

            if (!_isInitialized)
            {
                InitializeComponents();
                _isInitialized = true;
            }
            else
            {
                ShowRecentPanel();
            }

            LoadRecentlyViewed();
        }

        private void InitializeComponents()
        {

            // Remove existing panel if it exists
            if (_mainPanel != null)
            {
                _form.Controls.Remove(_mainPanel);
                _mainPanel.Dispose();
            }

            _mainPanel = new Panel
            {
                Name = "RecentViewedPanel",
                Location = new Point(_form.Width - 230, 0),
                Size = new Size(220, _form.Height),
                BackColor = Color.FromArgb(248, 249, 250),
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right
            };

            

            // Add subtle shadow effect
            _mainPanel.Paint += MainPanel_Paint;

            _form.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();

            SetupMainPanel();
        }

        private void MainPanel_Paint(object sender, PaintEventArgs e)
        {
            // Draw subtle left border
            using (Pen borderPen = new Pen(Color.FromArgb(220, 220, 220), 1))
            {
                e.Graphics.DrawLine(borderPen, 0, 0, 0, _mainPanel.Height);
            }
        }

        private void SetupMainPanel()
        {
            // Clear existing controls
            _mainPanel.Controls.Clear();

            // Header panel
            _headerPanel = new Panel
            {
                Name = "RecentHeaderPanel",
                Location = new Point(0, 0),
                Size = new Size(_mainPanel.Width, 60),
                BackColor = Color.FromArgb(240, 244, 248),
                Dock = DockStyle.Top
            };
            _mainPanel.Controls.Add(_headerPanel);

            // Title with icon
            _titleLabel = new Label
            {
                Location = new Point(15, 18),
                Size = new Size(190, 24),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft
            };
            if (!AppState.IsEnglish)
            {
                _titleLabel.Text = "📚 Xem gần đây";
            }
            else
            {
                _titleLabel.Text = "📚 Recently Viewed";
            }
            _headerPanel.Controls.Add(_titleLabel);

            // Separator line
            Panel separator = new Panel
            {
                Location = new Point(15, 50),
                Size = new Size(_mainPanel.Width - 30, 1),
                BackColor = Color.FromArgb(220, 220, 220)
            };
            _headerPanel.Controls.Add(separator);

            // Recent container with improved styling
            _recentContainer = new FlowLayoutPanel
            {
                Name = "RecentContainer",
                Location = new Point(0, 60),
                Size = new Size(_mainPanel.Width, _mainPanel.Height - 60),
                BackColor = Color.Transparent,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(8, 10, 8, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            // Custom scrollbar styling
            _recentContainer.AutoScrollMargin = new Size(0, 10);

            _mainPanel.Controls.Add(_recentContainer);

            UpdateTheme(); 
        }

        private void LoadRecentlyViewed()
        {
            if (_recentContainer == null || _userId <= 0) return;

            _recentContainer.Controls.Clear();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbManager._connectionString))
                {
                    connection.Open();

                    // Get recently viewed recipes for current user, limited to last 5 for better UX
                    string query = @"
                        SELECT DISTINCT r.recipe_id, r.title, r.image, rv.viewed_at
                        FROM recently_viewed rv
                        INNER JOIN recipes r ON rv.recipe_id = r.recipe_id
                        WHERE rv.user_id = @userId AND r.is_active = TRUE
                        ORDER BY rv.viewed_at DESC
                        LIMIT 5";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", _userId);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CreateRecentRecipePanel(
                                    Convert.ToInt32(reader["recipe_id"]),
                                    reader["title"].ToString(),
                                    reader["image"].ToString(),
                                    Convert.ToDateTime(reader["viewed_at"])
                                );
                            }
                        }
                    }
                }

                // Show message if no recent recipes
                if (_recentContainer.Controls.Count == 0)
                {
                    CreateEmptyStatePanel();
                }
            }
            catch (Exception ex)
            {
                if (!AppState.IsEnglish)
                    MessageBox.Show($"Lỗi khi tải công thức đã xem gần đây: {ex.Message}",
                    "Lỗi Cơ Sở Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show($"Error loading recently viewed recipes: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateEmptyStatePanel()
        {
            Panel emptyPanel = new Panel
            {
                Size = new Size(200, 120),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 20, 0, 0)
            };

            Label emptyIcon = new Label
            {
                Text = "👁️",
                Size = new Size(200, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Emoji", 24),
                BackColor = Color.Transparent,
                Location = new Point(0, 20)
            };

            Label emptyLabel = new Label
            {
                Size = new Size(200, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                BackColor = Color.Transparent,
                Location = new Point(0, 65)
            };
            if (!AppState.IsEnglish)
            {
                emptyLabel.Text = "Chưa có công thức gần đây";
            }
            else
                emptyIcon.Text = "No recent recipes";


            Label emptySubLabel = new Label
            {
                Size = new Size(200, 16),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 8),
                BackColor = Color.Transparent,
                Location = new Point(0, 85)
            };
            if (!AppState.IsEnglish)
            {
                emptySubLabel.Text = "Bắt đầu khám phá công thức!";
            }
            else
                emptySubLabel.Text = "Start exploring recipes!";

            emptyPanel.Controls.AddRange(new Control[] { emptyIcon, emptyLabel, emptySubLabel });
            _recentContainer.Controls.Add(emptyPanel);
        }

        private void CreateRecentRecipePanel(int recipeId, string title, string imagePath, DateTime viewedAt)
        {
            Panel recipePanel = new Panel
            {
                Size = new Size(200, 90),
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Cursor = Cursors.Hand,
                Tag = recipeId,
                Margin = new Padding(2, 3, 2, 3)
            };

            // Add rounded corner effect
            recipePanel.Paint += (s, e) => DrawRoundedPanel(e.Graphics, recipePanel);

            // Recipe Image with rounded corners
            PictureBox recipeImage = new PictureBox
            {
                Size = new Size(65, 65),
                Location = new Point(12, 12),
                SizeMode = PictureBoxSizeMode.Zoom,
                Tag = recipeId,
                Cursor = Cursors.Hand
            };

            // Custom paint for rounded image
            recipeImage.Paint += (s, e) => DrawRoundedImage(e.Graphics, recipeImage);

            // Load image
            LoadRecipeImage(recipeImage, imagePath);

            // Recipe Title with better formatting
            Label titleLabel = new Label
            {
                Text = title.Length > 22 ? title.Substring(0, 22) + "..." : title,
                Location = new Point(85, 12),
                Size = new Size(105, 36),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Tag = recipeId,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };

            // Viewed Time with better formatting
            string timeAgo = GetTimeAgo(viewedAt);
            Label timeLabel = new Label
            {
                Text = timeAgo,
                Location = new Point(85, 50),
                Size = new Size(105, 18),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(108, 117, 125),
                Tag = recipeId,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };

            // View indicator
            Label viewIndicator = new Label
            {
                Text = "👀 ",
                Location = new Point(85, 65),
                Size = new Size(15, 15),
                Font = new Font("Segoe UI Emoji", 8),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Tag = recipeId
            };

            // Add click events
            recipePanel.Click += (s, e) => OpenRecipeDetail(recipeId);
            recipeImage.Click += (s, e) => OpenRecipeDetail(recipeId);
            titleLabel.Click += (s, e) => OpenRecipeDetail(recipeId);
            timeLabel.Click += (s, e) => OpenRecipeDetail(recipeId);
            viewIndicator.Click += (s, e) => OpenRecipeDetail(recipeId);

            // Enhanced hover effects with animation-like behavior
            recipePanel.MouseEnter += (s, e) => {
                recipePanel.BackColor = Color.FromArgb(245, 248, 250);
                titleLabel.ForeColor = Color.FromArgb(0, 123, 255);
            };

            recipePanel.MouseLeave += (s, e) => {
                recipePanel.BackColor = Color.White;
                titleLabel.ForeColor = Color.FromArgb(33, 37, 41);
            };

            recipePanel.Controls.AddRange(new Control[] { recipeImage, titleLabel, timeLabel, viewIndicator });
            _recentContainer.Controls.Add(recipePanel);
        }

        private void DrawRoundedPanel(Graphics graphics, Panel panel)
        {
            // Draw subtle rounded rectangle background
            using (GraphicsPath path = GetRoundedRectanglePath(panel.ClientRectangle, 8))
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                // Fill with white background
                using (SolidBrush brush = new SolidBrush(panel.BackColor))
                {
                    graphics.FillPath(brush, path);
                }

                // Draw subtle border
                using (Pen pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                {
                    graphics.DrawPath(pen, path);
                }
            }
        }

        private void DrawRoundedImage(Graphics graphics, PictureBox pictureBox)
        {
            if (pictureBox.Image != null)
            {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (GraphicsPath path = GetRoundedRectanglePath(pictureBox.ClientRectangle, 6))
                {
                    graphics.SetClip(path);
                    graphics.DrawImage(pictureBox.Image, pictureBox.ClientRectangle);
                    graphics.ResetClip();

                    // Draw border around image
                    using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                    {
                        graphics.DrawPath(pen, path);
                    }
                }
            }
        }

        private GraphicsPath GetRoundedRectanglePath(Rectangle rectangle, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            path.AddArc(rectangle.X, rectangle.Y, diameter, diameter, 180, 90);
            path.AddArc(rectangle.Right - diameter, rectangle.Y, diameter, diameter, 270, 90);
            path.AddArc(rectangle.Right - diameter, rectangle.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rectangle.X, rectangle.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseAllFigures();

            return path;
        }

        private void LoadRecipeImage(PictureBox recipeImage, string imagePath)
        {
            string fullImagePath = System.IO.Path.Combine(Application.StartupPath, "..", "..", "..", "Images", System.IO.Path.GetFileName(imagePath));

            if (System.IO.File.Exists(fullImagePath))
            {
                try
                {
                    // Load and resize image for better performance
                    using (Image originalImage = Image.FromFile(fullImagePath))
                    {
                        recipeImage.Image = new Bitmap(originalImage, recipeImage.Size);
                    }
                }
                catch
                {
                    SetDefaultImage(recipeImage);
                }
            }
            else
            {
                SetDefaultImage(recipeImage);
            }
            if (!AppState.IsEnglish)
            {
                _titleLabel.Text = "📚 Xem gần đây";
            }
            else
            {
                _titleLabel.Text = "📚 Recently Viewed";
            }
        }

        private void SetDefaultImage(PictureBox recipeImage)
        {
            // Create a default image with cooking icon
            Bitmap defaultImage = new Bitmap(recipeImage.Width, recipeImage.Height);
            using (Graphics g = Graphics.FromImage(defaultImage))
            {
                g.Clear(Color.FromArgb(240, 240, 240));
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Draw cooking icon
                Font iconFont = new Font("Segoe UI Emoji", 20, FontStyle.Regular);
                string cookingIcon = "🍳";
                SizeF textSize = g.MeasureString(cookingIcon, iconFont);

                float x = (recipeImage.Width - textSize.Width) / 2;
                float y = (recipeImage.Height - textSize.Height) / 2;

                using (SolidBrush brush = new SolidBrush(Color.Gray))
                {
                    g.DrawString(cookingIcon, iconFont, brush, x, y);
                }
            }
            recipeImage.Image = defaultImage;
        }

        private string GetTimeAgo(DateTime viewedAt)
        {
            TimeSpan timeDiff = DateTime.Now - viewedAt;

            if (!AppState.IsEnglish)
                {
                if (timeDiff.TotalMinutes < 1)
                    return "Vừa xong";
                else if (timeDiff.TotalMinutes < 60)
                    return $"{(int)timeDiff.TotalMinutes} phút trước";
                else if (timeDiff.TotalHours < 24)
                    return $"{(int)timeDiff.TotalHours} giờ trước";
                else if (timeDiff.TotalDays < 7)
                    return $"{(int)timeDiff.TotalDays} ngày trước";
                else
                    return viewedAt.ToString("dd MMM");
            }
            else 
            if (timeDiff.TotalMinutes < 1)
                return "Just now";
            else if (timeDiff.TotalMinutes < 60)
                return $"{(int)timeDiff.TotalMinutes}m ago";
            else if (timeDiff.TotalHours < 24)
                return $"{(int)timeDiff.TotalHours}h ago";
            else if (timeDiff.TotalDays < 7)
                return $"{(int)timeDiff.TotalDays}d ago";
            else
                return viewedAt.ToString("MMM dd");
        }

        private void OpenRecipeDetail(int recipeId)
        {
            // Record this view in recently_viewed table (will update timestamp)
            _dbManager.AddToRecentlyViewed(recipeId, _userId);

            RecipeDetailForm detailForm = new RecipeDetailForm(
                recipeId,
                _dbManager,
                _userId,
                _form._userManager.IsLoggedIn,
                _form._userManager.CurrentUserRole);

            detailForm.RecipeDeleted += (s, args) =>
            {
                LoadRecentlyViewed(); // Refresh recent list
                // Also refresh home panel if visible
                if (_form._home?.IsVisible() == true)
                {
                    _form.RefreshHomePanel();
                }
            };

            detailForm.RecipeEdited += (s, args) =>
            {
                LoadRecentlyViewed(); // Refresh recent list
                // Also refresh home panel if visible
                if (_form._home?.IsVisible() == true)
                {
                    _form.RefreshHomePanel();
                }
            };

            detailForm.ShowDialog();

            // Refresh the recent list after viewing (this will move the item to top)
            LoadRecentlyViewed();
        }

        public void ShowRecentPanel()
        {
            if (_mainPanel != null)
            {
                _mainPanel.Visible = true;
                _mainPanel.BringToFront();
                LoadRecentlyViewed();
            }
        }

        public void HideRecentPanel()
        {
            if (_mainPanel != null)
            {
                _mainPanel.Visible = false;
            }
        }

        public bool IsVisible()
        {
            return _mainPanel != null && _mainPanel.Visible;
        }

        public void RefreshData()
        {
            LoadRecentlyViewed();
        }

        public void UpdateTheme()
        {
            if (_mainPanel == null) return;
            if (AppState.isDarkMode)
            {
                _mainPanel.BackColor = Color.Black;
                _headerPanel.BackColor = Color.Gray;
                _titleLabel.ForeColor = Color.White;
            }
            else
            {
                _mainPanel.BackColor = Color.LightYellow;
                _headerPanel.BackColor = Color.Orange;
                _titleLabel.ForeColor = Color.FromArgb(33, 37, 41);
            }
            // Update child panels
            foreach (Control ctrl in _recentContainer.Controls)
            {
                if (ctrl is Panel panel)
                {
                    panel.Invalidate(); // Trigger repaint for rounded corners
                    foreach (Control child in panel.Controls)
                    {
                        if (child is Label label)
                        {
                            label.ForeColor = AppState.isDarkMode ? Color.White : Color.FromArgb(33, 37, 41);
                        }
                    }
                }
            }
        }

        // Method to clear recent viewed history
        public void ClearRecentHistory()
        {
            if (_userId <= 0) return;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_dbManager._connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM recently_viewed WHERE user_id = @userId";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", _userId);
                        command.ExecuteNonQuery();
                    }
                }

                LoadRecentlyViewed(); // Refresh the display
            }
            catch (Exception ex)
            {
                if (!AppState.IsEnglish)
                    MessageBox.Show($"Lỗi khi xóa lịch sử xem gần đây: {ex.Message}",
                    "Lỗi Cơ Sở Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    MessageBox.Show($"Error clearing recent history: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method to add clear button to header (optional)
        public void AddClearButton()
        {
            if (_headerPanel == null) return;

            Button clearButton = new Button
            {
                Text = "Clear",
                Size = new Size(50, 24),
                Location = new Point(_headerPanel.Width - 65, 18),
                Font = new Font("Segoe UI", 8),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            clearButton.FlatAppearance.BorderSize = 0;
            clearButton.Click += (s, e) =>
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to clear your recent viewing history?",
                    "Clear Recent History",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    ClearRecentHistory();
                }
            };

            _headerPanel.Controls.Add(clearButton);
        }
    }

}
