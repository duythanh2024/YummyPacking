using System;
using UnityEngine;
//Lớp này gắn trực tiếp lên Prefab của mỗi miếng đồ ăn.
public class FoodTile : MonoBehaviour 
{
    public FoodData data;
     public SpriteRenderer tray;
    public SpriteRenderer icon;
    public bool isClickable = false; // Trạng thái có bị đè hay không   
    public int columnId;
    public void SetDefault()
    {
        tray.gameObject.SetActive(true);
        tray.transform.localScale=Vector3.one;
        transform.localPosition=Vector3.zero;
        GetComponent<BoxCollider2D>().enabled=true;
    }
private void OnMouseDown() 
{
    //Debug.Log("Nhan "+isClickable);
    // Kiểm tra xem miếng Cá này có đang bị miếng khác đè không
    if (isClickable) 
    {
        // Gửi thông tin miếng Cá này sang bộ não trung tâm
        GameManager.Instance.OnTileTapped(this); 
    }
    else 
    {
        Debug.Log("NO Nhan ");
        // Hiệu ứng rung lắc nhẹ báo hiệu miếng Cá đang bị kẹt
      //  BentoTweenHelper.ShakeEffect(this.transform);
    }
}


    // Hàm thực hiện Animation bay
    public void MoveToTarget(Vector3 targetPosition, Action onComplete) 
    {
        // Dùng DOTween (Unity) hoặc cc.tween (Cocos) để di chuyển mượt mà
    }

    // Thay đổi trạng thái hiển thị (sáng/tối) dựa trên việc có bị đè không
    public void SetHighlight(bool clickable) 
    {
        isClickable = clickable;
        // Logic đổi màu material/sprite ở đây
    }
}