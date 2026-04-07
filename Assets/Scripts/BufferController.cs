using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
//Quản lý Khay chờ
public class BufferController : MonoBehaviour
{
    private int capacity = 0;
    private List<FoodTile> currentBufferTiles = new List<FoodTile>();
    public List<BuffSlot> allSlots; // Kéo 5 slot vào đây trong Inspector

    public void ShowBuffSlot()
    {

        //kiem tra 3 mua rui thi 4 mo lock
        for (int i = 3; i <= 5; i++)
        {
            BuffSlot buffSlot = allSlots[i];
            if (GameData.Slot1 && i == 3)
            {
                buffSlot.isBuyed = true;
                buffSlot.ShowAdd(false);

            }
            else if (!GameData.Slot2 && i == 4)
            {
                if (GameData.Slot1)
                {
                    buffSlot.isUnlocked = true;
                    buffSlot.isBuyed = false;
                    buffSlot.ShowAdd(true);
                    buffSlot.ShowLock(false);
                }

            }
            else if (GameData.Slot2 && i == 4)
            {
                buffSlot.isUnlocked = true;
                buffSlot.isBuyed = true;
                buffSlot.ShowAdd(false);
                buffSlot.ShowLock(false);
            }
            else if (!GameData.Slot3 && i == 5)
            {
                if (GameData.Slot2)
                {
                    buffSlot.isUnlocked = true;
                    buffSlot.isBuyed = false;
                    buffSlot.ShowAdd(true);
                    buffSlot.ShowLock(false);
                }

            }
            else if (GameData.Slot3 && i == 5)
            {
                buffSlot.isUnlocked = true;
                buffSlot.isBuyed = true;
                buffSlot.ShowAdd(false);
                buffSlot.ShowLock(false);
            }
        }
        GetCapacity();

    }

    public void GetCapacity()
    {
        capacity = allSlots.Where(n => n.isUnlocked && n.isBuyed).ToList().Count;
        Debug.Log("currentBufferTiles.Count capacity: " + capacity);
    }
    public bool IsFull()
    {
        Debug.Log("currentBufferTiles.Count " + currentBufferTiles.Count + " capacity: " + capacity);
        return currentBufferTiles.Count >= capacity;
    }
    public BuffSlot GetFirstEmptySlot()
    {
        foreach (var slot in allSlots)
        {
            if (!slot.isOccupied) return slot;
        }
        return null; // Không còn slot nào trống
    }

    public List<Transform> slotTransforms; // Kéo 6 vị trí ô trống trong Inspector vào đây
    public List<FoodTile> CheckBufferInTray()
    {
        return currentBufferTiles;
    }
    // Đảm bảo hàm này là 'public' để các lớp khác (như GameManager) có thể gọi được
    public bool AddToBuffer(FoodTile tile)
    {
        if (currentBufferTiles.Count < capacity)
        {
            currentBufferTiles.Add(tile);
            Debug.Log(currentBufferTiles.Count);
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
            BuffSlot currentSlot = tile.currentBuffSlot;
            if (currentSlot != null)
            {
                // Gọi quản lý khay để giải phóng slot này
                currentSlot.Release();
                currentSlot = null; // Xoá liên kết sau khi bay ra
                tile.currentBuffSlot = null;
            }
            currentBufferTiles.Remove(tile);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        // Dồn các tile còn lại sang trái cho gọn (Tùy chọn UX)
    }
    public void ClearBuffer()
    {
        // RemoveFood();
        for (int i = 0; i < allSlots.Count; i++)
        {
            allSlots[i].isOccupied = false;
        }
        currentBufferTiles.Clear();

    }
    // Hàm bị thiếu đây:
    public Vector3 GetNextAvailableSlotPos()
    {
        // Kiểm tra xem còn chỗ trống không
        if (currentBufferTiles.Count <= slotTransforms.Count)
        {
            // Trả về vị trí của ô tiếp theo dựa trên số lượng item hiện có
            return slotTransforms[currentBufferTiles.Count - 1].position;
        }

        // Nếu đầy khay, trả về vị trí hiện tại của Buffer hoặc báo lỗi
        Debug.LogWarning("Khay đã đầy, không còn chỗ trống!");
        return transform.position;
    }

    public void RemoveFood()
    {
        if (currentBufferTiles != null && currentBufferTiles.Count > 0)
        {
            for (int i = currentBufferTiles.Count - 1; i >= 0; i--)
            {
                Transform child = currentBufferTiles[i].gameObject.transform;

                // So sánh Tag dùng CompareTag sẽ nhanh hơn và tránh lỗi chính tả
                if (child.CompareTag("Food"))
                {
                    // Trả về pool (thường sẽ làm mất child này khỏi parent hiện tại)
                    ObjectPooler.Instance.ReturnToPool("Food", child.gameObject);
                    FoodTile foodTile = child.gameObject.GetComponent<FoodTile>();
                    foodTile.SetDefault();
                }
            }
        }

    }
    public void RemoveLastTileFromBuff(FoodTile tileToUndo)
    {
        if (tileToUndo == null || currentBufferTiles == null || currentBufferTiles.Count == 0) return;

        // 1. Kiểm tra xem đĩa này có thực sự nằm trong danh sách đã đóng gói không
        if (currentBufferTiles.Contains(tileToUndo))
        {

            RemoveTile(tileToUndo);
            BuffSlot currentSlot = tileToUndo.currentBuffSlot;
            if (currentSlot != null)
            {
                // Gọi quản lý khay để giải phóng slot này
                currentSlot.Release();
                tileToUndo.currentBuffSlot = null;
            }
            // 5. TRẢ LẠI TƯƠNG TÁC (QUAN TRỌNG)
            // Khi Undo, món ăn phải bấm được lại để người chơi nhặt tiếp
            if (tileToUndo.TryGetComponent<BoxCollider2D>(out var col))
            {
                col.enabled = true;
            }
            tileToUndo.transform.SetParent(null);
        }
    }

}