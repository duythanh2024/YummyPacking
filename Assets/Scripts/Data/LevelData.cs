using UnityEngine;

using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Level_001", menuName = "BentoJam/Level Data")]
public class LevelData : ScriptableObject 
{
    [Header("--- THÔNG SỐ CHUNG ---")]
    public int levelId;
    
    [Tooltip("Số ô trống tối đa của Khay chờ (Thường là 3, level khó giảm còn 2)")]
    public int bufferCapacity = 3;

    [Header("--- HÀNG ĐỢI ĐƠN HÀNG (TẦNG 1) ---")]
    [Tooltip("Danh sách các đơn hàng. Đơn ở vị trí 0 sẽ ra trước.")]
    public List<OrderData> orderQueue;

    public bool IsStack=false; // true co tang, false la ko co

    [Header("--- BỐ CỤC BÀN ĂN (TẦNG 3) ---")]
    [Tooltip("Danh sách tất cả các mảnh đồ ăn có trên bàn lúc bắt đầu")]
    public List<BoardItemSetup> boardItems;

    public int difficultLevel;

    // Hàm tiện ích: Tự động kiểm tra xem thiết kế level có bị lỗi thiếu/thừa đồ ăn không
    // (CTO rất thích những hàm Validate như thế này để tránh lỗi do Designer nhập sai)
   [Header("--- CẤU HÌNH HIỂN THỊ ---")]
    public Vector2 plateDeltaOffset = new Vector2(8f, -15f);

    // --- PHƯƠNG THỨC TIỆN ÍCH (CTO METHODS) ---

    /// <summary>
    /// Lấy danh sách các đĩa thuộc một cột cụ thể, đã sắp xếp theo tầng từ dưới lên trên.
    /// </summary>
    public List<BoardItemSetup> GetItemsInColumn(int colId)
    {
        return boardItems.FindAll(x => x.columnId == colId)
                         .OrderBy(x => x.layer)
                         .ToList();
    }

    /// <summary>
    /// Lấy đĩa trên cùng (đang cho phép chọn) của một cột.
    /// </summary>
    public BoardItemSetup GetTopItemInColumn(int colId)
    {
        var colItems = GetItemsInColumn(colId);
        return colItems.Count > 0 ? colItems.Last() : null;
    }

    /// <summary>
    /// Hàm Validate "huyền thoại" để Designer không nhập sai dữ liệu.
    /// </summary>
    private void OnValidate()
    {
        // if (orderQueue == null || boardItems == null) return;

        // // 1. Kiểm tra tổng số lượng đồ ăn
        // Dictionary<string, int> requiredFood = new Dictionary<string, int>();
        // foreach (var order in orderQueue)
        // {
        //     if (order == null) continue;
        //     foreach (var placement in order.requiredLayout)
        //     {
        //         if (placement.foodType == null) continue;
        //         string id = placement.foodType;
        //         requiredFood[id] = requiredFood.GetValueOrDefault(id) + 1;
        //     }
        // }

        // Dictionary<string, int> availableFood = new Dictionary<string, int>();
        // foreach (var item in boardItems)
        // {
        //     if (item.foodAsset == null) continue;
        //     string id = item.foodAsset.name;
        //     availableFood[id] = availableFood.GetValueOrDefault(id) + 1;
        // }

        // // 2. So sánh và cảnh báo
        // foreach (var pair in requiredFood)
        // {
        //     int available = availableFood.GetValueOrDefault(pair.Key);
        //     if (available < pair.Value)
        //     {
        //         Debug.LogError($"[Level {levelId}] THIẾU ĐỒ ĂN: Loại {pair.Key} cần {pair.Value} nhưng bàn chỉ có {available}!");
        //     }
        //     else if (available > pair.Value)
        //     {
        //         Debug.LogWarning($"[Level {levelId}] THỪA ĐỒ ĂN: Loại {pair.Key} trên bàn có {available} nhưng đơn hàng chỉ cần {pair.Value}.");
        //     }
        // }
    }
}