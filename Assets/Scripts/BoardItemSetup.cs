using UnityEngine;

// Đánh dấu Serializable để hiển thị được trong Unity Inspector
[System.Serializable]
public class BoardItemSetup 
{
    [Tooltip("Kéo thả file FoodData (vd: Salmon_2x1) vào đây")]
    public FoodData foodAsset;
    
    // [Tooltip("Tọa độ X, Y trên mặt bàn (Tầng 3)")]
    // public Vector2 position; 
    
    // [Tooltip("Lớp chồng (Layer). Ví dụ: 0 là dưới cùng, 1 đè lên 0, 2 đè lên 1...")]
    // public int layer;        
}