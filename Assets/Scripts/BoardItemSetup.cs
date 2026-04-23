using UnityEngine;

// Đánh dấu Serializable để hiển thị được trong Unity Inspector
[System.Serializable]
public class BoardItemSetup
{
    public int foodID; // Thêm biến này để Mapping
    [Tooltip("Kéo thả file FoodData (vd: Salmon_2x1) vào đây")]
    public FoodData foodAsset;

    public TypeTrayFood typeTrayFood;

    // [Tooltip("Tọa độ X, Y trên mặt bàn (Tầng 3)")]
    // public Vector2 position; 

    // [Tooltip("Lớp chồng (Layer). Ví dụ: 0 là dưới cùng, 1 đè lên 0, 2 đè lên 1...")]
    // public int layer;       
    [Header("Phân Tầng & Cột &Hàng")]
    [Tooltip("Đĩa thuộc cột nào (0, 1, 2...). Dùng để nhóm đĩa theo hàng ngang.")]
    public int rowId;
    [Tooltip("Đĩa thuộc cột nào (0, 1, 2...). Dùng để nhóm đĩa theo hàng dọc.")]
    public int columnId;

    [Tooltip("Lớp chồng. 0 là dưới cùng, 1 đè lên 0... Đĩa có Layer cao nhất trong cùng 1 Column sẽ được chọn.")]
    public int layer;

    [Tooltip("Vị trí gốc của đĩa này (Nếu muốn tùy chỉnh riêng lẻ, nếu không sẽ dùng Delta tự động)")]
    public Vector2 customOffset;

    public bool Finish;
    public bool isIce;
public bool isLid;
    public bool requireAd;
}