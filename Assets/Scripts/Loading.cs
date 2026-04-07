using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_IOS
using Unity.Advertisement.IosSupport;
#endif
public class Loading : MonoBehaviour
{
    public Slider progressBar;
    public TextMeshProUGUI statusText;
    // public GameObject pnl_check_work;

    private float duration = 2.5f;
    async void Start()
    {
        // Gọi hàm dùng chung từ NetworkHelper đã viết ở trên
        // bool hasInternet = await Utilities.IsInternetAvailable();

        // if (hasInternet)
        // {
        //     StartLoading();
        // }
        // else
        // {
        //     pnl_check_work.SetActive(true);
        // }
        StartLoading();
    }

    public void StartLoading()
    {
        StartCoroutine(StartFakeLoading());
    }
    IEnumerator StartFakeLoading()
    {
        float elapsed = 0f;
        if (statusText != null) statusText.text = "Loading...";

        // 1. Chạy thanh Loading giả (Giữ nguyên logic của bạn)
        while (elapsed < duration)
        {
            float speed = Random.Range(0.5f, 1.5f);
            elapsed += Time.deltaTime * speed;
            float progress = Mathf.Clamp01(elapsed / duration);
            if (progressBar != null) progressBar.value = progress;
            yield return null;
        }

        // 2. Xử lý Apple ATT (Chỉ hiện Popup hệ thống, không hiện màn hình mồi)
#if UNITY_IOS && !UNITY_EDITOR
        // Kiểm tra trạng thái, nếu chưa xác định (NOT_DETERMINED) thì mới hiện Popup
        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            // Lệnh này kích hoạt TRỰC TIẾP Popup của Apple
            ATTrackingStatusBinding.RequestAuthorizationTracking();

            // Đợi cho đến khi người dùng nhấn "Allow" hoặc "Ask App Not to Track"
            // Việc đợi này rất quan trọng để đảm bảo có IDFA trước khi vào Home
            while (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
            {
                yield return null;
            }
        }
        
        // Log trạng thái để bạn kiểm tra trong Xcode console
#endif

        // 3. Bước cuối: Vào Home
        EnterHome();
    }

    void EnterHome()
    {
        // Sử dụng LoadSceneAsync để quá trình chuyển cảnh mượt mà hơn
        SceneManager.LoadSceneAsync("Home");
    }
}
