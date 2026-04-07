using UnityEngine;

public class TrayOrder : MonoBehaviour
{
    //public Transform[] transformsTrayOrder;
    public FoodSlot[] foodSlots;
    public GameObject locks;
    private Vector3 originPosition;
    void Awake()
    {
       originPosition=locks.transform.localPosition;
    }

    public void SetDefault()
    {
        locks.SetActive(false);
        locks.transform.localPosition=originPosition;
    }
}
