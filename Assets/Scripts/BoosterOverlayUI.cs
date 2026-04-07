using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoosterOverlayUI : MonoBehaviour
{
    public GameObject overlayGroup; // Panel đen mờ hoặc chứa Anim
public RectTransform iconContainer; // Chứa Icon để scale cho mượt
    public Image iconImage;
    public TextMeshProUGUI boosterNameText;        // Icon to giữa màn hình
[Header("Settings")]
    private string boosterSeqId = "BoosterAnim_Global";
    public void PlayBoosterAnim(string name, Sprite icon, System.Action onCompleteAction)
    {
      // 1. Dọn dẹp & Khởi tạo (Kill để tránh spam nút)
        DOTween.Kill(boosterSeqId);
        
        boosterNameText.text = name.ToUpper(); // Voodoo thường dùng Uppercase cho mạnh mẽ
        iconImage.sprite = icon;
        
        // Reset trạng thái
        overlayGroup.gameObject.SetActive(true);
        // overlayGroup.alpha = 0;
         iconContainer.localScale = Vector3.zero;
         iconContainer.localRotation = Quaternion.identity;

        // 2. CHUỖI HIỆU ỨNG JUICY (Sequence)
        Sequence seq = DOTween.Sequence().SetId(boosterSeqId).SetUpdate(true); // Chạy bất kể pause game
        
        // --- GIAI ĐOẠN 1: IMPACT (Hiện hình) ---
        // Flash màn hình nhẹ và phóng to icon cực nhanh với Ease OutBack
        //seq.Append(overlayGroup..DOFade(1f, 0.15f).SetEase(Ease.OutCubic));
        seq.Join(iconContainer.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack)); 
        
        // --- GIAI ĐOẠN 2: EMPHASIS (Nhấn mạnh) ---
        // Lắc nhẹ icon và rung màn hình (Haptic Feedback giả lập)
        seq.Append(iconContainer.DOPunchRotation(new Vector3(0, 0, 15f), 0.4f, 10, 1f));
        seq.Join(iconContainer.DOPunchScale(Vector3.one * 0.1f, 0.4f, 5, 1f));

        // --- GIAI ĐOẠN 3: EXECUTION (Thực thi logic) ---
        // Khoảnh khắc icon to nhất, ta gọi logic game (Undo/Shuffle/Hammer)
        // Lúc này màn hình đang bị che bởi Overlay nên người chơi không thấy đĩa "nhảy"
        seq.AppendCallback(() => {
            onCompleteAction?.Invoke();
            // Thêm hiệu ứng rung camera nhẹ nếu là Hammer
            //if(name.ToLower().Contains("hammer")) Camera.main.transform.DOShakePosition(0.2f, 0.1f);
        });

        // Chờ một nhịp rất ngắn để người chơi kịp thấy thành quả
        seq.AppendInterval(0.15f);

        // --- GIAI ĐOẠN 4: OUTRO (Biến mất) ---
        // Thu nhỏ biến mất và làm mờ dần
        seq.Append(iconContainer.DOScale(0f, 0.5f).SetEase(Ease.InBack));
       // seq.Join(overlayGroup.DOFade(0f, 0.25f).SetEase(Ease.InCubic));
        
        seq.OnComplete(() => {
            overlayGroup.gameObject.SetActive(false);
        });
    }
    
}