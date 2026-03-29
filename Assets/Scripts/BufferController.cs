using System;
using System.Collections.Generic;
using UnityEngine;
//Quản lý Khay chờ
public class BufferController :MonoBehaviour
{
    public int capacity = 5;
    private List<FoodTile> currentBufferTiles=new List<FoodTile>();

    public bool IsFull() => currentBufferTiles.Count >= capacity;
public List<Transform> slotTransforms; // Kéo 6 vị trí ô trống trong Inspector vào đây
    // public bool AddTile(FoodTile tile) 
    // {
    //     if (IsFull()) return false;
    //     currentBufferTiles.Add(tile);
    //     // Tính toán tọa độ ô trống trong khay và gọi tile.MoveToTarget(...)
    //     return true;
    // }

    public List<FoodTile>  CheckBufferInTray()
    {
        return currentBufferTiles;
    }
// Đảm bảo hàm này là 'public' để các lớp khác (như GameManager) có thể gọi được
    public bool AddToBuffer(FoodTile tile)
    {
        if (currentBufferTiles.Count < capacity)
        {
            currentBufferTiles.Add(tile);
            return true;
            // Thêm logic sắp xếp vị trí các mảnh trong khay tại đây
        }
        else
        {
            Debug.Log("Buffer đã đầy!");
             return false;
        }
    }
    public void RemoveTile(FoodTile tile) 
    {
        try
        {
              currentBufferTiles.Remove(tile);
        }catch(Exception e)
        {
            Debug.Log(e.Message);
        }
      
        // Dồn các tile còn lại sang trái cho gọn (Tùy chọn UX)
    }
    public void ClearBuffer()
    {
        currentBufferTiles.Clear();
        
    }
    // Hàm bị thiếu đây:
    public Vector3 GetNextAvailableSlotPos()
    {
        // Kiểm tra xem còn chỗ trống không
        if (currentBufferTiles.Count < slotTransforms.Count)
        {
            // Trả về vị trí của ô tiếp theo dựa trên số lượng item hiện có
            return slotTransforms[currentBufferTiles.Count-1].position;
        }

        // Nếu đầy khay, trả về vị trí hiện tại của Buffer hoặc báo lỗi
        Debug.LogWarning("Khay đã đầy, không còn chỗ trống!");
        return transform.position; 
    }
}