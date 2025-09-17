using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public class Home
    {
        private DatabaseManager _dbManager;
        private int _userId;
        private Form1 _form;
        private Panel _mainPanel;
        private ComboBox _sortComboBox;
        private FlowLayoutPanel _recipeContainer;
        private bool _isInitialized = false;
        private bool _hasRecentPanel = false;
        private Label homeLabel;
        private Panel _headerPanel;

        public FlowLayoutPanel RecipeContainer => _recipeContainer;
        public ComboBox SortComboBox { get => _sortComboBox; set => _sortComboBox = value; }

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
                ShowHomePanel();
            }
        }

        private void InitializeComponents()
        {
            if (_mainPanel != null)
            {
                _form.Controls.Remove(_mainPanel);
                _mainPanel.Dispose();
            }

            int panelWidth = _hasRecentPanel ? _form.Width - 540 : _form.Width - 340;

            _mainPanel = new ModernPanel(15) // Rounded corners
            {
                Name = "HomePanel",
                Location = new Point(320, 10), // Add margin
                Size = new Size(panelWidth, _form.Height - 20),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _form.Controls.Add(_mainPanel);
            _mainPanel.BringToFront();

            SetupMainPanel();
        }

        private void SetupMainPanel()
        {
            _mainPanel.Controls.Clear();

            // Modern gradient background
            if (AppState.isDarkMode)
            {
                _mainPanel.BackColor = Color.FromArgb(45, 45, 48);
            }
            else
            {
                _mainPanel.BackColor = Color.White;
            }

            // Modern header with gradient and shadow
            _headerPanel = new GradientPanel
            {
                Name = "HeaderPanel",
                Location = new Point(0, 0),
                Size = new Size(_mainPanel.Width, 80),
                StartColor = AppState.isDarkMode ? Color.FromArgb(60, 60, 63) : Color.FromArgb(255, 255, 255),
                EndColor = AppState.isDarkMode ? Color.FromArgb(45, 45, 48) : Color.FromArgb(248, 249, 250),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            _mainPanel.Controls.Add(_headerPanel);

            // Modern title with better typography
            homeLabel = new Label
            {
                Text = "Recipe Collection",
                Location = new Point(30, 25),
                Size = new Size(300, 35),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = AppState.isDarkMode ? Color.White : Color.FromArgb(33, 37, 41),
                BackColor = Color.Transparent
            };

            if (!AppState.IsEnglish)
                homeLabel.Text = "Bộ sưu tập công thức";
            else
                homeLabel.Text = "Recipe Collection";

            _headerPanel.Controls.Add(homeLabel);

            // Modern rounded ComboBox
            _sortComboBox = new ModernComboBox
            {
                Name = "SortComboBox",
                Width = 200,
                Height = 40,
                Location = new Point(_headerPanel.Width - 230, 20),
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                BackColor = AppState.isDarkMode ? Color.FromArgb(52, 58, 64) : Color.White,
                ForeColor = AppState.isDarkMode ? Color.White : Color.FromArgb(33, 37, 41),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            _sortComboBox.SelectedIndexChanged += _form.SortComboBox_SelectedIndexChanged;
            _headerPanel.Controls.Add(_sortComboBox);

            // Modern recipe container with card-style layout
            _recipeContainer = new ModernFlowLayoutPanel
            {
                Name = "RecipeContainer",
                Location = new Point(25, 90),
                Size = new Size(_mainPanel.Width - 50, _mainPanel.Height - 110),
                BackColor = Color.Transparent,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(15),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _mainPanel.Controls.Add(_recipeContainer);

            // Add resize event handler
            _form.SizeChanged += Form_SizeChanged;

            UpdateTheme();
        }

        private void Form_SizeChanged(object sender, EventArgs e)
        {
            if (_mainPanel != null && _recipeContainer != null)
            {
                _recipeContainer.Size = new Size(_mainPanel.Width - 50, _mainPanel.Height - 110);
            }
        }

        public void AdjustForRecentPanel(bool hasRecentPanel)
        {
            _hasRecentPanel = hasRecentPanel;

            if (_mainPanel != null)
            {
                int newWidth = hasRecentPanel ? _form.Width - 540 : _form.Width - 340;
                _mainPanel.Size = new Size(newWidth, _form.Height - 20);

                if (_recipeContainer != null)
                {
                    _recipeContainer.Size = new Size(_mainPanel.Width - 50, _mainPanel.Height - 110);
                }

                if (_sortComboBox != null && _headerPanel != null)
                {
                    _sortComboBox.Location = new Point(_headerPanel.Width - 230, 20);
                }
            }
        }

        public void ShowHomePanel()
        {
            if (_mainPanel != null)
            {
                _mainPanel.Visible = true;
                _mainPanel.BringToFront();

                if (_recipeContainer != null && _mainPanel.Width > 50)
                {
                    _recipeContainer.Size = new Size(_mainPanel.Width - 50, _mainPanel.Height - 110);
                }
            }
        }

        public void HideHomePanel()
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

        public void UpdateTheme()
        {
            if (_mainPanel == null) return;

            if (AppState.isDarkMode)
            {
                _mainPanel.BackColor = Color.FromArgb(45, 45, 48);
                _recipeContainer.BackColor = Color.Transparent;
                homeLabel.ForeColor = Color.White;
                if (_headerPanel is GradientPanel gradientHeader)
                {
                    gradientHeader.StartColor = Color.FromArgb(60, 60, 63);
                    gradientHeader.EndColor = Color.FromArgb(45, 45, 48);
                }
                if (_sortComboBox != null)
                {
                    _sortComboBox.BackColor = Color.FromArgb(52, 58, 64);
                    _sortComboBox.ForeColor = Color.White;
                }
            }
            else
            {
                _mainPanel.BackColor = Color.White;
                _recipeContainer.BackColor = Color.Transparent;
                homeLabel.ForeColor = Color.FromArgb(33, 37, 41);
                if (_headerPanel is GradientPanel gradientHeader)
                {
                    gradientHeader.StartColor = Color.White;
                    gradientHeader.EndColor = Color.FromArgb(248, 249, 250);
                }
                if (_sortComboBox != null)
                {
                    _sortComboBox.BackColor = Color.White;
                    _sortComboBox.ForeColor = Color.FromArgb(33, 37, 41);
                }
            }
        }

        public void UpdateLanguage(bool isEnglish)
        {
            if (_sortComboBox == null) return;

            _sortComboBox.Items.Clear();

            if (isEnglish)
            {
                homeLabel.Text = "Recipe Collection";
                _sortComboBox.Items.AddRange(new ComboBoxItem[]
                {
                    new ComboBoxItem{ Value = "All", Display = "All Categories"},
                    new ComboBoxItem{ Value = "Breakfast", Display = "Breakfast"},
                    new ComboBoxItem{ Value = "Lunch", Display = "Lunch"},
                    new ComboBoxItem{ Value = "Dinner", Display = "Dinner"},
                    new ComboBoxItem{ Value = "Dessert", Display = "Dessert"},
                    new ComboBoxItem{ Value = "Snack", Display = "Snack"},
                    new ComboBoxItem{ Value = "Other", Display = "Other"}
                });
            }
            else
            {
                homeLabel.Text = "Bộ sưu tập công thức";
                _sortComboBox.Items.AddRange(new ComboBoxItem[]
                {
                    new ComboBoxItem{ Value = "All", Display = "Tất cả danh mục"},
                    new ComboBoxItem{ Value = "Breakfast", Display = "Bữa sáng"},
                    new ComboBoxItem{ Value = "Lunch", Display = "Bữa trưa"},
                    new ComboBoxItem{ Value = "Dinner", Display = "Bữa tối"},
                    new ComboBoxItem{ Value = "Dessert", Display = "Tráng miệng"},
                    new ComboBoxItem{ Value = "Snack", Display = "Ăn vặt"},
                    new ComboBoxItem{ Value = "Other", Display = "Khác"}
                });
            }

            _sortComboBox.SelectedIndex = 0;
        }
    }

    // Custom modern controls for the Home panel
    public class ModernPanel : Panel
    {
        private int _cornerRadius;

        public ModernPanel(int cornerRadius)
        {
            _cornerRadius = cornerRadius;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw shadow
            using (var shadowBrush = new SolidBrush(Color.FromArgb(25, 0, 0, 0)))
            {
                var shadowRect = new Rectangle(3, 3, Width - 3, Height - 3);
                using (var shadowPath = GetRoundedRectanglePath(shadowRect, _cornerRadius))
                {
                    e.Graphics.FillPath(shadowBrush, shadowPath);
                }
            }

            // Draw main panel
            using (var path = GetRoundedRectanglePath(ClientRectangle, _cornerRadius))
            using (var brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            // Draw subtle border
            using (var borderPen = new Pen(Color.FromArgb(50, 0, 0, 0), 1))
            using (var path = GetRoundedRectanglePath(ClientRectangle, _cornerRadius))
            {
                e.Graphics.DrawPath(borderPen, path);
            }
        }

        private GraphicsPath GetRoundedRectanglePath(Rectangle rect, int cornerRadius)
        {
            var path = new GraphicsPath();
            int diameter = cornerRadius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }

    public class GradientPanel : Panel
    {
        public Color StartColor { get; set; } = Color.White;
        public Color EndColor { get; set; } = Color.FromArgb(248, 249, 250);

        public GradientPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (var brush = new LinearGradientBrush(ClientRectangle, StartColor, EndColor, 90f))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }

            // Add subtle bottom border
            using (var pen = new Pen(Color.FromArgb(30, 0, 0, 0), 1))
            {
                e.Graphics.DrawLine(pen, 0, Height - 1, Width, Height - 1);
            }
        }
    }

    public class ModernComboBox : ComboBox
    {
        public ModernComboBox()
        {
            FlatStyle = FlatStyle.Flat;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw rounded background
            using (var path = GetRoundedRectanglePath(ClientRectangle, 8))
            using (var brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            // Draw border
            using (var borderPen = new Pen(Color.FromArgb(100, 108, 124, 147), 1))
            using (var path = GetRoundedRectanglePath(ClientRectangle, 8))
            {
                e.Graphics.DrawPath(borderPen, path);
            }

            // Draw text
            if (SelectedItem != null)
            {
                string displayText = SelectedItem is ComboBoxItem item ? item.Display : SelectedItem.ToString();
                using (var textBrush = new SolidBrush(ForeColor))
                {
                    var textRect = new Rectangle(10, 0, Width - 30, Height);
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Center
                    };
                    e.Graphics.DrawString(displayText, Font, textBrush, textRect, format);
                }
            }

            // Draw dropdown arrow
            using (var arrowBrush = new SolidBrush(ForeColor))
            {
                var arrowPoints = new Point[]
                {
                    new Point(Width - 20, Height / 2 - 3),
                    new Point(Width - 12, Height / 2 + 3),
                    new Point(Width - 4, Height / 2 - 3)
                };
                e.Graphics.FillPolygon(arrowBrush, arrowPoints);
            }
        }

        private GraphicsPath GetRoundedRectanglePath(Rectangle rect, int cornerRadius)
        {
            var path = new GraphicsPath();
            int diameter = cornerRadius * 2;

            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }

    public class ModernFlowLayoutPanel : FlowLayoutPanel
    {
        public ModernFlowLayoutPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Invalidate(); // Refresh for smooth scrolling appearance
        }
    }
}