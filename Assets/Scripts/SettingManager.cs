using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public Sprite[] onSprite;
    public Sprite[] offSprite;
    public Image[] typeSprite;

    void OnEnable()
    {
        SetInfoMusic();
        SetInfoSound();
        SetInfoVibrate();
    }
    public void SetMusic()
    {
        AudioManager.Instance.Play("Click");
        GameData.IsMUsicOn = !GameData.IsMUsicOn;
        GameData.Save();
        SetInfoMusic();
        AudioManager.Instance.LoadSettings();
    }
    void SetInfoMusic()
    {
        if (!GameData.IsMUsicOn)
        {
            typeSprite[0].sprite = onSprite[0];
        }
        else
        {
            typeSprite[0].sprite = offSprite[0];
        }

    }
    public void SetSound()
    {
       AudioManager.Instance.Play("Click");
        GameData.IsSoundOn = !GameData.IsSoundOn;
        GameData.Save();
        SetInfoSound();
        AudioManager.Instance.LoadSettings();
    }
    void SetInfoSound()
    {
        if (!GameData.IsSoundOn)
        {
            typeSprite[1].sprite = onSprite[1];
        }
        else
        {
            typeSprite[1].sprite = offSprite[1];
        }
    }

    public void SetVibrate()
    {
     AudioManager.Instance.Play("Click");
        GameData.IsVibrateOn = !GameData.IsVibrateOn;
        GameData.Save();
        SetInfoVibrate();
        AudioManager.Instance.LoadSettings();
    }
    void SetInfoVibrate()
    {
      
        if (GameData.IsVibrateOn)
        {
            typeSprite[2].sprite = onSprite[2];
        }
        else
        {
            typeSprite[2].sprite = offSprite[2];
        }
    }


}
