using UnityEngine;
using DG.Tweening; // Import thư viện DOTween
using System;

public static class BentoTweenHelper
{
    // Cấu hình chuẩn cho Mobile để tối ưu hóa CPU và Game Feel
    private static float snapDuration = 0.3f;

    // ========================================================================
    // LOGIC CỐT LÕI: Di chuyển an toàn (Safe Move)
    // TỐI ƯU MOBILE: Gọi DOKill() ngay lập tức để giải phóng tween cũ,
    // tránh xung đột logic và tiết kiệm bộ nhớ trong pool của DOTween.
    // ========================================================================
    public static void SafeMove(Transform target, Vector3 endPos, float moveDuration,Ease standardEase, Action onComplete = null)
    {
        if (target == null) return;

        // 1. NGAY LẬP TỨC Huỷ các animation đang chạy trên object này
        // complete=true: Ép animation hiện tại nhảy về đích ngay lập tức, tránh object lơ lửng.
        target.DOKill(complete: true);

        // 2. Chạy animation mới
        target.DOMove(endPos, moveDuration)
              .SetEase(standardEase)
              .OnComplete(() => onComplete?.Invoke()); // Gọi Callback khi xong
    }

      public static void SafeDOLocalMove(Transform target, Vector3 endPos, float moveDuration,Ease standardEase, Action onComplete = null)
    {
        if (target == null) return;

        // 1. NGAY LẬP TỨC Huỷ các animation đang chạy trên object này
        // complete=true: Ép animation hiện tại nhảy về đích ngay lập tức, tránh object lơ lửng.
        target.DOKill(complete: true);

        // 2. Chạy animation mới
        target.DOLocalMove(endPos, moveDuration)
              .SetEase(standardEase)
              .OnComplete(() => onComplete?.Invoke()); // Gọi Callback khi xong
    }
    public static void SafeDOScale(Transform target, Vector3 endPos, float duration,LoopType loopType , Action onComplete = null)
    {
        if (target == null) return;

        // 1. NGAY LẬP TỨC Huỷ các animation đang chạy trên object này
        // complete=true: Ép animation hiện tại nhảy về đích ngay lập tức, tránh object lơ lửng.
        target.DOKill(complete: true);
        // 2. Chạy animation mới
        target.DOScale(endPos, duration)
             .SetLoops(2, loopType)
              .OnComplete(() => onComplete?.Invoke()); // Gọi Callback khi xong
    }
   public static void SafeDOShakePosition(Transform target,float duration, Vector3 endPos, int vibrato,float randomness, Action onComplete = null)
    {
        if (target == null) return;

        // 1. NGAY LẬP TỨC Huỷ các animation đang chạy trên object này
        // complete=true: Ép animation hiện tại nhảy về đích ngay lập tức, tránh object lơ lửng.
        target.DOKill(complete: true);

        // 2. Chạy animation mới
         target.DOShakePosition(duration, endPos, vibrato, randomness).OnComplete(() => onComplete?.Invoke());;
    }

      public static void SafeDOColor(SpriteRenderer  target,Color targetColor,float duration, Action onComplete = null)
    {
        if (target == null) return;

        // 1. NGAY LẬP TỨC Huỷ các animation đang chạy trên object này
        // complete=true: Ép animation hiện tại nhảy về đích ngay lập tức, tránh object lơ lửng.
        target.DOKill(complete: true);

        // 2. Chạy animation mới
       
         target.DOColor(targetColor,  duration).OnComplete(() => onComplete?.Invoke());;
    }

    // ========================================================================
    // GAME FEEL: Di chuyển vòng cung (Parabolic Move)
    // Dùng cho món ăn bay từ bàn (Tầng 3) lên khay Bento (Tầng 2).
    // TỐI ƯU MOBILE: Thay vì dùng DOPath đắt đỏ, ta tách biệt trục Y
    // để giả lập trọng lực. Chế độ Sequence của DOTween tự huỷ khi xong.
    // ========================================================================
    public static void ParabolicMove(Transform target, Vector3 worldEndPos, Action onComplete = null)
    {
   if (target == null) return;

        // 1. Dọn dẹp tuyệt đối các animation cũ
        target.DOKill(true);

        // 2. LẤY ĐÀ (Anticipation): Co nhẹ lại một chút trước khi vút đi
        // Giúp người chơi cảm thấy miếng đồ ăn có "sức bật"
        target.DOScale(new Vector3(0.85f, 0.85f, 0.85f), 0.05f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            // 3. HIỆU ỨNG VÚT (Stretch): Kéo dài miếng đồ ăn theo hướng bay
            // Đây là bí mật của Voodoo để tạo cảm giác tốc độ mà không cần nảy vòng cung
            target.DOScale(new Vector3(0.7f, 1.4f, 0.7f), snapDuration * 0.5f)
                  .SetLoops(2, LoopType.Yoyo)
                  .SetEase(Ease.InOutSine);

            // 4. BAY THẲNG ĐẾN ĐÍCH
            // Ease.OutBack với Amplitude thấp tạo ra một cú "hít" nhẹ khi chạm đích
            target.DOMove(worldEndPos, snapDuration)
                  .SetEase(Ease.OutQuint) 
                  .OnComplete(() =>
                  {
                      // Đảm bảo vị trí khớp 100%
                      target.position = worldEndPos;

                      // 5. VA CHẠM (Impact): Nhún bẹp xuống khi chạm khay
                      ApplyVoodooImpact(target, onComplete);
                  });
        });
    }

    private static void ApplyVoodooImpact(Transform target, Action onComplete)
    {
        // Hiệu ứng Squash & Stretch khi hạ cánh: Bẹp -> Nảy nhẹ -> Bình thường
        Sequence impactSeq = DOTween.Sequence();
        
        // Bẹp xuống (X, Z phình ra, Y co lại)
        impactSeq.Append(target.DOScale(new Vector3(1.25f, 0.75f, 1.25f), 0.1f).SetEase(Ease.OutQuad));
        
        // Nảy nhẹ lên quá đà một chút
        impactSeq.Append(target.DOScale(new Vector3(0.95f, 1.1f, 0.95f), 0.1f).SetEase(Ease.OutQuad));
        
        // Trở về mặc định (1,1,1) với hiệu ứng đàn hồi
        impactSeq.Append(target.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutElastic));

        impactSeq.OnComplete(() => {
            onComplete?.Invoke();
        });
    }

    // ========================================================================
    // EFFECT: Co giãn khi xếp vào (Pack Pop)
    // Tạo cảm giác "đã tay" (Juicy) khi món ăn khớp vào lưới Bento.
    // TỐI ƯU MOBILE: Dùng Ease.OutBack để tạo độ nảy tự nhiên mà không tốn CPU.
    // ========================================================================
    public static void PackPopEffect(Transform target,Action onComplete = null)
    {
        if (target == null) return;

        target.DOKill(complete: true); // Tránh pop chồng pop

        // Phóng to nhẹ rồi thu về kích thước chuẩn (Reset scale về 1 khi xong)
        target.DOScale(1.2f, 0.15f)
              .SetEase(Ease.OutBack)
              .OnComplete(() => {
                target.DOScale(1f, 0.1f);
                onComplete?.Invoke();
              
              });
    }

    public static void ErrorShake(Transform target,Action onComplete = null)
    {
        if (target == null) return;

        // 1. Dọn dẹp các tween cũ để tránh bị "rác" chuyển động
        target.DOKill(true);

        // 2. RUNG VỊ TRÍ (Shake Position)
        // Strength: 0.2f là vừa đủ để thấy lỗi mà không bị vỡ hình
        // Vibrato: 10 giúp cú rung dứt khoát, nhanh
      target.DOShakePosition(0.3f, 0.2f, 10, 90f, false, true)
          .OnComplete(() => onComplete?.Invoke());

        // 3. ĐỔI MÀU (Visual Feedback) - Rất quan trọng cho UX
        // Chớp đỏ nhẹ rồi quay lại trắng (Bình thường)
        SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.DOColor(Color.red, 0.15f).SetLoops(2, LoopType.Yoyo);
        }

        // 4. CO GIÃN NHẸ (Punch Scale)
        // Tạo cảm giác miếng đồ ăn đang "phản kháng"
        target.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.3f, 10, 1f);
    }

     public static void DoScale(Transform target,float endValue, float duration,Action onComplete = null)
    {
        if (target == null) return;

        target.DOKill(complete: true); // Tránh pop chồng pop

        // Phóng to nhẹ rồi thu về kích thước chuẩn (Reset scale về 1 khi xong)
        target.DOScale(endValue, duration)
              .SetEase(Ease.OutBack)
              .OnComplete(() => {
                onComplete?.Invoke();
              
              });
    }
}