using UnityEngine;

public class BuffSlot : MonoBehaviour
{
    public int index;
    public bool isOccupied = false; // Đánh dấu slot trống hay không
     public bool isUnlocked = false; //cho mua
    public bool isBuyed = false; //chua mua
    public Transform anchorPoint;  // Vị trí tâm của slot để món ăn bay vào đúng chỗ
    public GameObject add;
     public GameObject locks;

    public void ShowLock(bool display)
    {
        locks.SetActive(display);
    }
     public void ShowAdd(bool display)
    {
        add.SetActive(display);
    }

    // Hàm để món ăn "đăng ký" vào slot
    public void Occupy() => isOccupied = true; 

    // Hàm để giải phóng slot khi món ăn bay ra
    public void Release() => isOccupied = false; //trong
    public void Unlocked() => isUnlocked = true;
    public void IsBuy() => isBuyed = true;
}