using DG.Tweening;
using MobileMonetizationApp;
using TMPro;
using UnityEngine;

public class ShopFoodManager : MonoBehaviour
{
    public TextMeshProUGUI Txt_Coins;
    public GameObject RewardPanel;
    public GameObject Img_Coin;
    public TextMeshProUGUI Txt_Reward;
    public CoinEffectManager coinEffectManager;
    public ScenesType scenesType;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        ShowCoins();
    }

    void ShowCoins()
    {
        if (Txt_Coins != null)
        {
            Txt_Coins.text = GameData.Coins.ToKMB();
        }
    }
    public void WatchVideo()
    {
        AudioManager.Instance.Play("Click");

        AdmobAdsManager.instance.SetRewardItem("watchFreeCoin");
        AdmobAdsManager.instance.ShowRewardedAd();

    }
    public void ShowRewardCoin()
    {

        AudioManager.Instance.Play("Reward");
        RewardPanel.SetActive(true);
        BentoTweenHelper.DoScale(Img_Coin.transform, 1.1f, 1.0f, () =>
        {
            Txt_Reward.gameObject.SetActive(true);
        });
    }
    public void Collect()
    {
        Debug.Log("Collect");
        //AudioManager.Instance.Play("Click");
        RewardPanel.SetActive(false);
        Txt_Reward.gameObject.SetActive(false);
        Img_Coin.GetComponent<RectTransform>().localScale = Vector3.one * 0.8f;
        //Canvas overlay
        if (scenesType == ScenesType.Game)
        {
            coinEffectManager.PlayCoinOverLayFlowEffect(() =>
            {
                GameData.Coins += 100;
                GameData.Save();

                ShowCoins();

            });
        }else if (scenesType == ScenesType.Home)
        {
            //Canvas camera
            coinEffectManager.PlayCoinFlowEffect(() =>
            {
                GameData.Coins += 100;
                GameData.Save();

                HomeManager.Instance.ShowInfoCoins();

            });
        }

    }
    /// <summary>
    /// MUa booser
    /// </summary>
    /// <param name="type">0: undo, 1: swap, 2: hammer</param>
    public void BuyBoost(int type)
    {
        AudioManager.Instance.Play("Click");
        int coin = 0;
        if (type == 0)
            coin = GameData.untoBootPrice;
        else if (type == 1)
            coin = GameData.shuffBootPrice;
        else if (type == 2)
            coin = GameData.hammerBootPrice;

        if (GameData.Coins >= coin)
        {
            GameData.Coins -= coin;
            if (type == 0)
                GameData.UndoNumber += 3;
            else if (type == 1)
                GameData.SwapNumber += 3;
            else if (type == 2)
                GameData.HammerNumber += 3;
            GameData.Save();
            ShowCoins();
            ToastManager.Instance.ShowToast("Success");

        }
        else
        {
            ToastManager.Instance.ShowToast("Not Enough Coins");
        }
    }
    public void Close()
    {
        AudioManager.Instance.Play("Click");
        gameObject.SetActive(false);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.boosterManager.LoadBooster();
        }

    }
}
