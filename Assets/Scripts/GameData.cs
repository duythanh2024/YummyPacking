using UnityEngine;

public static class GameData
{
    //GIA CA
    public const int untoBootPrice = 100;
    public const int shuffBootPrice = 150;
    public const int hammerBootPrice = 500;
    public const int slot1Price = 500;
    public const int slot2Price = 1000;
    public const int slot3Price = 2500;
    public const int easyRewardLevel = 15; //sl order: 1,2
    public const int normalRewardLevel = 25; //sl order: 3,4
    public const int normalHardLevel = 50; //sl order: 5,6
    public const int veryHardLevel = 120; //sl order: 7,8,0,10

    private const string KEY_HAMER_TUTORIAL = "hammerBoostTutorial";
    private const string KEY_UNDO_TUTORIAL = "undoBoostTutorial";
    private const string KEY_SWAP_TUTORIAL = "swapBoostTutorial";
    private const string KEY_LEVEL = "SavedLevelIndex";
    private const string KEY_UNDO = "undoNumber";
    private const string KEY_SWAP = "swapNumber";
    private const string KEY_HAMMER = "hammerNumber";
    private const string KEY_COIN = "coinNumber";
    private const string KEY_HEART = "heartNumber";
    private const string KEY_STAR = "starNumber";

    private const string KEY_SLOT1 = "slot1Number";
    private const string KEY_SLOT2 = "slot2Number";
    private const string KEY_SLOT3 = "slot3Number";

    private const string KEY_SOUND = "Settings_Sound";
    private const string KEY_MUSIC = "Settings_Music";
    private const string KEY_VIBRATE = "Settings_Vibrate";

    public static int GetRewardLevel(int level)
    {
        if (level == 1 || level == 2)
            return easyRewardLevel;
        else if (level == 3 || level == 4)
            return normalRewardLevel;
        else if (level == 5 || level == 6)
            return normalHardLevel;
        else
            return veryHardLevel;

    }

    public static bool Slot1
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_SLOT1, 0) == 1;
        set => PlayerPrefs.SetInt(KEY_SLOT1, value ? 1 : 0);
    }
    public static bool Slot2
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_SLOT2, 0) == 1;
        set => PlayerPrefs.SetInt(KEY_SLOT2, value ? 1 : 0);
    }
    public static bool Slot3
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_SLOT3, 0) == 1;
        set => PlayerPrefs.SetInt(KEY_SLOT3, value ? 1 : 0);
    }

    public static int Coins
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_COIN, 10000);
        set => PlayerPrefs.SetInt(KEY_COIN, value);
    }
    public static int Heart
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_HEART, 5);
        set => PlayerPrefs.SetInt(KEY_HEART, value);
    }
    public static int Stars
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_STAR, 5);
        set => PlayerPrefs.SetInt(KEY_STAR, value);
    }
    public static int UndoNumber
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_UNDO, 10);
        set => PlayerPrefs.SetInt(KEY_UNDO, value);
    }

    public static int SwapNumber
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_SWAP, 10);
        set => PlayerPrefs.SetInt(KEY_SWAP, value);
    }

    public static int HammerNumber
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_HAMMER, 10);
        set => PlayerPrefs.SetInt(KEY_HAMMER, value);
    }

    public static int SavedLevelIndex
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_LEVEL, 0);
        set => PlayerPrefs.SetInt(KEY_LEVEL, value);
    }

    public static bool HammerBoostTutorial
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_HAMER_TUTORIAL, 0) == 1;
        set => PlayerPrefs.SetInt(KEY_HAMER_TUTORIAL, value ? 1 : 0);
    }
    public static bool UndoBoostTutorial
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_UNDO_TUTORIAL, 0) == 1;
        set => PlayerPrefs.SetInt(KEY_UNDO_TUTORIAL, value ? 1 : 0);
    }
    public static bool SwapBoostTutorial
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_SWAP_TUTORIAL, 0) == 1;
        set => PlayerPrefs.SetInt(KEY_SWAP_TUTORIAL, value ? 1 : 0);
    }
    public static bool IsSoundOn
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_SOUND, 0) == 1;
        set => PlayerPrefs.SetInt(KEY_SOUND, value ? 1 : 0);
    }

    public static bool IsMUsicOn
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_MUSIC, 0) == 1;
        set => PlayerPrefs.SetInt(KEY_MUSIC, value ? 1 : 0);
    }

    public static bool IsVibrateOn
    {
        // PlayerPrefs không hỗ trợ bool, ta dùng 0 (false) và 1 (true)
        get => PlayerPrefs.GetInt(KEY_VIBRATE, 1) == 1;
        set => PlayerPrefs.SetInt(KEY_VIBRATE, value ? 1 : 0);
    }
    public static void Save()
    {
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Xóa toàn bộ dữ liệu (Dùng khi debug hoặc Reset Game)
    /// </summary>
    public static void ResetAll()
    {
        PlayerPrefs.DeleteAll();
    }
}
