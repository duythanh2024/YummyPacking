using TMPro;
using UnityEngine;

public class ShopFoodManager : MonoBehaviour
{
    public TextMeshProUGUI Txt_Coins;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("ShopFoodManager " + gameObject.name);
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
        if(GameManager.Instance != null)
        {
             GameManager.Instance.boosterManager.LoadBooster();
        }
       
    }
}
