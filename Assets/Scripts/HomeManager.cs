
//using MobileMonetizationPro;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeManager : Singleton<HomeManager>
{
   // public ShopFoodManager shopFoodManager;
    public Sprite zoomMenu;
    public Sprite normalMenu;
    public GameObject[] screens;
    public Image[] bottomImg;
    //public GameObject Groups;

    [Header("Text")]
    public TextMeshProUGUI Txt_Coins;
    public TextMeshProUGUI Txt_Stars;
    //public Button Btn_Store;
   // public GameObject pnl_ShowDislog;
    public GameObject pnl_Show_Coin_Reward;
    bool isPresss;
    public GameObject pnl_dev;
    public TMP_InputField inputField;
    //Deco
    public Button Btn_Arena;
    void Start()
    {
        isPresss = false;
       // ShowLoading(false);
//         Btn_Store.gameObject.SetActive(false);
// #if UNITY_IOS
//         Btn_Store.gameObject.SetActive(true);
// #endif
        ShowInfoCoins();
        LoadScreen(1);
        AudioManager.Instance.PlayBackground(AudioManager.Instance.homeMusic);
    }
    public void ShowInfoCoins()
    {
        Txt_Coins.text = GameData.Coins.ToKMB().ToString();
        //Txt_Heart.text = GameData.Heart.ToString();
        Txt_Stars.text = GameData.Stars.ToString();
    }

    public void LoadGame()
    {
        if (isPresss)
            return;

        isPresss = true;

       // ShowLoading();

        AudioManager.Instance.Play("Click");

        if (HeartManager.Instance.UseHeart())
        {
            SceneManager.LoadSceneAsync("Game");
            isPresss = false;

        }
        else
        {
            //ShowLoading(false);
            isPresss = false;
            ToastManager.Instance.ShowToast("Out of hearts");

        }


    }

    // public void ShowLoading(bool display = true)
    // {
    //     pnl_ShowDislog.SetActive(display);
    // }

    public void LoadTest()
    {
        if (isPresss)
            return;
        isPresss = true;
        string text = inputField.text;
        int index = 1;
        if (text.Trim().Length > 0)
        {
            index = int.Parse(text);
        }
        pnl_dev.SetActive(false);
        //DebugLog.WriteLog("index " + index);
        PlayerPrefs.SetInt("Level", index);
        AudioManager.Instance.Play("Click");

        SceneManager.LoadSceneAsync("Game");
        isPresss = false;
    }

    public void OnClickTab(int index)
    {
        AudioManager.Instance.Play("Click");
        LoadScreen(index);


    }

    public void LoadScreen(int index)
    {


        foreach (GameObject obj in screens)
        {
            obj.SetActive(false);

        }
        foreach (Image img in bottomImg)
        {
            img.sprite = normalMenu;
            img.SetNativeSize();
            RectTransform imgBottomChild = img.transform.GetChild(0).GetComponent<RectTransform>();
            imgBottomChild.anchoredPosition = new Vector2(imgBottomChild.anchoredPosition.x, 50);
            RectTransform txtBottomChild = img.transform.GetChild(1).GetComponent<RectTransform>();
            txtBottomChild.anchoredPosition = new Vector2(txtBottomChild.anchoredPosition.x, 34);

        }
        bottomImg[index].sprite = zoomMenu;
        bottomImg[index].SetNativeSize();
        RectTransform imgChild = bottomImg[index].transform.GetChild(0).GetComponent<RectTransform>();
        imgChild.anchoredPosition = new Vector2(imgChild.anchoredPosition.x, 80);

        RectTransform txtChild = bottomImg[index].transform.GetChild(1).GetComponent<RectTransform>();
        txtChild.anchoredPosition = new Vector2(imgChild.anchoredPosition.x, 70);

        screens[index].SetActive(true);





    }

    public void ShowAudioClick()
    {
        AudioManager.Instance.Play("Click");
    }

    public void OpenLink(int type)
    {
        AudioManager.Instance.Play("Click");
        string url = "";
        if (type == 1)
        {
            url = Utilities.policy_url;
        }
        else
        {
            url = Utilities.term_url;
        }

        // Đảm bảo có prefix https để tránh lỗi trên một số trình duyệt mobile
        if (!url.StartsWith("http"))
        {
            url = "https://" + url;
        }

        try
        {
            Application.OpenURL(url);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Không thể mở liên kết: " + e.Message);
        }
    }
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         GameData.Stars+=100;
    //     }
    // }


    /// <summary>
    /// Sau khi mua goi remove ads
    /// </summary>
    // public void RemoveAds()
    // {

    //     ShowLoading(false);
    //     AudioManager.Instance.Play("Reward");
    //     GameData.NoAds = true;
    //     GameData.AdsRemoved = true;
    //     shopFoodManager.CheckAds();
    //     shopFoodManager.Show_Reward(1, GameData.removeAdstId, 0, 0, 0, true, false);//ads
    // }
    // public void RemoveAdsProducts()
    // {
    //     ShowLoading(false);
    //     AudioManager.Instance.Play("Reward");
    //     GameData.ProductNoAds = true;
    //     GameData.AdsRemoved = true;
    //     AddCoinBonus(GameData.adRemovalBonusCoin);
    //     AddHints(GameData.hintadRemovalProduct);
    //     shopFoodManager.CheckAds();
    //     UIManager.Instance.ShowCoin();
    //     shopFoodManager.Show_Reward(2, GameData.removeAdsProductId, GameData.adRemovalBonusCoin, GameData.hintadRemovalProduct, 0, true, false);//ads

    // }
    // /// <summary>
    // /// WellCome
    // /// </summary>
    // /// <param name="number"></param>
    // public void GetReWardBun(int type)
    // {

    //     DebugLog.WriteLog("Type " + type);
    //     DebugLog.WriteLog("tat SHow Loding");
    //     ShowLoading(false);
    //     AudioManager.Instance.Play("Reward");
    //     if (type == 1)
    //     {
    //         GameData.WelcomeBundle = true;
    //         GameData.Save();
    //         GameData.coinReward = GameData.welcomeBundleCoin;
    //         GameData.hints = GameData.hintwelcomeBundle;
    //         AddCoinBonus(GameData.welcomeBundleCoin);
    //         AddHints(GameData.hintwelcomeBundle);
    //         UIManager.Instance.ShowCoin();
    //         shopFoodManager.Show_Reward(2, GameData.welcomeBundId, GameData.coinReward, GameData.hints, GameData.timeCount, GameData.ads, GameData.timeCount > 0);//ads

    //     }
    //     else if (type == 2)
    //     {
    //         GameData.coinReward = GameData.tastyBundleCoin;
    //         GameData.hints = GameData.hinttastyBundle;
    //         AddCoinBonus(GameData.tastyBundleCoin);
    //         AddHints(GameData.hinttastyBundle);
    //         UIManager.Instance.ShowCoin();
    //         shopFoodManager.Show_Reward(2, GameData.tastyBundId, GameData.coinReward, GameData.hints, GameData.timeCount, GameData.ads, GameData.timeCount > 0);//ads

    //     }
    //     else if (type == 3)
    //     {
    //         GameData.coinReward = GameData.deliciousBundleCoin;
    //         GameData.hints = GameData.hintdeliciousBundle;
    //         AddCoinBonus(GameData.deliciousBundleCoin);
    //         AddHints(GameData.hintdeliciousBundle);
    //         UIManager.Instance.ShowCoin();
    //         shopFoodManager.Show_Reward(2, GameData.deliciousBundId, GameData.coinReward, GameData.hints, GameData.timeCount, GameData.ads, GameData.timeCount > 0);//ads

    //     }

    //     else if (type == 4)
    //     {
    //         GameData.WonderBundle = true;
    //         GameData.timeCount = 6;
    //         GameData.ads = true;
    //         GameData.AdsRemoved = true;
    //         GameData.coinReward = GameData.wonderfulBundleCoin;
    //         GameData.hints = GameData.hintwonderfulBundle;
    //         AddCoinBonus(GameData.wonderfulBundleCoin);
    //         AddHints(GameData.hintwonderfulBundle);
    //         UIManager.Instance.ShowCoin();
    //         shopFoodManager.CheckAds();
    //         shopFoodManager.Show_Reward(2, GameData.wonderfulBundId, GameData.coinReward, GameData.hints, GameData.timeCount, GameData.ads, GameData.timeCount > 0);//ads

    //     }
    //     else if (type == 5)
    //     {
    //         GameData.SuperBundle = true;
    //         GameData.timeCount = 36;
    //         GameData.ads = true;
    //         GameData.AdsRemoved = true;
    //         GameData.coinReward = GameData.superbBundleCoin;
    //         GameData.hints = GameData.hintsuperbBundle;
    //         AddCoinBonus(GameData.superbBundleCoin);
    //         AddHints(GameData.hintsuperbBundle);
    //         UIManager.Instance.ShowCoin();
    //         shopFoodManager.CheckAds();
    //         shopFoodManager.Show_Reward(2, GameData.superbBundId, GameData.coinReward, GameData.hints, GameData.timeCount, GameData.ads, GameData.timeCount > 0);//ads

    //     }
    //     else if (type == 6)
    //     {
    //         GameData.MasterBundle = true;
    //         GameData.timeCount = 72;
    //         GameData.ads = true;
    //         GameData.AdsRemoved = true;
    //         GameData.coinReward = GameData.masterpieceBundleCoin;
    //         GameData.hints = GameData.hintmasterpieceBundle;
    //         AddCoinBonus(GameData.masterpieceBundleCoin);
    //         AddHints(GameData.hintmasterpieceBundle);
    //         UIManager.Instance.ShowCoin();
    //         shopFoodManager.CheckAds();
    //         shopFoodManager.Show_Reward(2, GameData.masterpieceBundId, GameData.coinReward, GameData.hints, GameData.timeCount, GameData.ads, GameData.timeCount > 0);//ads

    //     }
    // }
    // void AddCoinBonus(int number)
    // {
    //     GameData.Coins += number;
    //     GameData.Save();

    // }
    // void AddHints(int number)
    // {
    //     GameData.Magnet += number;
    //     GameData.Freeze += number;
    //     GameData.Shuffle += number;
    //     GameData.MagicKey += number;
    //     GameData.BlowTorch += number;
    //     GameData.Save();

    // }

    // public void GetReWardCoins(int type)
    // {
    //     DebugLog.WriteLog("tat SHow Loding");
    //     ShowLoading(false);
    //     AudioManager.Instance.Play("Reward");
    //     int coin = 0;
    //     GameData.coinReward = 0;
    //     if (type == 1)
    //     {
    //         coin = GameData.coin1;
    //         GameData.coinReward = coin;
    //         AddCoinBonus(coin);
    //         UIManager.Instance.ShowCoin();
    //         shopFoodManager.Show_Reward(3, GameData.gold1ProductId, GameData.coinReward, 0, 0, false, false);//ads

    //     }
    //     else if (type == 2)
    //     {
    //         coin = GameData.coin2;
    //         GameData.coinReward = coin;
    //         AddCoinBonus(coin);
    //         UIManager.Instance.ShowCoin();
    //         shopFoodManager.Show_Reward(3, GameData.gold2ProductId, GameData.coinReward, 0, 0, false, false);//ads
    //     }
    //     else if (type == 3)
    //     {
    //         coin = GameData.coin3;
    //         GameData.coinReward = coin;
    //         AddCoinBonus(coin);
    //         UIManager.Instance.ShowCoin();
    //         shopFoodManager.Show_Reward(3, GameData.gold3ProductId, GameData.coinReward, 0, 0, false, false);//ads
    //     }
    //     else if (type == 4)
    //     {
    //         coin = GameData.coin4;
    //         GameData.coinReward = coin;
    //         AddCoinBonus(coin);
    //         UIManager.Instance.ShowCoin();
    //         shopFoodManager.Show_Reward(3, GameData.gold4ProductId, GameData.coinReward, 0, 0, false, false);//ads
    //     }
    //     else if (type == 5)
    //     {
    //         coin = GameData.coin5;
    //         GameData.coinReward = coin;
    //         AddCoinBonus(coin);
    //         UIManager.Instance.ShowCoin();
    //         shopFoodManager.Show_Reward(3, GameData.gold5ProductId, GameData.coinReward, 0, 0, false, false);//ads
    //     }
    //     else if (type == 6)
    //     {
    //         coin = GameData.coin6;
    //         GameData.coinReward = coin;
    //         AddCoinBonus(coin);
    //         UIManager.Instance.ShowCoin();
    //         shopFoodManager.Show_Reward(3, GameData.gold6ProductId, GameData.coinReward, 0, 0, false, false);//ads
    //     }

    // }
    // public void ShowFaildPurchase()
    // {
    //     ShowLoading(false);
    //     Toast.Instance.ShowToast("Purchase failed");

    // }
    // public void ShowAlreaddyPurchase()
    // {
    //     shopFoodManager.CheckAds();
    // }
    // public void ShowRefund() //Den bu
    // {

    // }
    /// <summary>
    /// Xem quang cao kiem 150 trong SHop
    /// </summary>
    public void ShowAdsCoin()
    {
        AudioManager.Instance.Play("Click");
        bool hasInternet = Utilities.CheckNetWork();

        if (!hasInternet)
        {
            ToastManager.Instance.ShowToast("Network error. Please check again");
            return;
        }
        //ShowLoading();
        // MobileMonetizationPro_AdmobAdsInitializer.instance.SetRewardItem("GetCoinShop");
        // MobileMonetizationPro_AdmobAdsInitializer.instance.ShowRewardedAd(Scenes.HomeScene);
    }
    /// <summary>
    /// Tra ve 150 coin
    /// </summary>
//    public void RewardCoinsAds()
//     {
//  AudioManager.Instance.Play("Reward");
//         // Hiển thị UI
//         UIManager.Instance.ShowCoin();
//         if (pnl_Show_Coin_Reward != null)
//         {
//           pnl_Show_Coin_Reward.SetActive(true);
//             EffectCoinShow effect = pnl_Show_Coin_Reward.GetComponent<EffectCoinShow>();
//             if (effect != null)
//             {
//                 effect.ShowEffect(Items.Coin, 150);
//             }
//         }
//     }

}
