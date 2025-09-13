using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public class ShoppingListForm : Form
    {
        private readonly DatabaseManager _dbManager;
        private readonly int _userId;
        private CheckedListBox _shoppingListBox;

        public ShoppingListForm(DatabaseManager dbManager, int userId)
        {
            _dbManager = dbManager;
            _userId = userId;
            InitializeComponents();
            LoadShoppingList();
        }

        /// <summary>
        /// Initializes the components of the shopping list form with modern styling.
        /// </summary>
        private void InitializeComponents()
        {
            this.Text = "Shopping List";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 10);

            Label titleLabel = new Label
            {
                Text = "Danh Sách Mua Sắm (Từ Recipes Yêu Thích):",
                Location = new Point(20, 20),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(33, 37, 41)
            };
            this.Controls.Add(titleLabel);

            _shoppingListBox = new CheckedListBox
            {
                Location = new Point(20, 60),
                Size = new Size(360, 400),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };
            _shoppingListBox.ItemCheck += ShoppingListBox_ItemCheck;
            this.Controls.Add(_shoppingListBox);
        }

        /// <summary>
        /// Loads the shopping list items from DB.
        /// </summary>
        private void LoadShoppingList()
        {
            List<(string name, string quantity, bool completed)> items = _dbManager.LoadShoppingList(_userId);
            _shoppingListBox.Items.Clear();
            foreach (var item in items)
            {
                string displayText = $"{item.name} - {item.quantity}";
                _shoppingListBox.Items.Add(displayText, item.completed);
            }
        }

        /// <summary>
        /// Handles item check change to update completion status in DB.
        /// </summary>
        private void ShoppingListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked || e.NewValue == CheckState.Unchecked)
            {
                string itemText = _shoppingListBox.Items[e.Index].ToString();
                string[] parts = itemText.Split(new string[] { " - " }, StringSplitOptions.None);
                if (parts.Length >= 1)
                {
                    string ingredientName = parts[0];
                    bool completed = e.NewValue == CheckState.Checked;
                    _dbManager.UpdateShoppingItem(_userId, ingredientName, completed);
                }
            }
        }
    }
}