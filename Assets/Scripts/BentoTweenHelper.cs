using UnityEngine;
using DG.Tweening;
using System;

public static class BentoTweenHelper
{
    private static float snapDuration = 0.3f;

    // ========================================================================
    // LOGIC CỐT LÕI: Di chuyển an toàn (Safe Move)
    // ========================================================================
    public static void SafeMove(Transform target, Vector3 endPos, float moveDuration, Ease standardEase, Action onComplete = null)
    {
        if (target == null) return;
        target.DOKill(complete: true);

        target.DOMove(endPos, moveDuration)
              .SetEase(standardEase)
              .SetLink(target.gameObject)
              .OnComplete(() => onComplete?.Invoke());
    }

    public static void SafeDOLocalMove(Transform target, Vector3 endPos, float moveDuration, Ease standardEase, Action onComplete = null)
    {
        if (target == null) return;
        target.DOKill(complete: true);

        target.DOLocalMove(endPos, moveDuration)
              .SetEase(standardEase)
              .SetLink(target.gameObject)
              .OnComplete(() => onComplete?.Invoke());
    }

    public static void SafeDOScale(Transform target, Vector3 endPos, float duration, LoopType loopType, Action onComplete = null)
    {
        if (target == null) return;
        target.DOKill(complete: true);

        target.DOScale(endPos, duration)
              .SetLoops(2, loopType)
              .SetLink(target.gameObject)
              .OnComplete(() => onComplete?.Invoke());
    }

    public static void SafeDOShakePosition(Transform target, float duration, Vector3 endPos, int vibrato, float randomness, Action onComplete = null)
    {
        if (target == null) return;
        target.DOKill(complete: true);

        target.DOShakePosition(duration, endPos, vibrato, randomness)
              .SetLink(target.gameObject)
              .OnComplete(() => onComplete?.Invoke());
    }

    public static void SafeDOColor(SpriteRenderer target, Color targetColor, float duration, Action onComplete = null)
    {
        if (target == null) return;
        target.DOKill(complete: true);

        target.DOColor(targetColor, duration)
              .SetLink(target.gameObject)
              .OnComplete(() => onComplete?.Invoke());
    }

    // ========================================================================
    // GAME FEEL: Di chuyển vòng cung (Parabolic Move)
    // ========================================================================
    public static void ParabolicMove(Transform target, Vector3 worldEndPos, Action onComplete = null)
    {
        if (target == null) return;
        target.DOKill(true);

        target.DOScale(new Vector3(0.85f, 0.85f, 0.85f), 0.05f)
              .SetEase(Ease.OutQuad)
              .SetLink(target.gameObject)
              .OnComplete(() =>
              {
                  // CHÚ Ý: Phải check null vì object có thể bị xóa trước khi callback chạy
                  if (target == null) return; 

                  target.DOScale(new Vector3(0.7f, 1.4f, 0.7f), snapDuration * 0.5f)
                        .SetLoops(2, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine)
                        .SetLink(target.gameObject);

                  target.DOMove(worldEndPos, snapDuration)
                        .SetEase(Ease.OutQuint)
                        .SetLink(target.gameObject)
                        .OnComplete(() =>
                        {
                            if (target == null) return;
                            target.position = worldEndPos;
                            ApplyVoodooImpact(target, onComplete);
                        });
              });
    }

    private static void ApplyVoodooImpact(Transform target, Action onComplete)
    {
        if (target == null) return;

        Sequence impactSeq = DOTween.Sequence();
        impactSeq.SetLink(target.gameObject); // Link toàn bộ Sequence vào target
        
        impactSeq.Append(target.DOScale(new Vector3(1.25f, 0.75f, 1.25f), 0.1f).SetEase(Ease.OutQuad));
        impactSeq.Append(target.DOScale(new Vector3(0.95f, 1.1f, 0.95f), 0.1f).SetEase(Ease.OutQuad));
        impactSeq.Append(target.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutElastic));

        impactSeq.OnComplete(() => {
            onComplete?.Invoke();
        });
    }

    // ========================================================================
    // EFFECT: Co giãn khi xếp vào (Pack Pop)
    // ========================================================================
    public static void PackPopEffect(Transform target, Action onComplete = null)
    {
        if (target == null) return;
        target.DOKill(complete: true);

        target.DOScale(1.2f, 0.15f)
              .SetEase(Ease.OutBack)
              .SetLink(target.gameObject)
              .OnComplete(() => {
                  if (target == null) return; // Check an toàn
                  target.DOScale(1f, 0.1f)
                        .SetLink(target.gameObject)
                        .OnComplete(() => onComplete?.Invoke());
              });
    }

    public static void ErrorShake(Transform target, Action onComplete = null)
    {
        if (target == null) return;
        target.DOKill(true);

        target.DOShakePosition(0.3f, 0.2f, 10, 90f, false, true)
              .SetLink(target.gameObject)
              .OnComplete(() => onComplete?.Invoke());

        if (target.TryGetComponent<SpriteRenderer>(out var sr))
        {
            sr.DOColor(Color.red, 0.15f).SetLoops(2, LoopType.Yoyo).SetLink(target.gameObject);
        }

        target.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f, 10, 1f)
              .SetLink(target.gameObject);
    }

    public static void DoScale(Transform target, float endValue, float duration, Action onComplete = null)
    {
        if (target == null) return;
        target.DOKill(complete: true);

        target.DOScale(endValue, duration)
              .SetEase(Ease.OutBack)
              .SetLink(target.gameObject)
              .OnComplete(() => onComplete?.Invoke());
    }

    public static void DOPunchScale(Transform target, Vector3 endValue, float duration, Action onComplete = null)
    {
        if (target == null) return;
        target.DOKill(complete: true);

        target.DOPunchScale(endValue, duration)
              .SetLoops(-1)
              .SetLink(target.gameObject); 
              // Lưu ý: Lặp vô hạn nên sẽ cần gọi DOKill ở chỗ khác để dừng
    }
}