using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class ItemArena : MonoBehaviour
{
    public TextMeshProUGUI Txt_name;
    public TextMeshProUGUI Txt_Prices;
    public Image Img_Tick;
    public Button btn_Buy;
    private DecoItem decoItem;
    private DecoManager decoManager;

    public void SetDecoManager(DecoManager decoManager)
    {
        this.decoManager = decoManager;
    }
    public void ShowData(DecoItem decoItem)
    {
        this.decoItem = decoItem;
        Txt_name.text = decoItem.content;
        Txt_Prices.text = "Build x" + decoItem.cost;
        bool isFinish = PlayerPrefs.GetInt("items" + decoItem.id, 0) == 1;
        Img_Tick.gameObject.SetActive(false);
        btn_Buy.gameObject.SetActive(false);
        this.decoItem.isFinish = isFinish;
        if (isFinish)
            Img_Tick.gameObject.SetActive(true);
        else
            btn_Buy.gameObject.SetActive(true);
    }

   

    public void Buy()
    {
         AudioManager.Instance.Play("Click");
        if (decoItem != null)
        {
            if (GameData.Stars >= decoItem.cost)
            {
                List<int> prerequisiteIds = decoItem.prerequisiteIds;
                if (prerequisiteIds.Count > 0)
                {

                    List<DecoItem> decoItems = decoManager.GetallItems();
                    for (int i = 0; i < prerequisiteIds.Count; i++)
                    {
                        DecoItem item = decoItems.FirstOrDefault(n => n.id != decoItem.id && n.id == prerequisiteIds[i] && n.isFinish == false);
                        if (item != null)
                        {
                            ToastManager.Instance.ShowToast(item.content + " first");
                            return;
                        }
                    }

                }
                //Hien popup show item chon
                decoManager.ShowArena(false);
                decoManager.ShowOption_Arena(decoItem);

            }
            else
            {
                ToastManager.Instance.ShowToast("Not Enough Stars");
            }
        }
    }
}
