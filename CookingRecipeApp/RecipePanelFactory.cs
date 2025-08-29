using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public static class RecipePanelFactory
    {
        public static Panel CreateRecipePanel(string imageUrl, string title, int recipeId)
        {
            Panel panel = new Panel
            {
                Width = 200,
                Height = 250,
                Margin = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            PictureBox pictureBox = new PictureBox
            {
                Size = new Size(180, 180),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };

            string projectPath = Application.StartupPath;
            string imagePath = Path.Combine(projectPath, imageUrl);

            if (File.Exists(imagePath))
            {
                pictureBox.Image = Image.FromFile(imagePath);
            }
            else
            {
                pictureBox.Image = null;
                MessageBox.Show($"File không tồn tại: {imagePath}");
            }
            panel.Controls.Add(pictureBox);

            Label lblTitle = new Label
            {
                Text = title,
                Location = new Point(10, 195),
                Size = new Size(180, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            panel.Controls.Add(lblTitle);

            panel.Click += (s, e) => { /* TODO: Mở form chi tiết với recipeId */ };

            return panel;
        }
    }
}