using UnityEngine;
using DG.Tweening;

public class CoinAnimation : MonoBehaviour
{
[Header("Settings")]
    [SerializeField] private float targetScale = 1.2f; // Scale lớn nhất (1.2 là 120%)
    [SerializeField] private float duration = 0.8f;    // Thời gian cho một nhịp
    [SerializeField] private Ease easeType = Ease.InOutSine; // Sine giúp chuyển động mượt nhất

    void Start()
    {
        StartBreathAnimation();
    }

  void StartBreathAnimation()
{
    Sequence s = DOTween.Sequence();
    
    s.Join(transform.DOScale(targetScale, duration));
    s.Join(transform.DORotate(new Vector3(0, 0, 30f), duration));
    
    s.SetEase(easeType)
     .SetLoops(-1, LoopType.Yoyo)
     .SetLink(gameObject)
     .SetUpdate(UpdateType.Normal, true); // Giúp tween mượt hơn trên mobile
}
}