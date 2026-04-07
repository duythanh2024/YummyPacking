using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class Option_Arena : MonoBehaviour
{
    public Image[] ops;
    public Image[] ticks;
    public Button Btn_Confirm;
    private DecoManager decoManager;
    private DecoItem decoItem;

    public void SetData(DecoItem decoItem, DecoManager decoManager)
    {
        Btn_Confirm.interactable = false;
        foreach (Image tick in ticks)
        {
            tick.gameObject.SetActive(false);
        }
        this.decoItem = decoItem;
        this.decoManager = decoManager;
        Debug.Log("ID" + decoItem.id);
        Vector3 imgScale = Vector3.one;
        if (decoItem.id == 7)
        {
            imgScale = Vector3.one * 0.25f;
        }
        else if (decoItem.id == 3 || decoItem.id == 1 || decoItem.id == 6)
        {
            imgScale = Vector3.one * 0.3f;
        }
        else if (decoItem.id == 2 || decoItem.id == 4 || decoItem.id == 5 || decoItem.id == 8)
        {
            imgScale = Vector3.one * 0.5f;
        }
        else if (decoItem.id == 9)
        {
            imgScale = Vector3.one * 0.2f;
        }
        else if (decoItem.id == 10)
        {
            imgScale = Vector3.one * 0.7f;
        }
        //  Vector3 imgScale=new Vector3();
        List<DecoOption> options = decoItem.options;
        foreach (DecoOption decoOption in options)
        {
            if (decoOption.optionId == 0)
            {
                ops[0].sprite = decoOption.icon;
                ops[0].SetNativeSize();
                ops[0].GetComponent<RectTransform>().localScale = imgScale;
            }
            else if (decoOption.optionId == 1)
            {
                ops[1].sprite = decoOption.icon;
                ops[1].SetNativeSize();
                ops[1].GetComponent<RectTransform>().localScale = imgScale;
            }
            else if (decoOption.optionId == 2)
            {
                ops[2].sprite = decoOption.icon;
                ops[2].SetNativeSize();
                ops[2].GetComponent<RectTransform>().localScale = imgScale;
            }
        }
    }

    public void Build(int type)
    {
        Btn_Confirm.interactable = true;
        foreach (Image tick in ticks)
        {
            tick.gameObject.SetActive(false);
        }
        ticks[type].gameObject.SetActive(true);

        List<DecoOption> options = decoItem.options;
        DecoOption decoOption = options.FirstOrDefault(n => n.optionId == type);
        if (decoOption != null)
        {
            decoManager.Build(decoItem.id, decoOption.optionId);
        }
    }

}
