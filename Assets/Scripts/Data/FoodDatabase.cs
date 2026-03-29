using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FoodDatabase", menuName = "Bento/Database")]
public class FoodDatabase : ScriptableObject
{
    public List<FoodData> allFoods;

    // Hàm quan trọng nhất: Truyền ID để lấy Hình ảnh
    public Sprite GetSpriteByID(string id)
    {
        FoodData data = allFoods.Find(f => f.id == id);
        if (data != null) return data.icon;
        
        Debug.LogError($"Không tìm thấy FoodID: {id}");
        return null;
    }

    public FoodData GetDataByID(string id)
    {
        return allFoods.Find(f => f.id == id);
    }
}