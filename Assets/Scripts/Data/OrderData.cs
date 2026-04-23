using UnityEngine;
using System.Collections.Generic;
using System;
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

    public TypeTray typeOfTray;//0: 2*3, 1: 3*3, 2: luc giac
                               // public int typeTray=0; //0: 2*3, 1: 3*3, 2: luc giac

    public TypeRewardSlot typeRewardSlot;
    public Sprite spriteSlot;

    //Slot Lock, Coin
    public int[] slotInTrays;

    [Header("Placement Strategy")]
    [Tooltip("Danh sách các món và vị trí cố định của chúng trong hộp")]
    public List<FoodPlacement> requiredLayout;

    // Hàm tiện ích để lấy toàn bộ danh sách foodType (dùng cho logic kiểm tra cũ nếu cần)
    public List<string> GetRequiredFoodTypes()
    {
        List<string> types = new List<string>();
        foreach (var item in requiredLayout) types.Add(item.foodType);
        return types;
    }
}
