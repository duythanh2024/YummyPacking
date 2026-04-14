using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening; 
using System.Collections;

public class WinScreenManager : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI coinRewardText;

    [Header("Buttons")]
    public Button claimX2Button;
    public Button nextButton;

    public GameObject Img_Cup;
    public GameObject Txt_Wel;

    [Header("Settings")]
    private int baseReward = 0; // Số coin gốc cho mỗi level
    // public GameObject particleSystem1, particleSystem2;
    public CoinEffectManager coinEffectManager;
    // private void Awake()
    // {
    //     winPanel.alpha = 0;
    //     winPanel.gameObject.SetActive(false);
    // }

    // --- BƯỚC 1: GỌI KHI NGƯỜI CHƠI THẮNG ---
    public void ShowWinScreen()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        Txt_Wel.SetActive(true);
        Img_Cup.GetComponent<RectTransform>().localScale = Vector3.one * 0.4f;
        Img_Cup.SetActive(true);
        StartCoroutine(CaptureBentoRoutine());
    }

    private IEnumerator CaptureBentoRoutine()
    {
        coinText.text = GameData.Coins.ToKMB();
        BentoTweenHelper.DoScale(Img_Cup.transform, 1, 1.5f, () => { });

        // Chờ đến cuối frame để đảm bảo mọi render đã hoàn tất
        yield return new WaitForSeconds(1.5f);

        // --- BƯỚC 2: HIỆN MÀN HÌNH WIN ---
        SetupWinUI();
    }

    private void SetupWinUI()
    {
        baseReward = GameManager.Instance.rewardLevel;
        coinRewardText.text = $"Reward: {baseReward}  <sprite name=\"coins_1\">";
        claimX2Button.interactable = true;
        nextButton.interactable = true;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }


        // Reset trạng thái nút
        claimX2Button.gameObject.SetActive(true);
        BentoTweenHelper.DOPunchScale(claimX2Button.transform, Vector3.one * 0.01f, 0.4f);
        //claimX2Button.transform.DOPunchScale(Vector3.one * 0.01f, 0.4f).SetLoops(-1);        // Hiệu ứng Fade In chuyên nghiệp
    }

    // --- BƯỚC 3: XỬ LÝ CLICK NÚT ---

    // Gắn vào ClaimX2Button
    public void OnClaimX2Clicked()
    {
        AudioManager.Instance.Play("Click");
        // 1. Gọi SDK Quảng cáo (Giả định bạn dùng AdMob/AppLovin)
        ///    Debug.Log("<color=green>Calling Rewarded Ad SDK...</color>");

        // Nếu dùng quảng cáo thực, bạn sẽ đăng ký callback cho "UserRewarded"
        // Ở đây tôi giả định xem quảng cáo thành công ngay lập tức:
        OnRewardedAdComplete();
    }

    // Gắn vào NoThanksButton
    public void OnNoThanksClicked()
    {
        AudioManager.Instance.Play("Click");
        claimX2Button.interactable = false;
        nextButton.interactable = false;

        coinEffectManager.PlayCoinFlowEffect(() =>
        {
            ClaimCoinsAndProceed(baseReward);
        });

    }

    // --- BƯỚC 4: NHẬN THƯỞNG & LÀM SẠCH ---

    // Callback khi xem quảng cáo thành công
    private void OnRewardedAdComplete()
    {
        claimX2Button.interactable = false;
        nextButton.interactable = false;
        int x2Reward = baseReward * 2;

        // Thêm hiệu ứng âm thanh "Tiền bay"
        //   Debug.Log("<color=yellow>Ad Watched: Rewarded X2!</color>");

        coinEffectManager.PlayCoinFlowEffect(() =>
               {
                   // Nhận coin gấp đôi
                   ClaimCoinsAndProceed(x2Reward);
               });

    }

    private void ClaimCoinsAndProceed(int amount)
    {
        DOTween.Kill("ClaimProcess");
        GameData.Coins += amount;

        coinText.text = GameData.Coins.ToKMB();

        // Dùng biến để quản lý hoặc gắn Target để an toàn
        DOVirtual.DelayedCall(1.0f, () =>
        {

            if (GameManager.Instance != null)
            {
                // GameManager.Instance.processing = false;
                GameManager.Instance.LoadNextLevel();
            }
        })
        .SetTarget(this) // Nếu script này bị hủy, Delay 1s này cũng biến mất
        .SetId("ClaimProcess"); // Đặt ID để có thể Kill thủ công nếu cần
    }
    private void OnDisable()
    {
       // Dọn dẹp "kép" để không lọt lưới bất kỳ Tween nào

    // 1. Giết các DOVirtual hoặc các Tween có target được gán bằng this (như DelayedCall ở trên)
    DOTween.Kill(this); 

    // 2. Giết các hiệu ứng vật lý, di chuyển, co giãn tác động lên Object này
    transform.DOKill();
    }

}