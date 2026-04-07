using UnityEngine;

// Script này giúp điều khiển vùng nào được bấm, vùng nào không
public class UnmaskRaycastFilter : MonoBehaviour, ICanvasRaycastFilter
{
    [HideInInspector] public Vector2 holeScreenPos;
    [HideInInspector] public float holeRadius;
    
    // Biến này để bạn bật/tắt nhanh chế độ chặn từ Inspector
    public bool isEnabled = true; 

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        float dist = Vector2.Distance(sp, holeScreenPos);
    
    if (dist < holeRadius) {
        return false; 
    }

    return true;
    }
}