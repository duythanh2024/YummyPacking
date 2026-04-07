using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using VIB;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClip[] audioClips;
    public AudioClip homeMusic;
    public AudioClip gameplayMusic;
    [SerializeField] private AudioSource musicSource = null;
    [SerializeField] private AudioSource sfxSourcePrefab = null;

    // DANH SÁCH QUẢN LÝ CÁC NGUỒN PHÁT (POOL)
    private List<AudioSource> sfxPool = new List<AudioSource>();
    private GameObject sfxParent; // GameObject cha để gom gọn các AudioSource trong Hierarchy

    private bool isVibrateOn;
    private bool isSoundOn; // Cache lại biến này để check nhanh
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);

        // Tạo vật chứa để Hierarchy gọn gàng
        sfxParent = new GameObject("SFX_Pool_Container");
        sfxParent.transform.SetParent(this.transform);

        // Khởi tạo trước 5 nguồn phát để tránh lag khi game bắt đầu
        for (int i = 0; i < 5; i++)
        {
            CreateNewSource();
        }
    }
    /// <summary>
    /// Gọi hàm này NGAY TRƯỚC khi gọi lệnh Show Ad của SDK
    /// </summary>
    public void PauseAllAudioForAds()
    {
        // Tắt toàn bộ âm thanh ở tầng hệ thống (Global)
        // Cách này an toàn nhất, tránh xung đột với trình phát Video của Ad
        AudioListener.pause = true;

        // Tạm dừng cả MusicSource để tiết kiệm tài nguyên
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
        Debug.Log("AudioManager: Audio paused for Ad");
    }
    /// <summary>
    /// Gọi hàm này khi Ad đóng (OnAdClosed)
    /// </summary>
    public void ResumeAllAudioAfterAds()
    {
        // Mở lại tầng hệ thống
        AudioListener.pause = false;

        // Load lại setting để đảm bảo nhạc/âm thanh tuân thủ đúng lựa chọn của User
        LoadSettings();

        // Nếu nhạc đang bị Pause thì Resume lại
        if (musicSource != null && !musicSource.mute)
        {
            musicSource.UnPause();
        }
        Debug.Log("AudioManager: Audio resumed after Ad");
    }
    // Tự động xử lý khi người dùng thoát game tạm thời (bấm Home/Nhận cuộc gọi)
    private void OnApplicationPause(bool pauseStatus)
    {
        // Nếu pauseStatus = true (game bị đẩy xuống nền), ta nên pause Audio
        AudioListener.pause = pauseStatus;
    }
    // Hàm tạo mới một AudioSource và thêm vào Pool
    private AudioSource CreateNewSource()
    {
        GameObject newObj = new GameObject("SFX_Source_" + sfxPool.Count);
        newObj.transform.SetParent(sfxParent.transform);

        AudioSource newSource = newObj.AddComponent<AudioSource>();
        newSource.playOnAwake = false;

        // Copy setting từ prefab mẫu nếu có (ví dụ Spatial Blend, Pitch...)
        if (sfxSourcePrefab != null)
        {
            newSource.spatialBlend = sfxSourcePrefab.spatialBlend;
            newSource.pitch = sfxSourcePrefab.pitch;
        }

        sfxPool.Add(newSource);
        return newSource;
    }
    // Hàm tìm kiếm AudioSource đang rảnh
    private AudioSource GetAvailableSource()
    {
        // 1. Tìm trong list xem có cái nào không playing không
        foreach (var source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // 2. Nếu tất cả đều bận, tạo cái mới
        return CreateNewSource();
    }
    void Start()
    {
        LoadSettings();
    }

    public void PlayBackground(AudioClip clip)
    {
        if (musicSource.clip == clip) return; // Nếu đang phát đúng bài đó rồi thì không phát lại

        try
        {
            musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
        catch
        {

        }

    }

    public void LoadSettings()
    {
        isSoundOn = GameData.IsSoundOn;
        isVibrateOn = GameData.IsVibrateOn;
        musicSource.mute = GameData.IsMUsicOn;
    }

    public void Play(string name)
    {
        try
        {

            // Nếu tắt tiếng thì không cần chạy logic tìm clip làm gì cho tốn hiệu năng
            if (isSoundOn) return;
            AudioClip clip = Array.Find(audioClips, sound => sound.name == name);

            if (clip == null)
            {
                return;
            }

            StartCoroutine(PlaySfxRoutine(clip, name));
        }
        catch
        {

        }

    }

    private IEnumerator PlaySfxRoutine(AudioClip clip, string name)
    {
        // 1. Đảm bảo load xong (An toàn cho WebGL)
        if (clip.loadState != AudioDataLoadState.Loaded)
        {
            clip.LoadAudioData();
            while (clip.loadState != AudioDataLoadState.Loaded)
            {
                yield return null;
            }
        }

        // 2. Lấy một nguồn phát riêng biệt từ Pool
        AudioSource source = GetAvailableSource();

        // 3. Cấu hình cho nguồn phát này
        source.clip = clip;
       // source.mute = GameData.IsSoundOn; // Đảm bảo mute đúng setting hiện tại

        // Xử lý Volume riêng biệt (Logic của bạn)
        // Vì mỗi âm thanh dùng source riêng, nên set volume ở đây không ảnh hưởng âm thanh khác
        if (name == "Coin")
            source.volume = 0.2f;
        else
            source.volume = 1.0f;

        // 4. Phát (Dùng Play() thay vì PlayOneShot để có toàn quyền kiểm soát)
        source.Play();
    }
    public void TriggerVibrate(int number)
    {
        if (!isVibrateOn) return;
        Vibration.Vibrate(number);
        // // Rung mặc định của Unity (0.5 giây trên Android/iOS)
        // #if UNITY_ANDROID || UNITY_IOS
        //      Vibration.Vibrate(50);
        // #endif
    }
}