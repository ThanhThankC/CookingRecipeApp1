-- Tạo cơ sở dữ liệu (nếu chưa có)
-- Drop database nếu tồn tại để tạo mới sạch sẽ
-- DROP DATABASE IF EXISTS recipes_db;
CREATE DATABASE recipes_db;
USE recipes_db;

select * FROM users

-- Bảng users (người dùng)
CREATE TABLE users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL, -- Lưu mật khẩu đã mã hóa
    email VARCHAR(100) UNIQUE,
    role ENUM('guest', 'registered', 'admin') DEFAULT 'guest',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Bảng meal_types (các kiểu bữa ăn: bữa sáng, bữa trưa, v.v.)
CREATE TABLE meal_types (
    meal_type_id INT AUTO_INCREMENT PRIMARY KEY,
    name ENUM('breakfast', 'lunch', 'dinner', 'snack', 'dessert', 'other') NOT NULL UNIQUE,
    description TEXT
);

-- Bảng recipes (công thức)
CREATE TABLE recipes (
    recipe_id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(100) NOT NULL,
    image VARCHAR(255), -- Lưu tên file hình ảnh (ví dụ: "images/pancakes.png")
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Bảng recipe_meal_types (quan hệ nhiều-nhiều: công thức - kiểu bữa ăn)
CREATE TABLE recipe_meal_types (
    recipe_id INT NOT NULL,
    meal_type_id INT NOT NULL,
    PRIMARY KEY (recipe_id, meal_type_id),
    FOREIGN KEY (recipe_id) REFERENCES recipes(recipe_id) ON DELETE CASCADE,
    FOREIGN KEY (meal_type_id) REFERENCES meal_types(meal_type_id) ON DELETE CASCADE
);

-- Bảng tips (lưu ý hoặc mẹo, liên kết với công thức)
CREATE TABLE tips (
    tip_id INT AUTO_INCREMENT PRIMARY KEY,
    recipe_id INT NOT NULL,
    description TEXT NOT NULL, -- Nội dung lưu ý/mẹo (e.g., "Nên dùng lửa nhỏ để tránh cháy")
    FOREIGN KEY (recipe_id) REFERENCES recipes(recipe_id) ON DELETE CASCADE
);

-- Bảng ingredients (nguyên liệu)
CREATE TABLE ingredients (
    ingredient_id INT AUTO_INCREMENT PRIMARY KEY,
    recipe_id INT NOT NULL,
    name VARCHAR(100) NOT NULL,
    quantity VARCHAR(50), -- Ví dụ: "2 cups", "500g"
    FOREIGN KEY (recipe_id) REFERENCES recipes(recipe_id) ON DELETE CASCADE
);

-- Bảng steps (các bước thực hiện)
CREATE TABLE steps (
    step_id INT AUTO_INCREMENT PRIMARY KEY,
    recipe_id INT NOT NULL,
    description TEXT NOT NULL,
    step_order INT NOT NULL,
    FOREIGN KEY (recipe_id) REFERENCES recipes(recipe_id) ON DELETE CASCADE
);

-- Bảng ratings (đánh giá)
CREATE TABLE ratings (
    rating_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    recipe_id INT NOT NULL,
    rating INT CHECK (rating >= 1 AND rating <= 5), -- Đánh giá từ 1-5
    is_favorite BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (recipe_id) REFERENCES recipes(recipe_id) ON DELETE CASCADE
);

-- Bảng shopping_lists (danh sách mua sắm)
CREATE TABLE shopping_lists (
    list_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    ingredient_name VARCHAR(100) NOT NULL,
    quantity VARCHAR(50),
    is_purchased BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- Bảng meal_plans (kế hoạch bữa ăn)
CREATE TABLE meal_plans (
    plan_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    recipe_id INT NOT NULL,
    plan_date DATE NOT NULL,
    meal_type ENUM('breakfast', 'lunch', 'dinner', 'snack') NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (recipe_id) REFERENCES recipes(recipe_id) ON DELETE CASCADE
);

-- Bảng recently_viewed (lịch sử xem gần đây)
CREATE TABLE recently_viewed (
    view_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    recipe_id INT NOT NULL,
    viewed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE,
    FOREIGN KEY (recipe_id) REFERENCES recipes(recipe_id) ON DELETE CASCADE
);

-- Bảng user_settings (tùy chỉnh ngôn ngữ và chủ đề)
CREATE TABLE user_settings (
    setting_id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    language ENUM('en', 'vi') DEFAULT 'en', -- Ví dụ: English/Vietnamese
    theme ENUM('light', 'dark', 'other') DEFAULT 'light',
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES users(user_id) ON DELETE CASCADE
);

-- Thêm dữ liệu (insert đầy đủ hơn)
-- Thêm người dùng (thêm thêm vài người dùng để đầy đủ)
INSERT INTO users (username, password, email, role) VALUES
('user01', '12345', 'guest@example.com', 'guest'),
('thanh', '12345', 'reg1@example.com', 'registered'),
('vietanh', '12345', 'reg2@example.com', 'registered'),
('admin_user', 'admin123', 'admin@example.com', 'admin'),
('test_user', 'test789', 'test@example.com', 'registered');

-- Thêm kiểu bữa ăn (meal_types)
INSERT INTO meal_types (name, description) VALUES
('breakfast', 'Bữa sáng'),
('lunch', 'Bữa trưa'),
('dinner', 'Bữa tối'),
('snack', 'Bữa xế'),
('dessert', 'Tráng miệng'),
('other', 'Khác');

-- Thêm công thức (thêm thêm vài công thức để đầy đủ)
INSERT INTO recipes (title, image) VALUES
('Chocolate Cake', 'image1.png'),
('Pancakes', 'image2.png'),
('Spaghetti Bolognese', 'image3.png'),
('Chicken Salad', 'image4.png'),
('Vegetable Stir Fry', 'image5.png'),
('Banana Smoothie', 'image6.png');

-- Liên kết công thức với kiểu bữa ăn (recipe_meal_types)
INSERT INTO recipe_meal_types (recipe_id, meal_type_id) VALUES
(1, 5), -- Chocolate Cake -> Dessert
(2, 1), -- Pancakes -> Breakfast
(3, 3), -- Spaghetti -> Dinner
(4, 2), -- Chicken Salad -> Lunch
(5, 4), -- Vegetable Stir Fry -> Snack
(6, 1); -- Banana Smoothie -> Breakfast

-- Thêm lưu ý/mẹo (tips)
INSERT INTO tips (recipe_id, description) VALUES
(1, 'Sử dụng chocolate chất lượng cao để bánh ngon hơn.'),
(1, 'Nướng ở nhiệt độ 180°C để tránh cháy.'),
(2, 'Thêm vani để tăng hương vị.'),
(2, 'Sử dụng chảo chống dính.'),
(3, 'Nấu sốt bò băm chậm để ngấm gia vị.'),
(4, 'Ướp gà trước 30 phút.'),
(5, 'Cắt rau củ đều tay để chín đều.'),
(6, 'Thêm đá bào để smoothie mát lạnh.');

-- Thêm nguyên liệu (thêm đầy đủ cho từng công thức)
INSERT INTO ingredients (recipe_id, name, quantity) VALUES
(1, 'Flour', '2 cups'), (1, 'Cocoa Powder', '1 cup'), (1, 'Sugar', '1.5 cups'), (1, 'Eggs', '3'), (1, 'Milk', '1 cup'),
(2, 'Flour', '1.5 cups'), (2, 'Milk', '1 cup'), (2, 'Eggs', '2'), (2, 'Butter', '2 tbsp'), (2, 'Baking Powder', '1 tsp'),
(3, 'Pasta', '300g'), (3, 'Ground Beef', '200g'), (3, 'Tomato Sauce', '150g'), (3, 'Onion', '1'), (3, 'Garlic', '2 cloves'),
(4, 'Chicken Breast', '200g'), (4, 'Lettuce', '100g'), (4, 'Tomato', '2'), (4, 'Cucumber', '1'), (4, 'Dressing', '50ml'),
(5, 'Broccoli', '100g'), (5, 'Carrot', '2'), (5, 'Bell Pepper', '1'), (5, 'Soy Sauce', '2 tbsp'), (5, 'Oil', '1 tbsp'),
(6, 'Banana', '2'), (6, 'Milk', '1 cup'), (6, 'Yogurt', '0.5 cup'), (6, 'Honey', '1 tbsp'), (6, 'Ice', '5 cubes');

-- Thêm bước thực hiện (thêm đầy đủ cho từng công thức)
INSERT INTO steps (recipe_id, description, step_order) VALUES
(1, 'Mix dry ingredients: flour, cocoa, sugar.', 1), (1, 'Add eggs and milk, stir well.', 2), (1, 'Bake at 180°C for 30 minutes.', 3), (1, 'Cool and frost.', 4),
(2, 'Mix flour, milk, eggs, butter.', 1), (2, 'Heat pan and pour batter.', 2), (2, 'Flip when bubbles form.', 3), (2, 'Serve with syrup.', 4),
(3, 'Boil pasta in salted water.', 1), (3, 'Sauté onion and garlic.', 2), (3, 'Add ground beef and cook.', 3), (3, 'Mix with tomato sauce and pasta.', 4),
(4, 'Grill chicken breast.', 1), (4, 'Chop vegetables.', 2), (4, 'Mix with dressing.', 3), (4, 'Serve chilled.', 4),
(5, 'Heat oil in wok.', 1), (5, 'Add vegetables and stir.', 2), (5, 'Add soy sauce.', 3), (5, 'Cook for 5 minutes.', 4),
(6, 'Peel bananas.', 1), (6, 'Blend with milk, yogurt, honey.', 2), (6, 'Add ice and blend again.', 3), (6, 'Pour and serve.', 4);

-- Thêm đánh giá (ratings)
INSERT INTO ratings (user_id, recipe_id, rating, is_favorite) VALUES
(1, 1, 4, TRUE), (2, 1, 5, FALSE), (3, 2, 5, TRUE), (4, 3, 4, FALSE), (5, 4, 3, TRUE), (1, 5, 5, FALSE), (2, 6, 4, TRUE);

-- Thêm danh sách mua sắm (shopping_lists)
INSERT INTO shopping_lists (user_id, ingredient_name, quantity) VALUES
(1, 'Flour', '2 cups'), (1, 'Cocoa Powder', '1 cup'), (2, 'Pasta', '300g'), (3, 'Chicken Breast', '200g'), (4, 'Broccoli', '100g'), (5, 'Banana', '2');

-- Thêm kế hoạch bữa ăn (meal_plans)
INSERT INTO meal_plans (user_id, recipe_id, plan_date, meal_type) VALUES
(1, 1, '2025-08-30', 'snack'),    -- Ngày hiện tại, dùng snack thay dessert
(1, 2, '2025-08-31', 'breakfast'), -- Ngày mai
(2, 3, '2025-09-01', 'dinner'),   -- Tiếp theo
(3, 4, '2025-09-02', 'lunch'),    -- Tiếp theo
(4, 5, '2025-09-03', 'snack'),    -- Tiếp theo
(5, 6, '2025-09-04', 'breakfast'), -- Tiếp theo
(2, 1, '2025-09-05', 'snack'),    -- Thêm người dùng khác
(3, 2, '2025-09-06', 'breakfast'), -- Thêm ngày khác
(4, 3, '2025-09-07', 'dinner');   -- Thêm ngày khác

-- Thêm lịch sử xem gần đây (recently_viewed)
INSERT INTO recently_viewed (user_id, recipe_id) VALUES
(1, 1), (1, 2), (2, 3), (3, 4), (4, 5), (5, 6);

-- Thêm tùy chỉnh người dùng (user_settings)
INSERT INTO user_settings (user_id, language, theme) VALUES
(1, 'en', 'light'), (2, 'vi', 'dark'), (3, 'en', 'other'), (4, 'vi', 'light'), (5, 'en', 'dark');