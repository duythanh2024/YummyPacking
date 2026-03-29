using System.Collections.Generic;
using UnityEngine;

//XU ly tang 3: cacs mon an xep cuoi cung de chon
public class MahjongBoardController : MonoBehaviour
{

public List<FoodTile>[] allTilesOnBoard = new List<FoodTile>[3];

    private void Awake() {
        // Khởi tạo List cho từng cột
        for (int i = 0; i < 3; i++) {
            allTilesOnBoard[i] = new List<FoodTile>();
        }
    }
    // RẤT QUAN TRỌNG: Thuật toán kiểm tra xem mảnh nào được phép nhấn
    public void UpdateClickableStates()
    {
    
    for (int colId = 0; colId < 3; colId++)
    {
        for (int i = 0; i < allTilesOnBoard[colId].Count; i++)
        {
            FoodTile tile = allTilesOnBoard[colId][i];
            
            // Chỉ miếng index 0 trong mỗi cột mới được bấm
            tile.isClickable = (i == 0);

            // Hiệu ứng Visual (Sáng/Tối)
            Color targetColor = tile.isClickable ? Color.white : Color.gray;
           // tile.tray.GetComponent<SpriteRenderer>().DOColor(targetColor, 0.2f);
            BentoTweenHelper.SafeDOColor(tile.tray.GetComponent<SpriteRenderer>(),targetColor, 0.2f);
            
        }
    }
    }
    // private bool IsCovered(FoodTile checkTile)
    // {
    //     // 1. Lấy vị trí Local để so sánh chính xác hơn World
    //     Vector3 checkPos = checkTile.transform.localPosition;

    //     // 2. Lấy kích thước Collider (nên cache size này lại để tối ưu)
    //     Vector2 size = checkTile.GetComponent<BoxCollider2D>().size;

    //     foreach (FoodTile otherTile in allTilesOnBoard)
    //     {
    //         if (otherTile == checkTile) continue;

    //         Vector3 otherPos = otherTile.transform.localPosition;

    //         // LOGIC Z: Miếng ở trên có Z nhỏ hơn (ví dụ -1 < 0)
    //         // Ta dùng một khoảng sai số nhỏ (0.01f) để tránh lỗi làm tròn số thực
    //         if (otherPos.z < checkPos.z - 0.01f)
    //         {
    //             // 3. Kiểm tra va chạm diện tích (Overlap) theo trục X và Y
    //             // Dùng 0.95f để người chơi dễ bấm hơn (chuẩn Voodoo)
    //             bool isOverlapX = Mathf.Abs(otherPos.x - checkPos.x) < size.x * 0.95f;
    //             bool isOverlapY = Mathf.Abs(otherPos.y - checkPos.y) < size.y * 0.95f;

    //             if (isOverlapX && isOverlapY)
    //             {
    //                 // Debug.Log($"{otherTile.name} (Z:{otherPos.z}) ĐÈ LÊN {checkTile.name} (Z:{checkPos.z})");
    //                 return true;
    //             }
    //         }
    //     }
    //     return false;
    // }
    // private bool IsCovered1(FoodTile checkTile)
    // {
    //     // Lấy Bounds (khung bao) 2D của mảnh hiện tại
    //     Bounds checkBounds = checkTile.GetComponent<BoxCollider2D>().bounds;

    //     foreach (FoodTile otherTile in allTilesOnBoard)
    //     {
    //         Debug.Log("UpdateClickableStates 1");
    //         if (otherTile == checkTile) continue;
    //         Debug.Log("UpdateClickableStates 2");
    //         // Nếu mảnh khác có Layer (Z-index) cao hơn VÀ khung bao đè lên nhau
    //         if (otherTile.transform.position.z < checkTile.transform.position.z)
    //         {
    //             Debug.Log("UpdateClickableStates 3");
    //             Bounds otherBounds = otherTile.GetComponent<BoxCollider2D>().bounds;
    //             if (checkBounds.Intersects(otherBounds))
    //             {
    //                 Debug.Log("giao");
    //                 return true;
    //             }
    //         }
    //     }
    //     return false;
    // }
    // // Gọi khi một mảnh bị nhặt đi
    // // public void RemoveTileFromBoard(FoodTile tile)
    // // {
    // //     activeTilesOnBoard.Remove(tile);
    // //     UpdateClickableStates(); // Cập nhật lại trạng thái các mảnh bên dưới

    // //     // Kiểm tra Win Condition ở Tầng 3
    // //     if (activeTilesOnBoard.Count == 0)
    // //     {
    // //         //GameEvents.TriggerBoardCleared();
    // //     }
    // // }

    public void RemoveTile(FoodTile tile)
    {
       
        UpdateClickableStates();
    }
}