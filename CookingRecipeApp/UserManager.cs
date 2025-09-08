using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public class UserManager
    {
        private readonly string _connectionString = "server=localhost;user id=root;password=1234;database=recipes_db;";
        public bool IsLoggedIn { get; private set; }
        public int CurrentUserId { get; private set; }
        public string CurrentUsername { get; private set; }

        public bool Register(string username, string password)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // Check if username already exists
                    string checkQuery = "SELECT COUNT(*) FROM users WHERE username = @username";
                    MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@username", username);
                    long count = (long)checkCommand.ExecuteScalar();

                    if (count > 0)
                    {
                        MessageBox.Show("Tên người dùng đã tồn tại. Vui lòng chọn tên khác.", "Lỗi Đăng Ký", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }

                    // Lưu plain password (không hash)
                    string insertQuery = "INSERT INTO users (username, password) VALUES (@username, @password)";
                    MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@username", username);
                    insertCommand.Parameters.AddWithValue("@password", password);
                    insertCommand.ExecuteNonQuery();

                    MessageBox.Show("Đăng ký thành công! Vui lòng đăng nhập.", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đăng ký: {ex.Message}", "Lỗi Cơ Sở Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool Login(string username, string password)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    string query = "SELECT user_id, password FROM users WHERE username = @username";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@username", username);
                    MySqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        string storedPassword = reader["password"].ToString();
                        int userId = reader.GetInt32("user_id");

                        // So sánh plain password
                        if (password.Equals(storedPassword))
                        {
                            IsLoggedIn = true;
                            CurrentUserId = userId;
                            CurrentUsername = username;
                            reader.Close();
                            return true;
                        }
                    }

                    reader.Close();
                    MessageBox.Show("Tên người dùng hoặc mật khẩu không đúng.", "Lỗi Đăng Nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đăng nhập: {ex.Message}", "Lỗi Cơ Sở Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void Logout()
        {
            IsLoggedIn = false;
            CurrentUserId = 0;
            CurrentUsername = null;
        }
    }
}