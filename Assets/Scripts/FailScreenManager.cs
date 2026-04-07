using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System;

public class FailScreenManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button reviveAdButton;   // Nút Xem Ad để Undo 1 bước
    public Button tryAgainButton;   // Nút Chơi lại từ đầu (hiện khi không thể Undo)

    [Header("Texts & Visuals")]
    public TextMeshProUGUI statusText; // "OOPS!", "STUCK?", "BENTO FULL!"
    [Header("Target To Capture (World Space)")]
    public GameObject targetBentoTray; // Đã đổi thành GameObject

    public Slider Progress_Percent;
    private string tweenId = "FailScreenTween";

    // ==========================================
    // 1. HIỂN THỊ POPUP THUA
    // ==========================================


    public void ShowFailScreen(int percent)
    {

        // Dọn dẹp tween cũ nếu có
        DOTween.Kill(tweenId);

        // Kích hoạt Panel
        //  mainCanvasGroup.gameObject.SetActive(true);



        statusText.text = $"Order:{percent}% completed";
        Progress_Percent.value = (float)percent / 100;
        // popupContainer.localScale = Vector3.zero;

        // Cấu hình nút bấm dựa trên điều kiện bạn đưa ra
        reviveAdButton.gameObject.SetActive(true);
        tryAgainButton.gameObject.SetActive(true);
        // statusText.text = "STUCK? REVERSE TIME!";

        // Hiệu ứng nảy liên tục cho nút Ad để gây chú ý (Monetization)
        reviveAdButton.transform.DOScale(1.1f, 0.6f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetId(tweenId);


        // // Hiệu ứng hiện Popup chuyên nghiệp (Juicy)
        // Sequence showSeq = DOTween.Sequence().SetId(tweenId).SetUpdate(true);
        // showSeq.Append(mainCanvasGroup.DOFade(1f, 0.3f));
    }

    // ==========================================
    // 2. XỬ LÝ CLICK NÚT HỒI SINH (ADS)
    // ==========================================
    public void OnReviveAdClicked()
    {
        GameManager.Instance.isWin = false;
        GameManager.Instance.isFail = false;
        AudioManager.Instance.Play("Click");
        // 1. Khóa nút để tránh spam
        reviveAdButton.interactable = false;

        // 2. GIẢ LẬP GỌI SDK QUẢNG CÁO (AdMob/AppLovin/UnityAds)
        Debug.Log("<color=cyan>[ADS]</color> Requesting Rewarded Ad...");

        // Trong thực tế, bạn sẽ đợi Callback "OnUserEarnedReward"
        // Ở đây ta giả định xem xong thành công:
        StartCoroutine(ExecuteReviveRoutine());
    }

    private IEnumerator ExecuteReviveRoutine()
    {
        // Hiệu ứng tắt Popup mượt mà trước khi hành động
        Debug.Log("<color=cyan>[ADS]</color> Tat...");

        Debug.Log("<color=green>Reviving: Undoing 1 step...</color>");
        yield return new WaitForSeconds(0.4f);
        HideFailScreen();
        gameObject.SetActive(false);

        if (BoosterManager.Instance != null)
            BoosterManager.Instance.ExecuteUndo();

        if (GameManager.Instance != null)
            GameManager.Instance.processing = false;
    }

    // ==========================================
    // 3. XỬ LÝ CLICK NÚT TRY AGAIN (THUA HẲN)
    // ==========================================
    public void OnTryAgainClicked()
    {
        AudioManager.Instance.Play("Click");
        HideFailScreen();

        GameManager.Instance.processing = false;
        GameManager.Instance.RestartLevel();


    }

    // ==========================================
    // 4. ĐÓNG POPUP
    // ==========================================
    public void HideFailScreen()
    {
        DOTween.Kill(tweenId);

        reviveAdButton.interactable = true;

    }
}