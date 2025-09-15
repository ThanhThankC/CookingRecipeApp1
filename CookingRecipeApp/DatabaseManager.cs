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

        /// <summary>
        /// Loads shopping list from shopping_lists table.
        /// </summary>
        public List<(string name, string quantity, bool purchased)> LoadShoppingList(int userId)
        {
            List<(string, string, bool)> items = new List<(string, string, bool)>();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT ingredient_name AS name, quantity, is_purchased AS purchased FROM shopping_lists WHERE user_id = @userId";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string name = reader["name"].ToString();
                        string quantity = reader["quantity"].ToString();
                        bool purchased = reader.GetBoolean("purchased");
                        items.Add((name, quantity, purchased));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading shopping list: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return items;
        }

        /// <summary>
        /// Adds a shopping item if not exists (or update if exists).
        /// </summary>
        public void AddShoppingItem(int userId, string name, string quantity)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO shopping_lists (user_id, ingredient_name, quantity, is_purchased) VALUES (@userId, @name, @quantity, FALSE) " +
                                   "ON DUPLICATE KEY UPDATE quantity = @quantity, is_purchased = FALSE";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@name", name);
                    command.Parameters.AddWithValue("@quantity", quantity);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding shopping item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Updates shopping item details (name, quantity, purchased status).
        /// </summary>
        public void UpdateShoppingItemDetails(int userId, string oldName, string newName, string quantity, bool purchased)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "UPDATE shopping_lists SET ingredient_name = @newName, quantity = @quantity, is_purchased = @purchased " +
                                   "WHERE user_id = @userId AND ingredient_name = @oldName";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@oldName", oldName);
                    command.Parameters.AddWithValue("@newName", newName);
                    command.Parameters.AddWithValue("@quantity", quantity);
                    command.Parameters.AddWithValue("@purchased", purchased);
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        // If name changed and no row updated, perhaps insert as new
                        AddShoppingItem(userId, newName, quantity);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating shopping item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Deletes a shopping item.
        /// </summary>
        public void DeleteShoppingItem(int userId, string name)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM shopping_lists WHERE user_id = @userId AND ingredient_name = @name";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.Parameters.AddWithValue("@name", name);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting shopping item: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Adds ingredients from favorited recipes to shopping list.
        /// </summary>
        public void AddIngredientsFromFavorites(int userId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                        INSERT IGNORE INTO shopping_lists (user_id, ingredient_name, quantity, is_purchased)
                        SELECT @userId, i.name, i.quantity, FALSE
                        FROM ingredients i
                        INNER JOIN recipes r ON i.recipe_id = r.recipe_id
                        INNER JOIN ratings f ON r.recipe_id = f.recipe_id
                        WHERE f.user_id = @userId AND f.is_favorite = TRUE AND r.is_active = TRUE";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@userId", userId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding ingredients from favorites: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        // Thêm những methods này vào class DatabaseManager

        /// <summary>
        /// Adds or updates a recipe in the recently viewed list for a user
        /// Uses INSERT ... ON DUPLICATE KEY UPDATE to handle unique constraint
        /// </summary>
        /// <param name="recipeId">ID of the recipe</param>
        /// <param name="userId">ID of the user</param>
        public void AddToRecentlyViewed(int recipeId, int userId)
        {
            if (userId <= 0 || recipeId <= 0) return;

            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // Use INSERT ... ON DUPLICATE KEY UPDATE to handle the unique constraint
                    // This will insert if not exists, or update the viewed_at timestamp if exists
                    string query = @"
                INSERT INTO recently_viewed (user_id, recipe_id, viewed_at) 
                VALUES (@userId, @recipeId, NOW())
                ON DUPLICATE KEY UPDATE viewed_at = NOW()";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        command.Parameters.AddWithValue("@recipeId", recipeId);
                        command.ExecuteNonQuery();
                    }

                    // Keep only the latest 10 records for this user to prevent table bloat
                    CleanupRecentlyViewed(userId, connection);
                }
            }
            catch (Exception ex)
            {
                // Log error but don't show to user as this is background functionality
                System.Diagnostics.Debug.WriteLine($"Error adding to recently viewed: {ex.Message}");
            }
        }

        /// <summary>
        /// Removes old entries from recently_viewed table, keeping only the latest 10 for each user
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="connection">Active database connection</param>
        private void CleanupRecentlyViewed(int userId, MySqlConnection connection)
        {
            try
            {
                string cleanupQuery = @"
            DELETE FROM recently_viewed 
            WHERE user_id = @userId 
            AND id NOT IN (
                SELECT id FROM (
                    SELECT id FROM recently_viewed 
                    WHERE user_id = @userId 
                    ORDER BY viewed_at DESC 
                    LIMIT 10
                ) AS recent_subset
            )";

                using (MySqlCommand command = new MySqlCommand(cleanupQuery, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cleaning up recently viewed: {ex.Message}");
            }
        }


        // Add these methods to your DatabaseManager.cs class

        /// <summary>
        /// Gets a specific meal plan by ID for editing purposes.
        /// </summary>
        /// <param name="mealPlanId">ID of the meal plan</param>
        /// <returns>DataTable containing meal plan details</returns>
        public DataTable GetMealPlanById(int mealPlanId)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                SELECT mp.plan_id, mp.user_id, mp.recipe_id, mp.plan_date, 
                       mp.meal_type, mp.custom_meal_name, r.title AS recipe_title
                FROM plans mp
                LEFT JOIN recipes r ON mp.recipe_id = r.recipe_id
                WHERE mp.plan_id = @mealPlanId";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@mealPlanId", mealPlanId);
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving meal plan: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dataTable;
        }

        public void UpdateMealPlan(int mealPlanId, int? recipeId, string customMealName, string notes)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                UPDATE meal_plans 
                SET recipe_id = @recipeId, custom_meal_name = @customMealName, notes = @notes, updated_at = NOW()
                WHERE plan_id = @mealPlanId";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@mealPlanId", mealPlanId);
                        command.Parameters.AddWithValue("@recipeId", (object)recipeId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@customMealName", (object)customMealName ?? DBNull.Value);
                        command.Parameters.AddWithValue("@notes", (object)notes ?? DBNull.Value);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            throw new Exception("Meal plan not found or could not be updated.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating meal plan: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DeleteMealPlan(int mealPlanId)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "DELETE FROM meal_plans WHERE plan_id = @mealPlanId";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@mealPlanId", mealPlanId);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            throw new Exception("Meal plan not found or could not be deleted.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting meal plan: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public DataTable GetMealPlanForWeek(int userId, DateTime weekStart)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                SELECT mp.plan_id, mp.plan_date, mp.meal_type, 
                       r.title AS recipe_title, mp.custom_meal_name
                FROM meal_plans mp
                LEFT JOIN recipes r ON mp.recipe_id = r.recipe_id
                WHERE mp.user_id = @userId 
                AND mp.plan_date BETWEEN @weekStart AND @weekEnd
                AND (r.is_active = TRUE OR r.recipe_id IS NULL)";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        command.Parameters.AddWithValue("@weekStart", weekStart.Date);
                        command.Parameters.AddWithValue("@weekEnd", weekStart.Date.AddDays(6));

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving meal plan: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dataTable;
        }

        public void AddMealPlan(int userId, int? recipeId, DateTime plannedDate, string mealType, string customMealName, string notes)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = @"
                INSERT INTO meal_plans (user_id, recipe_id, plan_date, meal_type, custom_meal_name, notes)
                VALUES (@userId, @recipeId, @plannedDate, @mealType, @customMealName, @notes)";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", userId);
                        command.Parameters.AddWithValue("@recipeId", (object)recipeId ?? DBNull.Value);
                        command.Parameters.AddWithValue("@plannedDate", plannedDate.Date);
                        command.Parameters.AddWithValue("@mealType", mealType);
                        command.Parameters.AddWithValue("@customMealName", (object)customMealName ?? DBNull.Value);
                        command.Parameters.AddWithValue("@notes", (object)notes ?? DBNull.Value);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding meal plan: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public DataTable GetAllActiveRecipes()
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT recipe_id, title FROM recipes WHERE is_active = TRUE";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving active recipes: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dataTable;
        }

        public DataTable GetAllRecipes()
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT recipe_id, title FROM recipes";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving all recipes: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dataTable;
        }

        public DataTable SearchActiveRecipes(string keyword)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT recipe_id, title FROM recipes WHERE title LIKE @keyword AND is_active = TRUE";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@keyword", "%" + keyword + "%");
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching recipes: {ex.Message}",
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return dataTable;
        }
    }
}
