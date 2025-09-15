using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public class MealPlanner
    {
        private Panel _mealPlannerPanel;
        private Form1 _form;
        private DatabaseManager _dbManager;
        private int _currentUserId;

        private DateTimePicker _datePicker;
        private Button _prevWeekButton;
        private Button _nextWeekButton;
        private Label _weekLabel;
        private FlowLayoutPanel _dayContainer;
        private Button _addMealButton;

        private DateTime _currentWeekStart;
        private readonly string[] _daysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        private readonly string[] _mealTypes = { "Breakfast", "Lunch", "Dinner", "Snack" };
        private DataTable _cachedMealData;

        public MealPlanner()
        {
            _currentWeekStart = GetMondayOfWeek(DateTime.Now);
        }

        public void SetupPanel(Form1 form, DatabaseManager dbManager, int userId)
        {
            _form = form;
            _dbManager = dbManager;
            _currentUserId = userId;

            if (_mealPlannerPanel != null)
            {
                _form.Controls.Remove(_mealPlannerPanel);
                _mealPlannerPanel.Dispose();
            }

            CreateMealPlannerPanel();
            LoadWeekView();
        }

        private void CreateMealPlannerPanel()
        {
            _mealPlannerPanel = new Panel
            {
                Name = "MealPlannerPanel",
                Location = new Point(300, 0),
                Size = new Size(_form.ClientSize.Width - 300, _form.ClientSize.Height),
                BackColor = Color.FromArgb(248, 249, 250),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                AutoScroll = true
            };

            CreateHeader();
            CreateWeekNavigation();
            CreateDayContainer();
            CreateActionButtons();

            _form.Controls.Add(_mealPlannerPanel);
        }

        private void CreateHeader()
        {
            Label headerLabel = new Label
            {
                Text = "Meal Planner",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Location = new Point(20, 20),
                Size = new Size(300, 40),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _mealPlannerPanel.Controls.Add(headerLabel);
        }

        private void CreateWeekNavigation()
        {
            _prevWeekButton = new Button
            {
                Text = "◀ Previous",
                Font = new Font("Segoe UI", 10),
                Size = new Size(100, 35),
                Location = new Point(20, 80),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _prevWeekButton.Click += PrevWeekButton_Click;
            _mealPlannerPanel.Controls.Add(_prevWeekButton);

            _weekLabel = new Label
            {
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41),
                Size = new Size(300, 35),
                Location = new Point(130, 80),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _mealPlannerPanel.Controls.Add(_weekLabel);

            _nextWeekButton = new Button
            {
                Text = " Next   ▶",
                Font = new Font("Segoe UI", 10),
                Size = new Size(100, 35),
                Location = new Point(440, 80),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _nextWeekButton.Click += NextWeekButton_Click;
            _mealPlannerPanel.Controls.Add(_nextWeekButton);

            _datePicker = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(560, 85),
                Size = new Size(120, 25),
                Value = DateTime.Now
            };
            _datePicker.ValueChanged += DatePicker_ValueChanged;
            _mealPlannerPanel.Controls.Add(_datePicker);
        }

        private void CreateDayContainer()
        {
            _dayContainer = new FlowLayoutPanel
            {
                Location = new Point(20, 140),
                Size = new Size(_mealPlannerPanel.Width - 40, 500),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.White
            };
            _mealPlannerPanel.Controls.Add(_dayContainer);
        }

        private void CreateActionButtons()
        {
            _addMealButton = new Button
            {
                Text = "Add Meal",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(120, 40),
                Location = new Point(20, 660),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _addMealButton.Click += AddMealButton_Click;
            _mealPlannerPanel.Controls.Add(_addMealButton);
        }

        private void LoadWeekView()
        {
            _weekLabel.Text = $"{_currentWeekStart:MMM dd} - {_currentWeekStart.AddDays(6):MMM dd, yyyy}";
            _dayContainer.Controls.Clear();

            try
            {
                _cachedMealData = _dbManager.GetMealPlanForWeek(_currentUserId, _currentWeekStart);

                for (int day = 0; day < 7; day++)
                {
                    DateTime currentDay = _currentWeekStart.AddDays(day);
                    GroupBox dayGroup = CreateDayGroup(currentDay, _daysOfWeek[day]);
                    _dayContainer.Controls.Add(dayGroup);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading meal plan data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private GroupBox CreateDayGroup(DateTime date, string dayName)
        {
            GroupBox group = new GroupBox
            {
                Text = $"{dayName} ({date:MMM dd})",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size((_dayContainer.Width - 30) / 7, 480),
                BackColor = Color.White
            };

            FlowLayoutPanel mealList = new FlowLayoutPanel
            {
                Location = new Point(10, 20),
                Size = new Size(group.Width - 20, group.Height - 30),
                FlowDirection = FlowDirection.TopDown,
                AutoScroll = true,
                BackColor = Color.White
            };

            foreach (string mealType in _mealTypes)
            {
                var mealsForCell = _cachedMealData.AsEnumerable()
                    .Where(row => row.Field<DateTime>("plan_date").Date == date.Date &&
                                 row.Field<string>("meal_type") == mealType)
                    .ToArray();

                Label mealTypeLabel = new Label
                {
                    Text = mealType,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Size = new Size(mealList.Width - 20, 20),
                    TextAlign = ContentAlignment.MiddleLeft,
                    BackColor = Color.FromArgb(233, 236, 239)
                };
                mealList.Controls.Add(mealTypeLabel);

                if (mealsForCell.Length == 0)
                {
                    Label emptyLabel = new Label
                    {
                        Text = "Double-click to add meal",
                        Font = new Font("Segoe UI", 8, FontStyle.Italic),
                        ForeColor = Color.FromArgb(150, 150, 150),
                        Size = new Size(mealList.Width - 20, 20),
                        TextAlign = ContentAlignment.MiddleLeft,
                        Cursor = Cursors.Hand,
                        Tag = new { Date = date, MealType = mealType }
                    };
                    emptyLabel.DoubleClick += (s, e) => AddMealToDate(date, mealType);
                    mealList.Controls.Add(emptyLabel);
                }
                else
                {
                    foreach (var meal in mealsForCell)
                    {
                        int mealPlanId = meal.Field<int>("meal_plan_id");
                        string mealText = meal.Field<string>("recipe_title") ?? meal.Field<string>("custom_meal_name");
                        string displayText = TruncateText(mealText, 20);

                        Label mealLabel = new Label
                        {
                            Text = displayText,
                            Font = new Font("Segoe UI", 8),
                            Size = new Size(mealList.Width - 20, 20),
                            TextAlign = ContentAlignment.MiddleLeft,
                            BackColor = Color.FromArgb(220, 248, 198),
                            Cursor = Cursors.Hand,
                            Tag = mealPlanId
                        };

                        ToolTip mealToolTip = new ToolTip();
                        mealToolTip.SetToolTip(mealLabel, $"{mealText}\nDouble-click to edit");

                        mealLabel.DoubleClick += (s, e) => ViewMealDetails(mealPlanId);
                        mealLabel.MouseEnter += (s, e) => mealLabel.BackColor = Color.FromArgb(200, 230, 180);
                        mealLabel.MouseLeave += (s, e) => mealLabel.BackColor = Color.FromArgb(220, 248, 198);

                        mealList.Controls.Add(mealLabel);
                    }
                }
            }

            group.Controls.Add(mealList);
            return group;
        }

        private string TruncateText(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - 3) + "...";
        }

        private void AddMealToDate(DateTime date, string mealType)
        {
            using (AddMealForm addMealForm = new AddMealForm(_dbManager, date, mealType))
            {
                if (addMealForm.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (addMealForm.SelectedRecipeId.HasValue)
                        {
                            _dbManager.AddMealPlan(_currentUserId, addMealForm.SelectedRecipeId.Value,
                                date, mealType, null, null);
                        }
                        else if (!string.IsNullOrEmpty(addMealForm.CustomMealName))
                        {
                            _dbManager.AddMealPlan(_currentUserId, null, date, mealType,
                                addMealForm.CustomMealName, addMealForm.Notes);
                        }

                        LoadWeekView();
                        MessageBox.Show("Meal added successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error adding meal: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ViewMealDetails(int mealPlanId)
        {
            using (AddMealForm detailForm = new AddMealForm(_dbManager, mealPlanId, _currentUserId))
            {
                if (detailForm.ShowDialog() == DialogResult.OK)
                {
                    LoadWeekView();
                }
            }
        }

        private DateTime GetMondayOfWeek(DateTime date)
        {
            int daysFromMonday = ((int)date.DayOfWeek - 1 + 7) % 7;
            return date.AddDays(-daysFromMonday).Date;
        }

        private void PrevWeekButton_Click(object sender, EventArgs e)
        {
            _currentWeekStart = _currentWeekStart.AddDays(-7);
            LoadWeekView();
            _datePicker.Value = _currentWeekStart;
        }

        private void NextWeekButton_Click(object sender, EventArgs e)
        {
            _currentWeekStart = _currentWeekStart.AddDays(7);
            LoadWeekView();
            _datePicker.Value = _currentWeekStart;
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
            _currentWeekStart = GetMondayOfWeek(_datePicker.Value);
            LoadWeekView();
        }

        private void AddMealButton_Click(object sender, EventArgs e)
        {
            using (var quickAddForm = new Form())
            {
                quickAddForm.Text = "Quick Add Meal";
                quickAddForm.Size = new Size(300, 200);
                quickAddForm.StartPosition = FormStartPosition.CenterParent;
                quickAddForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                quickAddForm.MaximizeBox = false;
                quickAddForm.MinimizeBox = false;

                var dateLabel = new Label { Text = "Date:", Location = new Point(20, 20), Size = new Size(50, 20) };
                var datePicker = new DateTimePicker { Location = new Point(80, 18), Size = new Size(150, 25), Value = DateTime.Now };

                var mealLabel = new Label { Text = "Meal:", Location = new Point(20, 60), Size = new Size(50, 20) };
                var mealCombo = new ComboBox
                {
                    Location = new Point(80, 58),
                    Size = new Size(150, 25),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                mealCombo.Items.AddRange(_mealTypes);
                mealCombo.SelectedIndex = 0;

                var addBtn = new Button
                {
                    Text = "Add",
                    Location = new Point(80, 120),
                    Size = new Size(70, 30),
                    DialogResult = DialogResult.OK
                };
                var cancelBtn = new Button
                {
                    Text = "Cancel",
                    Location = new Point(160, 120),
                    Size = new Size(70, 30),
                    DialogResult = DialogResult.Cancel
                };

                quickAddForm.Controls.AddRange(new Control[] { dateLabel, datePicker, mealLabel, mealCombo, addBtn, cancelBtn });

                if (quickAddForm.ShowDialog() == DialogResult.OK)
                {
                    AddMealToDate(datePicker.Value.Date, mealCombo.SelectedItem.ToString());
                }
            }
        }

        public void RefreshView()
        {
            if (_mealPlannerPanel != null && _mealPlannerPanel.Visible)
            {
                LoadWeekView();
            }
        }

        public void ShowMealPlannerPanel()
        {
            if (_mealPlannerPanel != null)
            {
                _mealPlannerPanel.Visible = true;
                RefreshView();
            }
        }

        public void HideMealPlannerPanel()
        {
            if (_mealPlannerPanel != null)
                _mealPlannerPanel.Visible = false;
        }

        public bool IsVisible()
        {
            return _mealPlannerPanel != null && _mealPlannerPanel.Visible;
        }
    }
}