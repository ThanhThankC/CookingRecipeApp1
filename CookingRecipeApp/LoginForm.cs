using System;
using System.Drawing;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public class LoginForm : Form
    {
        private readonly UserManager _userManager;
        private TextBox _usernameTextBox;
        private TextBox _passwordTextBox;
        private CheckBox _adminCheckBox; // Added: Checkbox for admin role
        private Button _loginButton;
        private Button _registerButton;
        private Label _registerLink;

        public LoginForm(UserManager userManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Login / Register";
            this.Size = new Size(400, 320); // Increased height to fit checkbox
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(248, 249, 250);
            this.Font = new Font("Segoe UI", 10);

            // Username Label
            Label usernameLabel = new Label
            {
                Text = "Username:",
                Location = new Point(50, 40),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.Add(usernameLabel);

            // Username TextBox
            _usernameTextBox = new TextBox
            {
                Location = new Point(180, 40),
                Size = new Size(180, 30),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };
            this.Controls.Add(_usernameTextBox);

            // Password Label
            Label passwordLabel = new Label
            {
                Text = "Password:",
                Location = new Point(50, 80),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.Add(passwordLabel);

            // Password TextBox
            _passwordTextBox = new TextBox
            {
                Location = new Point(180, 80),
                Size = new Size(180, 30),
                UseSystemPasswordChar = true,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
                BackColor = Color.White
            };
            this.Controls.Add(_passwordTextBox);

            // Admin Checkbox (visible only in register mode)
            _adminCheckBox = new CheckBox
            {
                Text = "Register as Admin",
                Location = new Point(180, 120),
                Size = new Size(180, 30),
                Font = new Font("Segoe UI", 10),
                Visible = false // Hidden in login mode
            };
            this.Controls.Add(_adminCheckBox);

            // Login Button
            _loginButton = new Button
            {
                Text = "Login",
                Location = new Point(180, 160),
                Size = new Size(180, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            _loginButton.Click += LoginButton_Click;
            _loginButton.MouseEnter += (s, e) => _loginButton.BackColor = Color.FromArgb(0, 105, 217);
            _loginButton.MouseLeave += (s, e) => _loginButton.BackColor = Color.FromArgb(0, 123, 255);
            this.Controls.Add(_loginButton);

            // Register Button
            _registerButton = new Button
            {
                Text = "Register",
                Location = new Point(180, 160),
                Size = new Size(180, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10),
                Visible = false
            };
            _registerButton.Click += RegisterButton_Click;
            _registerButton.MouseEnter += (s, e) => _registerButton.BackColor = Color.FromArgb(0, 105, 217);
            _registerButton.MouseLeave += (s, e) => _registerButton.BackColor = Color.FromArgb(0, 123, 255);
            this.Controls.Add(_registerButton);

            // Register Link
            _registerLink = new Label
            {
                Text = "Don't have an account? Sign up here.",
                Location = new Point(180, 200),
                Size = new Size(180, 20),
                ForeColor = Color.FromArgb(0, 123, 255),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9, FontStyle.Underline)
            };
            _registerLink.Click += RegisterLink_Click;
            this.Controls.Add(_registerLink);
        }

        private void LoginButton_Click(object sender, EventArgs e)
        {
            string username = _usernameTextBox.Text.Trim();
            string password = _passwordTextBox.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_userManager.Login(username, password))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void RegisterButton_Click(object sender, EventArgs e)
        {
            string username = _usernameTextBox.Text.Trim();
            string password = _passwordTextBox.Text;
            bool isAdmin = _adminCheckBox.Checked;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_userManager.Register(username, password, isAdmin))
            {
                _usernameTextBox.Text = "";
                _passwordTextBox.Text = "";
                _adminCheckBox.Checked = false;
                // Switch back to login mode
                _loginButton.Visible = true;
                _registerButton.Visible = false;
                _adminCheckBox.Visible = false;
                _registerLink.Text = "Don't have an account? Sign up here.";
            }
        }

        private void RegisterLink_Click(object sender, EventArgs e)
        {
            if (_registerLink.Text.Contains("Sign up"))
            {
                _loginButton.Visible = false;
                _registerButton.Visible = true;
                _adminCheckBox.Visible = true; // Show checkbox in register mode
                _registerLink.Text = "Already have an account? Log in here.";
            }
            else
            {
                _loginButton.Visible = true;
                _registerButton.Visible = false;
                _adminCheckBox.Visible = false; // Hide checkbox in login mode
                _registerLink.Text = "Don't have an account? Sign up here.";
            }
        }
    }
}