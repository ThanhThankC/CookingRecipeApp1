using MySql.Data.MySqlClient;
using System.Data;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public class DatabaseManager
    {
        private readonly string conString = "server=localhost;user id=root;password=1234;database=recipes_db;";

        public void GetData(FlowLayoutPanel flowLayoutPanelRecipes)
        {
            using (MySqlConnection con = new MySqlConnection(conString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT image_url AS HinhAnh, title AS TenCongThuc, recipe_id " +
                    "FROM recipes " +
                    "ORDER BY created_at DESC;", con);

                MySqlDataReader reader = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(reader);

                flowLayoutPanelRecipes.Controls.Clear();

                foreach (DataRow row in dt.Rows)
                {
                    Panel recipePanel = RecipePanelFactory.CreateRecipePanel(
                        row["HinhAnh"].ToString(),
                        row["TenCongThuc"].ToString(),
                        (int)row["recipe_id"]
                    );
                    flowLayoutPanelRecipes.Controls.Add(recipePanel);
                }
            }
        }

        public void SearchRecipes(string keyword, FlowLayoutPanel flowLayoutPanelRecipes)
        {
            using (MySqlConnection con = new MySqlConnection(conString))
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand(
                    "SELECT image_url AS HinhAnh, title AS TenCongThuc, recipe_id " +
                    "FROM recipes " +
                    "WHERE title LIKE @keyword " +
                    "ORDER BY created_at DESC;", con);
                cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                MySqlDataReader reader = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(reader);

                flowLayoutPanelRecipes.Controls.Clear();
                foreach (DataRow row in dt.Rows)
                {
                    Panel recipePanel = RecipePanelFactory.CreateRecipePanel(
                        row["HinhAnh"].ToString(),
                        row["TenCongThuc"].ToString(),
                        (int)row["recipe_id"]
                    );
                    flowLayoutPanelRecipes.Controls.Add(recipePanel);
                }
            }
        }
    }
}