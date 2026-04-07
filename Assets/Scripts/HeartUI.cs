using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI txtTimer;      // Text đếm giờ (00:30)
    public TextMeshProUGUI txtHeartCount; // Số lượng tim (5)
   // public Image imgFill;                 // Thanh loading vòng tròn (nếu có)

    void Start()
    {
        // Cập nhật lần đầu
        UpdateHeartCount(HeartManager.Instance.GetCurrentHearts());
    }

    void OnEnable()
    {
        // Đăng ký nhận sự kiện
        if (HeartManager.Instance != null)
        {
            HeartManager.Instance.OnTimerTick += UpdateTimer;
            HeartManager.Instance.OnHeartChanged += UpdateHeartCount;
        }
    }

    void OnDisable()
    {
        // Hủy đăng ký khi tắt UI để tránh lỗi
        if (HeartManager.Instance != null)
        {
            HeartManager.Instance.OnTimerTick -= UpdateTimer;
            HeartManager.Instance.OnHeartChanged -= UpdateHeartCount;
        }
    }

    // Hàm này chỉ chạy 1 lần/giây khi có tín hiệu
    void UpdateTimer(string timeString, float fillAmount)
    {
        if(txtTimer) txtTimer.text = timeString;
        //if(imgFill) imgFill.fillAmount = fillAmount;
    }

    // Hàm này chỉ chạy khi số tim thay đổi
    void UpdateHeartCount(int amount)
    {
         if(txtHeartCount) txtHeartCount.text = amount.ToString();
    }
}