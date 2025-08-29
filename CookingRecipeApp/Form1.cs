using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public partial class Form1 : Form
    {
        private readonly DatabaseManager dbManager; private readonly UIManager uiManager;

        public Form1()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            uiManager = new UIManager(this);
            uiManager.SetupLayout();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dbManager.GetData(uiManager.FlowLayoutPanelRecipes);
        }

        public void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = uiManager.TxtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(keyword) && keyword != "Search recipes...")
            {
                dbManager.SearchRecipes(keyword, uiManager.FlowLayoutPanelRecipes);
            }
            else
            {
                dbManager.GetData(uiManager.FlowLayoutPanelRecipes);
            }
        }

        // Menu Button Handlers
        public void btnNewRecipe_Click(object sender, EventArgs e) { }
        public void btnHome_Click(object sender, EventArgs e) { }
        public void btnShoppingList_Click(object sender, EventArgs e) { }
        public void btnMealPlanner_Click(object sender, EventArgs e) { }
        public void btnCookbooks_Click(object sender, EventArgs e) { }
    }

}