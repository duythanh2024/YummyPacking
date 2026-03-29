# YummyPacking
Yummy Packing
📦 Root (Assets / assets)
 ┣ 📂 Animations       # Chứa Animation clips/controllers (Mở hộp Bento, Thức ăn bay, Nổ hiệu ứng)
 ┣ 📂 Art              # Chứa toàn bộ hình ảnh, model
 ┃ ┣ 📂 Sprites        # UI, Icon đồ ăn (2D)
 ┃ ┣ 📂 Materials      # Vật liệu (nếu dùng 3D)
 ┃ ┗ 📂 Textures
 ┣ 📂 Audio            # Chứa âm thanh
 ┃ ┣ 📂 BGM            # Nhạc nền (Cozy, Chill)
 ┃ ┗ 📂 SFX            # Tiếng "Ting", tiếng nhặt đồ, tiếng hít (Snap) vào lưới
 ┣ 📂 Configs          # Chứa data cấu hình Level (JSON, CSV, hoặc ScriptableObjects)
 ┃ ┣ 📂 Levels         # Level_001.json, Level_002.json...
 ┃ ┗ 📂 Items          # Định nghĩa hình dáng (Shape) và ID của từng loại đồ ăn
 ┣ 📂 Prefabs          # Các đối tượng dựng sẵn
 ┃ ┣ 📂 Entities       # FoodTile, BentoGrid, OrderCard
 ┃ ┣ 📂 UI             # Các màn hình (MainMenu, Gameplay, WinPopup, LosePopup)
 ┃ ┗ 📂 VFX            # Hiệu ứng (Partices dọn sạch bàn, Pháo hoa)
 ┣ 📂 Scenes           # Loading, MainMenu, Gameplay
 ┗ 📂 Scripts          # Source Code chính
   ┣ 📂 Core           # Các Manager điều phối game (GameManager, LevelManager)
   ┣ 📂 Gameplay       # Logic vận hành chính
   ┃ ┣ 📂 PackLayer    # Quản lý thuật toán đè/nhặt ở Tầng 3
   ┃ ┣ 📂 GridSystem   # Thuật toán Auto-fit và check không gian lưới Bento
   ┃ ┗ 📂 OrderSystem  # Quản lý đơn hàng hiện/ẩn và Check Win
   ┣ 📂 Data           # Scripts parse file JSON, lưu Data người chơi (Save/Load)
   ┣ 📂 UI             # Code điều khiển các màn hình, nút bấm, Animation UI
   ┗ 📂 Utils          # Các công cụ hỗ trợ (ObjectPool, Timer, MathHelpers)
