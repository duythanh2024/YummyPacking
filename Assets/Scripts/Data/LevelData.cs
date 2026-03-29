using UnityEngine;

using System.Collections.Generic;

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

    [Header("--- BỐ CỤC BÀN ĂN (TẦNG 3) ---")]
    [Tooltip("Danh sách tất cả các mảnh đồ ăn có trên bàn lúc bắt đầu")]
    public List<BoardItemSetup> boardItems;

    // Hàm tiện ích: Tự động kiểm tra xem thiết kế level có bị lỗi thiếu/thừa đồ ăn không
    // (CTO rất thích những hàm Validate như thế này để tránh lỗi do Designer nhập sai)
    private void OnValidate()
    {
        // Có thể viết logic đếm tổng số foodAsset trong boardItems 
        // và so sánh với tổng requirement của orderQueue để cảnh báo lỗi ngay trên Editor.
    }
}