using System.Collections.Generic;
using UnityEngine;

public class TrayOrder : MonoBehaviour
{
    //public Transform[] transformsTrayOrder;
    public FoodSlot[] foodSlots;
    public GameObject locks;
    private Vector3 originPosition;
    public string CurrentPoolTag { get; set; } // Lưu lại tag khi được spawn
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
 public void GetRewardInSlot(TypeRewardSlot typeRewardSlot,Sprite sprite,int[] slotInTrays)
    {
        Debug.Log("GetRewardInSlot");
       for (int i = 0; i < foodSlots.Length; i++)
        {
             foodSlots[i].gameObject.transform.GetChild(0).gameObject.SetActive(false);
             for (int j = 0; j < slotInTrays.Length; j++)
            {
                if(foodSlots[i].gameObject.transform.childCount > 0)
                {
                    if (i == slotInTrays[j])
                    {
                         foodSlots[i].gameObject.transform.GetChild(0).gameObject.SetActive(true);
                         foodSlots[i].gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite=sprite;
                         foodSlots[i].typeRewardSlot=typeRewardSlot;
                    }
                }
            }
            
        }
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
            if(foodSlots[i].gameObject.transform.childCount > 0)
            {
                foodSlots[i].gameObject.transform.GetChild(0).gameObject.SetActive(false);
            }
            foodSlots[i].typeRewardSlot=TypeRewardSlot.None;
           // foodSlots[i].gameObject.transform.GetChild(0).gameObject.SetActive(false);
            // Quan trọng: Đánh dấu ScriptableObject đã thay đổi để Unity lưu lại (nếu chạy trong Editor)
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(currentOrder);
#endif

        }
    }
}
