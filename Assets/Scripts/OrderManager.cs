using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
//Quản lý Đơn hàng Tầng 1
public class OrderManager : MonoBehaviour
{
    public int maxActiveOrders = 1; // Số lượng đơn tối đa hiển thị cùng lúc trên Tầng 1

    public List<OrderData> activeOrders = new List<OrderData>(); // Danh sách  đơn đang nằm trên màn hình
    private Queue<OrderData> orderQueue = new Queue<OrderData>();  // HÀNG ĐỢI: Chứa các đơn còn lại của màn chơi
    // public GameObject orderPrefab;     // Kéo Prefab OrderUIElement vào đây
    public Transform uiContainer;
    private List<OrderUIElement> spawnedUI = new List<OrderUIElement>();
    private int numberOrder = 0;
    private int activeOrder = 0;
    public string CurrentPoolTag { get; set; } // Lưu lại tag khi được spawn
    public void InitializeOrders(List<OrderData> levelOrders)
    {


        // Chỉ cần 1 vòng lặp duy nhất
        foreach (var ui in spawnedUI)
        {
            // Trả về đúng pool dựa trên tag đã lưu khi khởi tạo
            if (ui != null && !string.IsNullOrEmpty(ui.CurrentPoolTag))
            {
                ObjectPooler.Instance.ReturnToPool(ui.CurrentPoolTag, ui.gameObject);
            }
        }

        spawnedUI.Clear();
        activeOrders.Clear();
        orderQueue = new Queue<OrderData>(levelOrders);
        numberOrder = orderQueue.Count;
        activeOrder = 0;
        RefreshOrders();

    }
    public string GetTagTray(TypeTray typeTray)
    {
        switch (typeTray)
        {
            case TypeTray.Tray23:
                return PoolTags.Order23;

            case TypeTray.Tray33:
                return PoolTags.Order33;

            case TypeTray.TrayHex:
                return PoolTags.Order66;

            default:
                // Log cảnh báo để CEO/CTO như bạn dễ dàng debug khi mở rộng studio
                Debug.LogWarning($"[GetTagTray] Type {typeTray} chưa được cấu hình Tag. Trả về mặc định Order23.");
                return PoolTags.Order23;
        }
    }
    private void RefreshOrders()
    {


        GameManager.Instance.ShowOrderActive(activeOrder, numberOrder);
        while (activeOrders.Count < maxActiveOrders && orderQueue.Count > 0)
        {
            OrderData data = orderQueue.Dequeue();
            activeOrders.Add(data);

            string tag = GetTagTray(data.typeOfTray);
            GameObject newGo = ObjectPooler.Instance.SpawnFromPool(tag, Vector3.zero, Quaternion.identity);

            newGo.transform.SetParent(uiContainer);
            OrderUIElement uiElem = newGo.GetComponent<OrderUIElement>();
            uiElem.ClearOldData();
            // QUAN TRỌNG: Gán tag để tí nữa biết đường mà thu hồi
            uiElem.CurrentPoolTag = tag;

            uiElem.SetupUI(data);
            spawnedUI.Add(uiElem);
            activeOrder++;
        }

    }

    public int GetPerCent()
    {

        if ((activeOrder - 1) <= 0)
            return 0;
        return 100 * (activeOrder - 1) / numberOrder;

    }

    public OrderData GetCurrentActiveOrder()
    {
        if (activeOrders != null && activeOrders.Count > 0)
        {
            // Trả về đơn hàng đầu tiên (ưu tiên xử lý từ trái qua phải)
            return activeOrders[0];
        }

        // Debug.LogWarning("Không tìm thấy đơn hàng active nào!");
        return null;
    }

    // Hàm gọi khi Lưới Bento đã xếp đủ đồ ăn cho một đơn
    public void CompleteOrder(OrderData completedOrder, Action onComplete = null)
    {

        int index = activeOrders.IndexOf(completedOrder);
        if (index != -1)
        {
            // 1. Hiệu ứng biến mất cho UI tương ứng
            OrderUIElement uiToRemove = spawnedUI[index];
            uiToRemove.completedOverlay.transform.localScale = Vector3.zero;
            uiToRemove.completedOverlay.SetActive(true);
            AudioManager.Instance.Play("Reward");
            BentoTweenHelper.DoScale(uiToRemove.completedOverlay.transform, 1.2f, 1.0f, () =>
            {
                BentoTweenHelper.DoScale(uiToRemove.transform, 0, 0.5f, () =>
                {


                    ObjectPooler.Instance.ReturnToPool(uiToRemove.CurrentPoolTag, uiToRemove.gameObject);
                    // 2. Xóa khỏi danh sách quản lý
                    spawnedUI.RemoveAt(index);
                    activeOrders.RemoveAt(index);
                    //                    Debug.Log("RefreshOrders CompleteOrder " + activeOrders.Count + ": " + orderQueue.Count);
                    // 3. Nạp đơn mới ngay lập tức
                    RefreshOrders();
                    onComplete?.Invoke();

                });

            });

        }
    }
    // --- ĐÂY LÀ HÀM BẠN ĐANG THIẾU ---
    public bool IsQueueEmpty()
    {
        // Kiểm tra xem hàng đợi có trống không
        return orderQueue == null || orderQueue.Count == 0;
    }

}