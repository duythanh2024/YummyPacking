using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public class OrderUIElement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Image[] foodIcon;        // Hình ảnh món ăn
    public GameObject completedOverlay; // Hình check-mark khi xong
    public string CurrentPoolTag { get; set; } // Lưu lại tag khi được spawn
    // Hàm này để nạp dữ liệu từ OrderData vào Prefab
    public void SetupUI(OrderData data)
    {
        if (data == null) return;

        
        List<FoodPlacement> requireds = data.requiredLayout;

        Debug.Log("requireds.Count "+requireds.Count +" foodIcon "+foodIcon.Length);

        for (int i = 0; i < requireds.Count; i++)
        {
            Sprite sprite = GameManager.Instance.foodDb.GetSpriteByID(requireds[i].foodType);
            if (sprite != null)
            {
                //loi
                foodIcon[i].sprite = sprite;
            }
        }

        completedOverlay.SetActive(false);

        // Hiệu ứng xuất hiện mượt mà
        transform.localScale = Vector3.zero;

        BentoTweenHelper.DoScale(transform,1f, 0.5f);

    }
    public void ClearOldData()
{
    CurrentPoolTag = ""; 
}
}
