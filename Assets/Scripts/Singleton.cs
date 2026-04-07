using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    // Tìm kiếm instance đã có trong Scene
                    _instance = (T)FindFirstObjectByType(typeof(T));

                    if (_instance == null)
                    {
                        // Nếu chưa có, tạo một GameObject mới để chứa nó
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = typeof(T).ToString() + " (Singleton)";

                        // Giữ lại khi chuyển Scene (Tùy chọn)
                       // DontDestroyOnLoad(singleton);
                    }
                }
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            // Nếu bạn muốn Singleton tồn tại xuyên suốt các Scene:
            // DontDestroyOnLoad(gameObject); 
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }
}