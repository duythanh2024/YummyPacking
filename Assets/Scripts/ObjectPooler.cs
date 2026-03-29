using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PoolItem
{
    public string tag;          // Tên định danh (VD: "Coin", "Tray")
    public GameObject prefab;   // Prefab tương ứng
    public int size;            // Số lượng khởi tạo ban đầu
    public bool expandable;     // Có cho phép tự động nở thêm khi hết không?
}
public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    public List<PoolItem> itemsToPool;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializePool();
    }

    void InitializePool()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (PoolItem item in itemsToPool)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < item.size; i++)
            {
                GameObject obj = Instantiate(item.prefab);
                obj.SetActive(false);
                // obj.transform.parent=transform;
                obj.transform.SetParent(transform, false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(item.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            //   Debug.LogWarning("Pool với tag " + tag + " không tồn tại!");
            return null;
        }

        GameObject objectToSpawn;

        // Kiểm tra nếu hết object trong queue
        if (poolDictionary[tag].Count == 0)
        {
            PoolItem itemInfo = itemsToPool.Find(x => x.tag == tag);
            if (itemInfo != null && itemInfo.expandable)
            {
                objectToSpawn = Instantiate(itemInfo.prefab);
            }
            else return null;
        }
        else
        {
            objectToSpawn = poolDictionary[tag].Dequeue();
        }

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        // 1. Kiểm tra an toàn trước khi xử lý
        if (!poolDictionary.ContainsKey(tag))
        {
            //   Debug.LogError($"Pool với tag {tag} không tồn tại! Đang phá hủy object để tránh rác.");
            Destroy(obj);
            return;
        }

        // 2. Dọn dẹp trạng thái (Cực kỳ quan trọng)
        // Nếu object có dùng Interface reset, hãy gọi nó hoặc chủ động reset tại đây
        obj.SetActive(false);

        // 3. Tối ưu SetParent: 
        // Chỉ đưa về Pool cha nếu nó đang bị "lạc trôi" làm con của object khác (như Tray)
        if (obj.transform.parent != transform)
        {
            obj.transform.SetParent(transform);
        }

        // 4. Đưa vào hàng đợi
        poolDictionary[tag].Enqueue(obj);
    }
}
