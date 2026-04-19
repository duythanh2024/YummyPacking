using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class BackgroundScaler : MonoBehaviour
{
    private RectTransform rectTransform;
    private Image image;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        ScaleBackground();
    }
// Update gọi trong Editor để bạn dễ dàng quan sát khi thay đổi độ phân giải
    void Update()
    {
        if (!Application.isPlaying) 
        {
            ScaleBackground();
        }
    }
    public void ScaleBackground()
    {
        if (rectTransform == null || image == null || image.sprite == null) return;

        // 1. Đặt Anchor và Pivot về tâm
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // 2. Lấy kích thước màn hình hiện tại từ Canvas
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // 3. Lấy tỉ lệ khung hình
        float screenAspect = screenWidth / screenHeight;
        float textureAspect = image.sprite.rect.width / image.sprite.rect.height;

        // 4. Tính toán scale để "Fill" toàn bộ màn hình (Crop nếu cần)
        if (screenAspect > textureAspect)
        {
            // Màn hình rộng hơn ảnh -> scale theo chiều rộng
            float scaleModifier = screenWidth / image.sprite.rect.width;
            rectTransform.sizeDelta = new Vector2(screenWidth, image.sprite.rect.height * scaleModifier);
        }
        else
        {
            // Màn hình cao hơn ảnh -> scale theo chiều cao
            float scaleModifier = screenHeight / image.sprite.rect.height;
            rectTransform.sizeDelta = new Vector2(image.sprite.rect.width * scaleModifier, screenHeight);
        }
    }
}