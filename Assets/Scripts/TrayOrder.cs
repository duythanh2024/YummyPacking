using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class TrayOrder : MonoBehaviour
{
    //public Transform[] transformsTrayOrder;
    public FoodSlot[] foodSlots;
    public GameObject locks;
    private Vector3 originPosition;
    void Awake()
    {
        originPosition = locks.transform.localPosition;
    }

    public void SetDefault()
    {
        locks.SetActive(false);
        locks.transform.localPosition = originPosition;
        SetPosition();
    }

    public void SetPosition()
    {
        OrderData currentOrder = GameManager.Instance.orderCtrl.GetCurrentActiveOrder();
        List<FoodPlacement> requiredLayout = currentOrder.requiredLayout;
        for (int i = 0; i < foodSlots.Length; i++)
        {

           Vector2 newPos = new Vector2(
            foodSlots[i].anchorPoint.localPosition.x,
            foodSlots[i].anchorPoint.localPosition.y
        );
           requiredLayout[i].gridValues = newPos;
// Quan trọng: Đánh dấu ScriptableObject đã thay đổi để Unity lưu lại (nếu chạy trong Editor)
#if UNITY_EDITOR
    UnityEditor.EditorUtility.SetDirty(currentOrder);
#endif

        }
    }
}
