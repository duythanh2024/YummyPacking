using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class BoosterBuyManager : MonoBehaviour
{
    public TextMeshProUGUI Txt_Booster_Name;
    public TextMeshProUGUI Txt_Booster_Des;
    public TextMeshProUGUI Txt_Booster_Price;
    public TextMeshProUGUI Txt_Booster_Number;

    public Image Img_Booster;
    public Image Img_Small_Booster;
    private int typeBuy;
    private int prices;
    private int indexSlot;

    public void SetData(string name, string des, int prices, int number, Sprite spriteBoost, int typeBuy, int indexSlot = 0)
    {
        Txt_Booster_Name.text = name;
        Txt_Booster_Des.text = des;
        Txt_Booster_Price.text = "<sprite name=\"coins_1\">" + prices.ToString();
        Img_Booster.sprite = spriteBoost;
        Img_Booster.SetNativeSize();
        Img_Small_Booster.sprite = spriteBoost;
        Img_Small_Booster.SetNativeSize();
        Txt_Booster_Number.text = "X" + number;
        this.typeBuy = typeBuy;
        this.prices = prices;
        this.indexSlot = indexSlot;

    }
    public void Buy()
    {
        AudioManager.Instance.Play("Click");
        if (typeBuy == 0) //undo
        {
            if (GameData.Coins >= this.prices)
            {
                AudioManager.Instance.Play("Coins");
                GameData.Coins -= this.prices;
                GameData.UndoNumber += 3;
                GameData.Save();
                GameManager.Instance.ShowCoin();
                GameManager.Instance.boosterManager.LoadBooster();

                gameObject.SetActive(false);
            }
            else
            {
                GameManager.Instance.Pnl_Shop.SetActive(true);
            }
        }
        else if (typeBuy == 1) //swap
        {
            if (GameData.Coins >= this.prices)
            {
                AudioManager.Instance.Play("Coins");
                GameData.Coins -= this.prices;
                GameData.SwapNumber += 3;
                GameData.Save();
                GameManager.Instance.ShowCoin();
                GameManager.Instance.boosterManager.LoadBooster();

                gameObject.SetActive(false);
            }
            else
            {
                GameManager.Instance.Pnl_Shop.SetActive(true);
            }
        }
        else if (typeBuy == 2) //hamer
        {
if (GameData.Coins >= this.prices)
            {
                AudioManager.Instance.Play("Coins");
                GameData.Coins -= this.prices;
                GameData.HammerNumber += 3;
                GameData.Save();
                GameManager.Instance.ShowCoin();
                GameManager.Instance.boosterManager.LoadBooster();
                gameObject.SetActive(false);
            }
            else
            {
                GameManager.Instance.Pnl_Shop.SetActive(true);
            }
        }
        if (typeBuy == 3) //slot
        {
            if (GameData.Coins >= this.prices)
            {

                AudioManager.Instance.Play("Coins");
                GameData.Coins -= this.prices;
                GameData.Save();

                if (indexSlot == 3)
                {
                    GameData.Slot1 = true;
                    GameData.Save();
                }
                else if (indexSlot == 4)
                {
                    GameData.Slot2 = true;
                    GameData.Save();
                }
                else if (indexSlot == 5)
                {
                    GameData.Slot3 = true;
                    GameData.Save();
                }

                GameManager.Instance.ShowCoin();
                GameManager.Instance.bufferCtrl.ShowBuffSlot();
                gameObject.SetActive(false);

            }
            else
            {
                GameManager.Instance.Pnl_Shop.SetActive(true);
            }

        }
    }
}
