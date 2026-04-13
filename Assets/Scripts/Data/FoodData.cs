using UnityEngine;
using System.Collections.Generic;

// Dòng này giúp bạn click chuột phải trong project: Create -> BentoJam -> Food Data
[CreateAssetMenu(fileName = "NewFoodData", menuName = "BentoJam/FoodData")]
public class FoodData : ScriptableObject 
{
    [Header("Basic Info")]
    public string id;               // VD: "salmon_2x1"
    public string foodType;         // VD: "Salmon", "Rice", "Shrimp"
    public Sprite icon;             // Hình ảnh 2D hiển thị

    [Header("Shape Definition")]
    [Tooltip("Danh sách các ô bị chiếm chỗ. Gốc tọa độ (0,0) là góc trên-trái của mảnh ghép.")]
    public List<Vector2Int> shapeBlocks; 
    
    // Ví dụ cách thiết lập shapeBlocks trên Inspector:
    // Mảnh 1x2 (ngang): (0,0), (1,0)
    // Mảnh chữ L:      (0,0), (0,1), (1,1)
    
    // (Tùy chọn) Kích thước tổng thể của mảnh ghép để tính toán bound (khung bao)
    public int width = 1;
    public int height = 1;
    public bool isBuff; //BOM
}
