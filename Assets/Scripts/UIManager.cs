using TMPro;
using UnityEngine;
public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI   txt_Coin;
    
    public void ShowCoin()
    {
        txt_Coin.text=Utilities.ToKMB(GameData.Coins);
    }
}
