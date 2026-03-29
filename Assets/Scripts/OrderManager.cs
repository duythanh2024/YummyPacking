using System;
using System.Collections.Generic;
using UnityEngine;
//Quản lý Đơn hàng Tầng 1
public class OrderManager : MonoBehaviour
{
    public int maxActiveOrders = 3; // Số lượng đơn tối đa hiển thị cùng lúc trên Tầng 1

    public List<OrderData> activeOrders = new List<OrderData>(); // Danh sách 3 đơn đang nằm trên màn hình
    private Queue<OrderData> orderQueue = new Queue<OrderData>();  // HÀNG ĐỢI: Chứa các đơn còn lại của màn chơi
    public GameObject orderPrefab;     // Kéo Prefab OrderUIElement vào đây
    public Transform uiContainer;
    private List<OrderUIElement> spawnedUI = new List<OrderUIElement>();
    public void InitializeOrders(List<OrderData> levelOrders)
    {

        // foreach (var ui in spawnedUI) Destroy(ui.gameObject);
        spawnedUI.Clear();
        activeOrders.Clear();
        orderQueue = new Queue<OrderData>(levelOrders);
        RefreshOrders();

    }
    private void RefreshOrders()
    {
        while (activeOrders.Count < maxActiveOrders && orderQueue.Count > 0)
        {
            OrderData data = orderQueue.Dequeue();
            activeOrders.Add(data);
            // GameObject newGo = Instantiate(orderPrefab, uiContainer);

            GameObject newGo = ObjectPooler.Instance.SpawnFromPool("Order", Vector3.zero, Quaternion.identity);
            newGo.transform.SetParent(uiContainer);

            OrderUIElement uiElem = newGo.GetComponent<OrderUIElement>();
            uiElem.SetupUI(data);
            spawnedUI.Add(uiElem);
        }
    }

    public OrderData GetCurrentActiveOrder()
    {
        if (activeOrders != null && activeOrders.Count > 0)
        {
            // Trả về đơn hàng đầu tiên (ưu tiên xử lý từ trái qua phải)
            return activeOrders[0];
        }

        Debug.LogWarning("Không tìm thấy đơn hàng active nào!");
        return null;
    }

    // Hàm xử lý việc rút đơn từ hàng đợi lên màn hình
    private void PopNextOrder()
    {
        // Kiểm tra xem hàng đợi còn đơn không VÀ trên màn hình còn chỗ trống không (Slot A, B, C)
        if (orderQueue.Count > 0 && activeOrders.Count < maxActiveOrders)
        {
            OrderData nextOrder = orderQueue.Dequeue(); // Lấy đơn ra khỏi Hàng đợi
            activeOrders.Add(nextOrder);                // Đưa vào danh sách đang hiển thị

            // Bắn Event để UI Controller sinh ra (Spawn) thẻ Đơn hàng mới
            // GameEvents.TriggerOrderSpawned(nextOrder);
        }
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
            BentoTweenHelper.DoScale(uiToRemove.completedOverlay.transform, 1.2f, 1.0f, () =>
            {
                BentoTweenHelper.DoScale(uiToRemove.transform, 0, 0.5f, () =>
                {
                   

                    ObjectPooler.Instance.ReturnToPool("Order",uiToRemove.gameObject);
                    // 2. Xóa khỏi danh sách quản lý
                    spawnedUI.RemoveAt(index);
                    activeOrders.RemoveAt(index);
                    Debug.Log("RefreshOrders CompleteOrder " + activeOrders.Count + ": " + orderQueue.Count);
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