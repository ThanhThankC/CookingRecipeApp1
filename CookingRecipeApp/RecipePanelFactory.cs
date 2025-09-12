using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace CookingRecipeApp
{
    public static class RecipePanelFactory
    {
        public static Panel CreateRecipePanel(string imageUrl, string title, string mealType, int recipeId, EventHandler clickHandler)
        {
            // Tạo panel hiển thị thông tin công thức với thiết kế hiện đại (flat, hover effect)
            Panel panel = new Panel
            {
                Width = 220,  // COMMENT: Tăng width cho spacing tốt hơn
                Height = 300,
                Margin = new Padding(15),  // COMMENT: Tăng margin cho grid layout mượt mà
                BorderStyle = BorderStyle.None,  // COMMENT: Flat design, no border
                BackColor = Color.FromArgb(245, 245, 245),  // COMMENT: Light gray background cho modern look
                Enabled = true,
                Tag = recipeId,
                Cursor = Cursors.Hand  // COMMENT: UX: Cursor hand để chỉ rõ clickable
            };

            // Thêm hover effect (change background on mouse enter/leave)
            panel.MouseEnter += (s, e) => panel.BackColor = Color.FromArgb(220, 220, 220);  // COMMENT: Hover: Darken slightly
            panel.MouseLeave += (s, e) => panel.BackColor = Color.FromArgb(245, 245, 245);

            // Tạo hình ảnh công thức
            PictureBox pictureBox = new PictureBox
            {
                Size = new Size(200, 200),  // COMMENT: Tăng size cho hình ảnh nổi bật hơn
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.None,  // COMMENT: Flat
                Tag = recipeId
            };

            string projectPath = Application.StartupPath;
            string relativePath = Path.Combine("..", "..", "..", "Images");
            string basePath = Path.GetFullPath(Path.Combine(projectPath, relativePath));
            string fullImagePath = Path.Combine(basePath, Path.GetFileName(imageUrl));

            try
            {
                if (File.Exists(fullImagePath))
                {
                    pictureBox.Image = Image.FromFile(fullImagePath);
                }
                else
                {
                    pictureBox.Image = null;  // COMMENT: Có thể thêm placeholder image default ở đây cho UX tốt hơn
                    MessageBox.Show($"Hình ảnh không tồn tại: {fullImagePath}. Vui lòng kiểm tra thư mục Images.", "Lỗi Hình Ảnh", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                pictureBox.Image = null;
                MessageBox.Show($"Lỗi khi tải hình ảnh: {ex.Message}", "Lỗi Hình Ảnh", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            panel.Controls.Add(pictureBox);

            // Tạo tiêu đề công thức
            Label titleLabel = new Label
            {
                Text = title,
                Location = new Point(10, 215), 
                Size = new Size(200, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11, FontStyle.Bold), 
                ForeColor = Color.FromArgb(33, 37, 41), 
                Tag = recipeId
            };
            panel.Controls.Add(titleLabel);

            // Tạo nhãn loại món ăn
            Label mealTypeLabel = new Label
            {
                Text = mealType,
                Location = new Point(10, 255),
                Size = new Size(200, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(108, 117, 125), 
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                Tag = recipeId
            };
            panel.Controls.Add(mealTypeLabel);

            // Gắn sự kiện click cho panel, hình ảnh và tiêu đề
            if (clickHandler != null)
            {
                panel.Click += clickHandler;
                pictureBox.Click += clickHandler;
                titleLabel.Click += clickHandler;
                mealTypeLabel.Click += clickHandler;
            }

            // COMMENT: UX: Thêm hover cho labels và picture để đồng bộ
            foreach (Control ctrl in panel.Controls)
            {
                ctrl.MouseEnter += (s, e) => panel.BackColor = Color.FromArgb(220, 220, 220);
                ctrl.MouseLeave += (s, e) => panel.BackColor = Color.FromArgb(245, 245, 245);
            }

            return panel;
        }
    }
}
