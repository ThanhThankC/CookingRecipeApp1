using System;
using System.Drawing;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public class UIManager
    {
        private readonly Form1 form;
        private Panel leftPanel;
        private TextBox txtSearch;
        private Button btnNewRecipe;
        private Button btnHome;
        private Button btnShoppingList;
        private Button btnMealPlanner;
        private Button btnCookbooks;

        private Panel mainPanel;
        private FlowLayoutPanel flowLayoutPanelRecipes;

        public TextBox TxtSearch => txtSearch;
        public FlowLayoutPanel FlowLayoutPanelRecipes => flowLayoutPanelRecipes;

        public UIManager(Form1 form)
        {
            this.form = form ?? throw new ArgumentNullException(nameof(form));
        }

        public void SetupLayout()
        {
            SetupLeftPanel();
            SetupMainPanel();
            form.Resize += Form1_Resize;
        }

        private void SetupLeftPanel()
        {
            leftPanel = new Panel
            {
                Name = "LeftPanel",
                Width = 300,
                Height = form.ClientSize.Height,
                Location = new Point(0, 0),
                BackColor = Color.LightGray,
                Dock = DockStyle.Left
            };
            form.Controls.Add(leftPanel);

            txtSearch = new TextBox
            {
                Name = "txtSearch",
                Text = "Search recipes...",
                Width = 220,
                Height = 30,
                Location = new Point(40, 60),
                BackColor = Color.White,
                ForeColor = Color.Gray,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 11, FontStyle.Regular)
            };

            txtSearch.GotFocus += (s, e) =>
            {
                if (txtSearch.Text == "Search recipes...")
                {
                    txtSearch.Text = "";
                    txtSearch.ForeColor = Color.Black;
                }
            };
            txtSearch.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtSearch.Text))
                {
                    txtSearch.Text = "Search recipes...";
                    txtSearch.ForeColor = Color.Gray;
                }
            };
            txtSearch.TextChanged += form.txtSearch_TextChanged;

            leftPanel.Controls.Add(txtSearch);

            btnNewRecipe = CreateMenuButton("New recipe", 120, form.btnNewRecipe_Click);
            btnHome = CreateMenuButton("Home", 180, form.btnHome_Click);
            btnShoppingList = CreateMenuButton("Shopping list", 240, form.btnShoppingList_Click);
            btnMealPlanner = CreateMenuButton("Meal planner", 300, form.btnMealPlanner_Click);
            btnCookbooks = CreateMenuButton("Cookbooks", 360, form.btnCookbooks_Click);

            leftPanel.Controls.AddRange(new Control[]
            {
                btnNewRecipe, btnHome, btnShoppingList, btnMealPlanner,
                btnCookbooks,
            });
        }

        private Button CreateMenuButton(string text, int top, EventHandler clickHandler)
        {
            var button = new Button
            {
                Text = text,
                Width = 240,
                Height = 35,
                Location = new Point(30, top),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                BackColor = Color.White,
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleLeft
            };
            if (clickHandler != null)
            {
                button.Click += clickHandler;
            }
            return button;
        }

        private void SetupMainPanel()
        {
            mainPanel = new Panel
            {
                Name = "MainPanel",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(320, 20, 20, 20)
            };
            form.Controls.Add(mainPanel);

            flowLayoutPanelRecipes = new FlowLayoutPanel
            {
                Name = "flowLayoutPanelRecipes",
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true
            };
            mainPanel.Controls.Add(flowLayoutPanelRecipes);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (form.ClientSize.Width < 1000)
            {
                leftPanel.Width = 50;
                txtSearch.Visible = false;
                mainPanel.Padding = new Padding(70, 20, 20, 20);
            }
            else
            {
                leftPanel.Width = 300;
                txtSearch.Visible = true;
                mainPanel.Padding = new Padding(320, 20, 20, 20);
            }
        }
    }
}