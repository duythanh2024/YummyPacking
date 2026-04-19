using UnityEngine;

public class SafeArea : MonoBehaviour
{
      RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplyTopSafeArea();
    }

    void ApplyTopSafeArea()
    {
       Rect safeArea = Screen.safeArea;

        // 1. Tính toán tỉ lệ của vùng an toàn so với toàn màn hình (0.0 -> 1.0)
        float anchorMaxY = (safeArea.y + safeArea.height) / Screen.height;

        // 2. Ép Anchor Max Y của TopInfo xuống đúng vạch an toàn
        // Điều này đảm bảo TopInfo không bao giờ lấn lên vùng tai thỏ
        Vector2 anchorMax = rectTransform.anchorMax;
        anchorMax.y = anchorMaxY;
        rectTransform.anchorMax = anchorMax;

        // 3. Đặt Pivot và AnchoredPosition về 0 để nó bám chặt vào mép trên của Safe Area
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = Vector2.zero;
    }
}
