using UnityEngine;
using DG.Tweening;
using System.Collections;
using System; // Thêm namespace này để dùng Action

public class CoinEffectManager : MonoBehaviour
{
    public static CoinEffectManager Instance;

    [Header("Settings")]
    public GameObject coinPrefab;      // Prefab của đồng coin (UI Image)
    public RectTransform coinTarget;       // Vị trí đích trên UI (Icon Coin)
    public RectTransform coinSpawnParent;  // Thường là một Layer trên cùng của Canvas
    public int coinCount = 10;         // Số lượng coin muốn hiện
    
    private Coroutine spawnRoutine;

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

        GameObject coin = Instantiate(coinPrefab, coinSpawnParent);
        
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

        s.OnComplete(() => {
            if (coinTarget != null)
            {
                // Tránh gọi PunchScale quá nhiều lần gây giật lag UI, 
                // có thể check nếu là đồng xu cuối cùng hoặc cách n đồng xu mới punch 1 lần.
                coinTarget.DOPunchScale(Vector3.one * 0.1f, 0.1f, 5, 1);
            }

            if (coin != null) Destroy(coin);

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