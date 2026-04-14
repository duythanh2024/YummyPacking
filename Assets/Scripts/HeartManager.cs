using UnityEngine;
using System;
using System.Collections;

public class HeartManager : Singleton<HeartManager>
{


    [Header("Cấu hình")]
    public int maxHearts = 5;
    public int recoveryTimeSeconds =1800; // 30 giây hồi 1 tim

    // Sự kiện để UI tự cập nhật (Observer Pattern)
    public event Action<string, float> OnTimerTick; // string: Text hiển thị, float: fill amount thanh bar
    public event Action<int> OnHeartChanged;       // int: số tim hiện tại

    private int currentHearts;
    private DateTime nextHeartTime;
    private DateTime infiniteEndTime;

    // Key lưu data
    private const string PREF_HEARTS = "fz_hearts";
    private const string PREF_NEXT_TIME = "fz_next_time";
    private const string PREF_INF_TIME = "fz_inf_time";

    void Start()
    {
       
        LoadData();

        // Bắt đầu đếm ngược (Chỉ chạy 1 lần mỗi giây - Rất nhẹ)
        StartCoroutine(CountDownCoroutine());
    }

    // Tự động đồng bộ khi người dùng quay lại game từ background
    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus) // Khi quay lại game (Resume)
        {
            CalculateOfflineRecovery();
        }
        else // Khi ẩn game (Pause)
        {
            SaveData(); // Chỉ lưu data khi người chơi thoát ra
        }
    }

    void OnApplicationQuit()
    {
        SaveData();
    }

    // --- LOGIC TÍNH TOÁN ---

    private void CalculateOfflineRecovery()
    {
        if (currentHearts >= maxHearts) return;

        DateTime now = DateTime.UtcNow;
        if (now >= nextHeartTime)
        {
            // Tính số tim đã hồi khi tắt máy
            double secondsPassed = (now - nextHeartTime).TotalSeconds;
            int recovered = 1 + (int)(secondsPassed / recoveryTimeSeconds);

            currentHearts = Mathf.Min(currentHearts + recovered, maxHearts);

            // Cập nhật mốc thời gian tiếp theo
            if (currentHearts < maxHearts)
            {
                double remain = secondsPassed % recoveryTimeSeconds;
                nextHeartTime = now.AddSeconds(recoveryTimeSeconds - remain);
            }

            // Báo cho UI biết số tim mới
            OnHeartChanged?.Invoke(currentHearts);
        }
    }

    IEnumerator CountDownCoroutine()
    {
        var wait = new WaitForSecondsRealtime(1f); // Dùng Realtime để không bị ảnh hưởng khi Pause game

        while (true)
        {
            DateTime now = DateTime.UtcNow;

            // 1. Kiểm tra Vô hạn trước (ƯU TIÊN)
            if (now < infiniteEndTime)
            {
                TimeSpan span = infiniteEndTime - now;
                string text = string.Format("∞ {0:D2}:{1:D2}", (int)span.TotalMinutes, span.Seconds);
                OnTimerTick?.Invoke(text, 1f); // Bắn sự kiện cập nhật UI
            }
            // 2. Kiểm tra hồi tim
            else
            {
                CalculateOfflineRecovery(); // Tính toán lại xem có được cộng tim chưa

                if (currentHearts < maxHearts)
                {
                    TimeSpan span = nextHeartTime - now;
                    // Đảm bảo không hiển thị số âm
                    if (span.TotalSeconds < 0) span = TimeSpan.Zero;

                    string text = string.Format("{0:D2}:{1:D2}", span.Minutes, span.Seconds);
                    float fill = 1f - (float)(span.TotalSeconds / recoveryTimeSeconds);

                    OnTimerTick?.Invoke(text, fill);
                }
                else
                {
                    OnTimerTick?.Invoke("FULL", 1f);
                }
            }

            yield return wait; // Đợi 1 giây thực
        }
    }

    // --- CÁC HÀM GỌI TỪ NGOÀI ---

    public bool UseHeart()
    {
        bool isHesrt=false;
        // Nếu còn vô hạn thì không trừ
        if (DateTime.UtcNow < infiniteEndTime) return true;

        if (currentHearts > 0)
        {
            if (currentHearts == maxHearts)
            {
                // Nếu đang đầy mà dùng, bắt đầu tính giờ hồi phục
                nextHeartTime = DateTime.UtcNow.AddSeconds(recoveryTimeSeconds);
            }

            currentHearts--;
            OnHeartChanged?.Invoke(currentHearts);
            SaveData();
            isHesrt=true;
        }
        return isHesrt;
       
    }

    public void AddInfiniteTime(int minutes)
    {
        DateTime now = DateTime.UtcNow;
        if (now < infiniteEndTime)
            infiniteEndTime = infiniteEndTime.AddMinutes(minutes);
        else
            infiniteEndTime = now.AddMinutes(minutes);

        // Gọi 1 lần để UI cập nhật ngay lập tức
        OnTimerTick?.Invoke("∞ Updating...", 1f);
        SaveData();
    }

    public int GetCurrentHearts() => currentHearts;

    // --- LƯU TRỮ (Dùng Binary để chính xác tuyệt đối) ---

    private void SaveData()
    {
        GameData.Heart = currentHearts;
        GameData.Save();

        PlayerPrefs.SetString(PREF_NEXT_TIME, nextHeartTime.ToBinary().ToString());
        PlayerPrefs.SetString(PREF_INF_TIME, infiniteEndTime.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        currentHearts = GameData.Heart;

//        Debug.Log(currentHearts);

        // Load Time an toàn
        long nextBin = Convert.ToInt64(PlayerPrefs.GetString(PREF_NEXT_TIME, DateTime.UtcNow.ToBinary().ToString()));
        nextHeartTime = DateTime.FromBinary(nextBin);

        long infBin = Convert.ToInt64(PlayerPrefs.GetString(PREF_INF_TIME, DateTime.UtcNow.AddSeconds(-1).ToBinary().ToString()));
        infiniteEndTime = DateTime.FromBinary(infBin);

        // Cập nhật lại ngay khi load xong để tránh hiển thị sai
        CalculateOfflineRecovery();
        OnHeartChanged?.Invoke(currentHearts);
    }
}