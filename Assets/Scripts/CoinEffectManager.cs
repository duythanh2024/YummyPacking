using UnityEngine;
using DG.Tweening;
using System.Collections;
using System; // Thêm namespace này để dùng Action

public class CoinEffectManager : MonoBehaviour
{
    public static CoinEffectManager Instance;

    [Header("Settings")]
    //public GameObject coinPrefab;      // Prefab của đồng coin (UI Image)
    public RectTransform coinTarget;       // Vị trí đích trên UI (Icon Coin)
    public RectTransform coinSpawnParent;  // Thường là một Layer trên cùng của Canvas
    public int coinCount = 10;         // Số lượng coin muốn hiện

    private Coroutine spawnRoutine;
    //Shop
    public void PlayCoinOverLayFlowEffect(Action onAllComplete = null)
    {
        // Dừng Routine cũ nếu nó đang chạy để tránh chồng chéo
        StopSpawnRoutine();
        spawnRoutine = StartCoroutine(SpawnCoinsOverLayRoutine(onAllComplete));
    }
    private IEnumerator SpawnCoinsOverLayRoutine(Action onAllComplete)
    {
        int completedCoins = 0;
        if (coinSpawnParent == null) yield break;

        // Trong Canvas Overlay, .position trả về tọa độ pixel trên màn hình.
        // Đảm bảo coinTarget cũng nằm trong Canvas.
        Vector3 targetScreenPos = coinTarget.position;

        for (int i = 0; i < coinCount; i++)
        {
            if (this == null || coinSpawnParent == null) yield break;


            GameObject coin = ObjectPooler.Instance.SpawnFromPool("Coins",Vector3.zero, Quaternion.identity);
            coin.transform.SetParent(coinSpawnParent);

            //GameObject coin = Instantiate(coinPrefab, coinSpawnParent);

            // 1. Chuyển sang dùng RectTransform cho UI
            RectTransform coinRect = coin.GetComponent<RectTransform>();

            // 2. Dùng anchoredPosition thay vì localPosition để tính toán offset trong UI
            float randomX = UnityEngine.Random.Range(-50f, 50f);
            float randomY = UnityEngine.Random.Range(-50f, 50f);

            coinRect.anchoredPosition = new Vector2(randomX, randomY);
            coinRect.localScale = Vector3.zero;

            Sequence s = DOTween.Sequence();
            s.SetTarget(coin);

            // Hiệu ứng hiện ra
            s.Append(coinRect.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));

            // 3. Bay về đích. DOMove vẫn hoạt động tốt với màn hình Overlay khi dùng chung hệ trục position
            s.Append(coinRect.DOMove(targetScreenPos, 0.7f).SetEase(Ease.InBack));
            s.Join(coinRect.DOScale(0.4f, 0.7f));

            s.OnComplete(() =>
            {
                if (coinTarget != null)
                {
                    // Tối ưu UI: Không gọi PunchScale trên mọi đồng xu để tránh lỗi đè Tween làm méo UI
                    // Chỉ giật scale và phát âm thanh mỗi 3 đồng xu 1 lần (hoặc tùy bạn chỉnh)
                    if (completedCoins % 3 == 0 || completedCoins == coinCount - 1)
                    {
                        // Kill tween cũ trước khi chạy tween mới để UI không bị vỡ scale
                        coinTarget.DOKill(true);
                        coinTarget.DOPunchScale(Vector3.one * 0.1f, 0.1f, 5, 1);
                        AudioManager.Instance.Play("Coins");
                    }
                }

                if (coin != null)
                {
                    ObjectPooler.Instance.ReturnToPool("Coins",coin);
                }

                completedCoins++;
                if (completedCoins >= coinCount)
                {
                    ResetTargetScale();
                    onAllComplete?.Invoke();
                }
            });

            yield return new WaitForSeconds(0.05f);
        }
    }
    //Win

    public void PlayCoinFlowEffect(Action onAllComplete = null)
    {
        // Dừng Routine cũ nếu nó đang chạy để tránh chồng chéo
        StopSpawnRoutine();
        spawnRoutine = StartCoroutine(SpawnCoinsRoutine(onAllComplete));
    }

    private IEnumerator SpawnCoinsRoutine(Action onAllComplete)
    {
        int completedCoins = 0;
        if (coinSpawnParent == null) yield break;

        // Lấy vị trí đích trong World Space một lần duy nhất trước khi lặp
        Vector3 targetWorldPos = coinTarget.position;

        for (int i = 0; i < coinCount; i++)
        {
            if (this == null || coinSpawnParent == null) yield break;

          //  GameObject coin = Instantiate(coinPrefab, coinSpawnParent);
            GameObject coin = ObjectPooler.Instance.SpawnFromPool("Coins",Vector3.zero, Quaternion.identity);
            coin.transform.SetParent(coinSpawnParent);
            // Tính toán vị trí ngẫu nhiên quanh điểm spawn
            float randomX = UnityEngine.Random.Range(-50f, 50f); // Giới hạn vùng spawn nhỏ lại cho đẹp
            float randomY = UnityEngine.Random.Range(-50f, 50f);

            coin.transform.localPosition = new Vector3(randomX, randomY, 0);
            coin.transform.localScale = Vector3.zero;

            Sequence s = DOTween.Sequence();
            s.SetTarget(coin);

            // Hiệu ứng hiện ra (Pop up)
            s.Append(coin.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));

            // Hiệu ứng bay về đích (Sử dụng DOMove với vị trí World đã lấy)
            // Dùng SetUpdate(true) nếu game của bạn có lúc pause (Time.timeScale = 0)
            s.Append(coin.transform.DOMove(targetWorldPos, 0.7f).SetEase(Ease.InBack));
            s.Join(coin.transform.DOScale(0.4f, 0.7f));

            s.OnComplete(() =>
            {
                if (coinTarget != null)
                {
                    // Tránh gọi PunchScale quá nhiều lần gây giật lag UI, 
                    // có thể check nếu là đồng xu cuối cùng hoặc cách n đồng xu mới punch 1 lần.
                    coinTarget.DOPunchScale(Vector3.one * 0.1f, 0.1f, 5, 1);
                }
                //Lam lai
               
                if (coin != null)
                {
                    ObjectPooler.Instance.ReturnToPool("Coins",coin);
                }

                completedCoins++;
                if (completedCoins >= coinCount)
                {
                    AudioManager.Instance.Play("Coins");
                    ResetTargetScale(); // Đảm bảo hàm này không xung đột với PunchScale
                    onAllComplete?.Invoke();
                }
            });

            yield return new WaitForSeconds(0.05f);
        }
    }

    private void ResetTargetScale()
    {
        if (coinTarget != null)
        {
            coinTarget.DOKill(); // Dừng mọi rung lắc đang dở dang
            coinTarget.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad);
        }
    }

    private void StopSpawnRoutine()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    // Quan trọng: Tự động dọn dẹp khi Component này bị hủy hoặc Disable
    private void OnDisable()
    {
        StopSpawnRoutine();
        // Kill toàn bộ các tween liên quan đến manager này để tránh treo bộ nhớ
        DOTween.Kill(this);

        // Nếu coinTarget vẫn tồn tại, đưa nó về trạng thái chuẩn
        if (coinTarget != null) coinTarget.localScale = Vector3.one;
    }
}