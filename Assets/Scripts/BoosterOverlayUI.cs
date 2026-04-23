using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class BoosterOverlayUI : MonoBehaviour
{
    public GameObject overlayGroup; 
    public RectTransform iconContainer; 
    public Image iconImage;
    public TextMeshProUGUI boosterNameText;        

    // Cache lại các tham số Vector để tránh khởi tạo Struct liên tục trong hàm
    private readonly Vector3 punchRotation = new Vector3(0, 0, 15f);
    private readonly Vector3 punchScale = new Vector3(0.1f, 0.1f, 0.1f);

    // Lưu Action vào biến class để tránh tạo rác Closure (Lambda)
    private Action currentOnCompleteAction;

    // private void Awake()
    // {
    //     // Tối ưu 1: Ép TextMeshPro luôn hiển thị chữ IN HOA ở cấp độ render.
    //     // Cài đặt này chỉ gọi 1 lần khi khởi tạo.
    //     boosterNameText.fontStyle = FontStyles.UpperCase;
    // }

    public void PlayBoosterAnim(string name, Sprite icon, Action onCompleteAction)
    {
        // Tối ưu 2: Dùng gameObject làm Target/ID thay vì cấp phát String
        // Việc dùng chuỗi string "BoosterAnim_Global" làm ID sẽ khiến DOTween phải xử lý so sánh chuỗi, chậm hơn so với so sánh Object Reference.
        DOTween.Kill(this.gameObject);

        // Lưu callback lại để dùng lúc sau
        this.currentOnCompleteAction = onCompleteAction;

        // Tối ưu 1 (Tiếp): Gán thẳng chuỗi name. Không dùng name.ToUpper() nữa!
        boosterNameText.SetText(name); 
        iconImage.sprite = icon;

        // Reset trạng thái (overlayGroup vốn đã là GameObject, không cần gọi .gameObject.SetActive)
        overlayGroup.SetActive(true);
        iconContainer.localScale = Vector3.zero;
        iconContainer.localRotation = Quaternion.identity;

        // 2. CHUỖI HIỆU ỨNG JUICY (Sequence)
        // SetTarget(this.gameObject) để gán ID quản lý tween
        Sequence seq = DOTween.Sequence().SetUpdate(true).SetTarget(this.gameObject).SetLink(this.gameObject); // <--- THÊM DÒNG NÀY VÀO ĐÂY; 

        // --- GIAI ĐOẠN 1: IMPACT ---
        seq.Join(iconContainer.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack));

        // --- GIAI ĐOẠN 2: EMPHASIS ---
        seq.Append(iconContainer.DOPunchRotation(punchRotation, 0.4f, 10, 1f));
        seq.Join(iconContainer.DOPunchScale(punchScale, 0.4f, 5, 1f));

        seq.AppendInterval(0.15f);

        // --- GIAI ĐOẠN 3: OUTRO ---
        seq.Append(iconContainer.DOScale(0f, 0.5f).SetEase(Ease.InBack));

        // Tối ưu 3: Dùng Method Group thay vì Lambda/Anonymous Function
        seq.OnComplete(OnAnimationComplete);
    }

    // Hàm riêng để xử lý khi kết thúc Tween
    private void OnAnimationComplete()
    {
        overlayGroup.SetActive(false);
        
        currentOnCompleteAction?.Invoke();
        currentOnCompleteAction = null; // Clear tham chiếu để giải phóng bộ nhớ an toàn
    }
}