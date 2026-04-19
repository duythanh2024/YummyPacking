using UnityEngine;

public class FoodSlot : MonoBehaviour
{
    public bool isOccupied = false; // Đánh dấu slot trống hay không
    public bool isLocked = false; // Đánh dấu slot trống hay không
    public Transform anchorPoint;  // Vị trí tâm của slot để món ăn bay vào đúng chỗ
    // Hàm để món ăn "đăng ký" vào slot
    public void Occupy() => isOccupied = true; 

    // Hàm để giải phóng slot khi món ăn bay ra
    public void Release() => isOccupied = false; //trong
}
