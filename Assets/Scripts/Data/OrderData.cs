using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class FoodPlacement
{
    public string foodType;        // VD: "Rice", "Salmon"
    public Vector2Int gridCoord;   // Tọa độ mục tiêu trong lưới (VD: 0,0 hoặc 2,1)
     public Vector2 gridValues;   // Tọa độ mục tiêu trong lưới (VD: 0,0 hoặc 2,1)
}
[CreateAssetMenu(fileName = "NewOrderData", menuName = "BentoJam/Order Data")]
public class OrderData : ScriptableObject 
{
    [Header("Order Info")]
    public string orderId;
    
    [Tooltip("Kích thước khay Bento (Cột x Hàng). VD: 2x3")]
    public Vector2Int targetGridSize = new Vector2Int(2, 3);

    [Header("Placement Strategy")]
    [Tooltip("Danh sách các món và vị trí cố định của chúng trong hộp")]
    public List<FoodPlacement> requiredLayout; 
    
    // Hàm tiện ích để lấy toàn bộ danh sách foodType (dùng cho logic kiểm tra cũ nếu cần)
    public List<string> GetRequiredFoodTypes()
    {
        List<string> types = new List<string>();
        foreach(var item in requiredLayout) types.Add(item.foodType);
        return types;
    }
}