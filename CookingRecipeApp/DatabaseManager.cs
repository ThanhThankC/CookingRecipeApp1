using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public class DatabaseManager
    {
        public readonly string _connectionString = "server=localhost;user id=root;password=1234;database=recipes_db;";

        public void LoadRecipes(FlowLayoutPanel recipeContainer, EventHandler clickHandler, string mealType)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"SELECT r.recipe_id, r.title, r.image, m.name AS meal_type
                                    FROM recipes r
                                    LEFT JOIN recipe_meal_types rm ON r.recipe_id = rm.recipe_id
                                    LEFT JOIN meal_types m ON rm.meal_type_id = m.meal_type_id
                                    WHERE r.is_active = TRUE";
                    if (!string.IsNullOrEmpty(mealType) && mealType != "All")
                    {
                        query += " AND m.name = @mealType";
                    }

                    MySqlCommand command = new MySqlCommand(query, connection);
                    if (!string.IsNullOrEmpty(mealType) && mealType != "All")
                    {
                        command.Parameters.AddWithValue("@mealType", mealType);
                    }

                    MySqlDataReader reader = command.ExecuteReader();
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);

                    recipeContainer.Controls.Clear();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        Panel recipePanel = RecipePanelFactory.CreateRecipePanel(
                            imageUrl: row["image"].ToString(),
                            title: row["title"].ToString(),
                            mealType: row["meal_type"].ToString(),
                            recipeId: (int)row["recipe_id"],
                            clickHandler: clickHandler
                        );
                        recipeContainer.Controls.Add(recipePanel);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recipe list: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SearchRecipes(string keyword, FlowLayoutPanel recipeContainer, EventHandler clickHandler, string mealType)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"SELECT r.recipe_id, r.title, r.image, m.name AS meal_type
                                    FROM recipes r
                                    LEFT JOIN recipe_meal_types rm ON r.recipe_id = rm.recipe_id
                                    LEFT JOIN meal_types m ON rm.meal_type_id = m.meal_type_id
                                    WHERE r.title LIKE @keyword AND r.is_active = TRUE";
                    if (!string.IsNullOrEmpty(mealType) && mealType != "All")
                    {
                        query += " AND m.name = @mealType";
                    }

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@keyword", "%" + keyword + "%");
                    if (!string.IsNullOrEmpty(mealType) && mealType != "All")
                    {
                        command.Parameters.AddWithValue("@mealType", mealType);
                    }

                    MySqlDataReader reader = command.ExecuteReader();
                    DataTable dataTable = new DataTable();
                    dataTable.Load(reader);

                    recipeContainer.Controls.Clear();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        Panel recipePanel = RecipePanelFactory.CreateRecipePanel(
                            imageUrl: row["image"].ToString(),
                            title: row["title"].ToString(),
                            mealType: row["meal_type"].ToString(),
                            recipeId: (int)row["recipe_id"],
                            clickHandler: clickHandler
                        );
                        recipeContainer.Controls.Add(recipePanel);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching recipes: {ex.Message}", "Search Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void GetRecipeSuggestions(string keyword, ListBox suggestionsListBox, string mealType)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"SELECT DISTINCT r.title
                                    FROM recipes r
                                    LEFT JOIN recipe_meal_types rm ON r.recipe_id = rm.recipe_id
                                    LEFT JOIN meal_types m ON rm.meal_type_id = m.meal_type_id
                                    WHERE r.title LIKE @keyword AND r.is_active = TRUE";
                    if (!string.IsNullOrEmpty(mealType) && mealType != "All")
                    {
                        query += " AND m.name = @mealType";
                    }
                    query += " LIMIT 10";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@keyword", "%" + keyword + "%");
                    if (!string.IsNullOrEmpty(mealType) && mealType != "All")
                    {
                        command.Parameters.AddWithValue("@mealType", mealType);
                    }

                    MySqlDataReader reader = command.ExecuteReader();
                    suggestionsListBox.Items.Clear();
                    while (reader.Read())
                    {
                        suggestionsListBox.Items.Add(reader["title"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting suggestions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void LoadMealTypes(ComboBox comboBox)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    MySqlCommand command = new MySqlCommand("SELECT name FROM meal_types ORDER BY meal_type_id", connection);
                    MySqlDataReader reader = command.ExecuteReader();
                    comboBox.Items.Clear();
                    comboBox.Items.Add("All");
                    while (reader.Read())
                    {
                        comboBox.Items.Add(reader["name"].ToString());
                    }
                    comboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading meal types: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AddRecipe(string title, string image, string mealType, List<(string name, string quantity)> ingredients, List<string> steps)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string recipeQuery = "INSERT INTO recipes (title, image, is_active) VALUES (@title, @image, TRUE); SELECT LAST_INSERT_ID();";
                            MySqlCommand recipeCmd = new MySqlCommand(recipeQuery, connection, transaction);
                            recipeCmd.Parameters.AddWithValue("@title", title);
                            recipeCmd.Parameters.AddWithValue("@image", image);
                            int recipeId = Convert.ToInt32(recipeCmd.ExecuteScalar());

                            if (!string.IsNullOrEmpty(mealType) && mealType != "All")
                            {
                                string mealQuery = "INSERT INTO meal_types (name) VALUES (@mealType); SELECT LAST_INSERT_ID();";
                                MySqlCommand mealCmd = new MySqlCommand(mealQuery, connection, transaction);
                                mealCmd.Parameters.AddWithValue("@mealType", mealType);
                                int mealTypeId = Convert.ToInt32(mealCmd.ExecuteScalar());

                                string rmQuery = "INSERT INTO recipe_meal_types (recipe_id, meal_type_id) VALUES (@recipeId, @mealTypeId)";
                                MySqlCommand rmCmd = new MySqlCommand(rmQuery, connection, transaction);
                                rmCmd.Parameters.AddWithValue("@recipeId", recipeId);
                                rmCmd.Parameters.AddWithValue("@mealTypeId", mealTypeId);
                                rmCmd.ExecuteNonQuery();
                            }

                            foreach (var ing in ingredients)
                            {
                                string ingQuery = "INSERT INTO ingredients (recipe_id, name, quantity) VALUES (@recipeId, @name, @quantity)";
                                MySqlCommand ingCmd = new MySqlCommand(ingQuery, connection, transaction);
                                ingCmd.Parameters.AddWithValue("@recipeId", recipeId);
                                ingCmd.Parameters.AddWithValue("@name", ing.name);
                                ingCmd.Parameters.AddWithValue("@quantity", ing.quantity);
                                ingCmd.ExecuteNonQuery();
                            }

                            for (int i = 0; i < steps.Count; i++)
                            {
                                string stepsQuery = "INSERT INTO steps (recipe_id, description, step_order) VALUES (@recipeId, @description, @order)";
                                MySqlCommand stepsCmd = new MySqlCommand(stepsQuery, connection, transaction);
                                stepsCmd.Parameters.AddWithValue("@recipeId", recipeId);
                                stepsCmd.Parameters.AddWithValue("@description", steps[i]);
                                stepsCmd.Parameters.AddWithValue("@order", i + 1);
                                stepsCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding recipe: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void UpdateRecipe(int recipeId, string title, string image, string mealType, List<(string name, string quantity)> ingredients, List<string> steps, string userRole)
        {
            if (userRole != "admin")
            {
                MessageBox.Show("Only admins can edit recipes.", "Permission Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string updateRecipeQuery = "UPDATE recipes SET title = @title, image = @image WHERE recipe_id = @recipeId AND is_active = TRUE";
                            MySqlCommand recipeCmd = new MySqlCommand(updateRecipeQuery, connection, transaction);
                            recipeCmd.Parameters.AddWithValue("@recipeId", recipeId);
                            recipeCmd.Parameters.AddWithValue("@title", title);
                            recipeCmd.Parameters.AddWithValue("@image", image);
                            int rowsAffected = recipeCmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                            {
                                throw new Exception("Recipe not found or is inactive.");
                            }

                            string deleteMealTypeQuery = "DELETE FROM recipe_meal_types WHERE recipe_id = @recipeId";
                            MySqlCommand deleteMealCmd = new MySqlCommand(deleteMealTypeQuery, connection, transaction);
                            deleteMealCmd.Parameters.AddWithValue("@recipeId", recipeId);
                            deleteMealCmd.ExecuteNonQuery();

                            if (!string.IsNullOrEmpty(mealType) && mealType != "All")
                            {
                                string mealQuery = "SELECT meal_type_id FROM meal_types WHERE name = @mealType";
                                MySqlCommand mealCmd = new MySqlCommand(mealQuery, connection, transaction);
                                mealCmd.Parameters.AddWithValue("@mealType", mealType);
                                object mealTypeIdObj = mealCmd.ExecuteScalar();

                                int mealTypeId;
                                if (mealTypeIdObj == null)
                                {
                                    string insertMealQuery = "INSERT INTO meal_types (name) VALUES (@mealType); SELECT LAST_INSERT_ID();";
                                    MySqlCommand insertMealCmd = new MySqlCommand(insertMealQuery, connection, transaction);
                                    insertMealCmd.Parameters.AddWithValue("@mealType", mealType);
                                    mealTypeId = Convert.ToInt32(insertMealCmd.ExecuteScalar());
                                }
                                else
                                {
                                    mealTypeId = Convert.ToInt32(mealTypeIdObj);
                                }

                                string rmQuery = "INSERT INTO recipe_meal_types (recipe_id, meal_type_id) VALUES (@recipeId, @mealTypeId)";
                                MySqlCommand rmCmd = new MySqlCommand(rmQuery, connection, transaction);
                                rmCmd.Parameters.AddWithValue("@recipeId", recipeId);
                                rmCmd.Parameters.AddWithValue("@mealTypeId", mealTypeId);
                                rmCmd.ExecuteNonQuery();
                            }

                            string deleteIngredientsQuery = "DELETE FROM ingredients WHERE recipe_id = @recipeId";
                            MySqlCommand deleteIngCmd = new MySqlCommand(deleteIngredientsQuery, connection, transaction);
                            deleteIngCmd.Parameters.AddWithValue("@recipeId", recipeId);
                            deleteIngCmd.ExecuteNonQuery();

                            foreach (var ing in ingredients)
                            {
                                string ingQuery = "INSERT INTO ingredients (recipe_id, name, quantity) VALUES (@recipeId, @name, @quantity)";
                                MySqlCommand ingCmd = new MySqlCommand(ingQuery, connection, transaction);
                                ingCmd.Parameters.AddWithValue("@recipeId", recipeId);
                                ingCmd.Parameters.AddWithValue("@name", ing.name);
                                ingCmd.Parameters.AddWithValue("@quantity", ing.quantity);
                                ingCmd.ExecuteNonQuery();
                            }

                            string deleteStepsQuery = "DELETE FROM steps WHERE recipe_id = @recipeId";
                            MySqlCommand deleteStepsCmd = new MySqlCommand(deleteStepsQuery, connection, transaction);
                            deleteStepsCmd.Parameters.AddWithValue("@recipeId", recipeId);
                            deleteStepsCmd.ExecuteNonQuery();

                            for (int i = 0; i < steps.Count; i++)
                            {
                                string stepsQuery = "INSERT INTO steps (recipe_id, description, step_order) VALUES (@recipeId, @description, @order)";
                                MySqlCommand stepsCmd = new MySqlCommand(stepsQuery, connection, transaction);
                                stepsCmd.Parameters.AddWithValue("@recipeId", recipeId);
                                stepsCmd.Parameters.AddWithValue("@description", steps[i]);
                                stepsCmd.Parameters.AddWithValue("@order", i + 1);
                                stepsCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating recipe: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DeleteRecipe(int recipeId, string userRole)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            if (userRole == "admin")
                            {
                                string deleteSteps = "DELETE FROM steps WHERE recipe_id = @recipeId";
                                MySqlCommand stepsCmd = new MySqlCommand(deleteSteps, connection, transaction);
                                stepsCmd.Parameters.AddWithValue("@recipeId", recipeId);
                                stepsCmd.ExecuteNonQuery();

                                string deleteIng = "DELETE FROM ingredients WHERE recipe_id = @recipeId";
                                MySqlCommand ingCmd = new MySqlCommand(deleteIng, connection, transaction);
                                ingCmd.Parameters.AddWithValue("@recipeId", recipeId);
                                ingCmd.ExecuteNonQuery();

                                string deleteRm = "DELETE FROM recipe_meal_types WHERE recipe_id = @recipeId";
                                MySqlCommand rmCmd = new MySqlCommand(deleteRm, connection, transaction);
                                rmCmd.Parameters.AddWithValue("@recipeId", recipeId);
                                rmCmd.ExecuteNonQuery();

                                string deleteRecipe = "DELETE FROM recipes WHERE recipe_id = @recipeId";
                                MySqlCommand recipeCmd = new MySqlCommand(deleteRecipe, connection, transaction);
                                recipeCmd.Parameters.AddWithValue("@recipeId", recipeId);
                                recipeCmd.ExecuteNonQuery();
                            }
                            else
                            {
                                string inactiveQuery = "UPDATE recipes SET is_active = FALSE WHERE recipe_id = @recipeId";
                                MySqlCommand inactiveCmd = new MySqlCommand(inactiveQuery, connection, transaction);
                                inactiveCmd.Parameters.AddWithValue("@recipeId", recipeId);
                                inactiveCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting recipe: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public bool IsRecipeFavorited(int recipeId, int userId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT COUNT(*) FROM ratings WHERE user_id = @userId AND recipe_id = @recipeId AND is_favorite = TRUE";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@recipeId", recipeId);
                    long count = (long)command.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error checking favorite: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void AddFavorite(int recipeId, int userId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO ratings (user_id, recipe_id, is_favorite) VALUES (@userId, @recipeId, TRUE) " +
                                  "ON DUPLICATE KEY UPDATE is_favorite = TRUE";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@recipeId", recipeId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding favorite: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void RemoveFavorite(int recipeId, int userId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "UPDATE ratings SET is_favorite = FALSE WHERE user_id = @userId AND recipe_id = @recipeId";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@recipeId", recipeId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error removing favorite: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public List<(string name, string quantity, bool completed)> LoadShoppingList(int userId)
        {
            List<(string, string, bool)> items = new List<(string, string, bool)>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                        SELECT DISTINCT i.name, i.quantity, COALESCE(s.is_purchased, 0) as completed
                        FROM ingredients i
                        INNER JOIN recipes r ON i.recipe_id = r.recipe_id
                        INNER JOIN ratings f ON r.recipe_id = f.recipe_id
                        LEFT JOIN shopping_lists s ON i.name = s.ingredient_name AND f.user_id = s.user_id
                        WHERE f.user_id = @userId AND f.is_favorite = TRUE AND r.is_active = TRUE";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string name = reader["name"].ToString();
                        string quantity = reader["quantity"].ToString();
                        bool completed = Convert.ToBoolean(reader["completed"]);
                        items.Add((name, quantity, completed));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading shopping list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return items;
        }

        public void UpdateShoppingItem(int userId, string ingredientName, bool completed)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlTransaction transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            string deleteQuery = "DELETE FROM shopping_lists WHERE user_id = @userId AND ingredient_name = @ingredientName";
                            MySqlCommand deleteCmd = new MySqlCommand(deleteQuery, connection, transaction);
                            deleteCmd.Parameters.AddWithValue("@userId", userId);
                            deleteCmd.Parameters.AddWithValue("@ingredientName", ingredientName);
                            deleteCmd.ExecuteNonQuery();

                            if (completed)
                            {
                                string insertQuery = "INSERT INTO shopping_lists (user_id, ingredient_name, is_purchased) VALUES (@userId, @ingredientName, 1)";
                                MySqlCommand insertCmd = new MySqlCommand(insertQuery, connection, transaction);
                                insertCmd.Parameters.AddWithValue("@userId", userId);
                                insertCmd.Parameters.AddWithValue("@ingredientName", ingredientName);
                                insertCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating shopping item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
