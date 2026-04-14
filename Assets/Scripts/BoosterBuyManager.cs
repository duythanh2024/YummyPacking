using TMPro;
using UnityEngine;
using UnityEngine.UI;
// Tối ưu 1: Sử dụng Enum thay cho Magic Numbers (0, 1, 2, 3) để code dễ đọc và mở rộng hơn.
public enum BoosterType
{
    Undo = 0,
    Swap = 1,
    Magnet = 2,
    Slot = 3
}
public class BoosterBuyManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI Txt_Booster_Name;
    public TextMeshProUGUI Txt_Booster_Des;
    public TextMeshProUGUI Txt_Booster_Price;
    public TextMeshProUGUI Txt_Booster_Number;

    public Image Img_Booster;
    public Image Img_Small_Booster;

    private int typeBuy;
    private BoosterType currentType;
    private int currentPrice;
    private int currentSlotIndex;
    // Cache lại chuỗi tĩnh để tránh tạo rác khi gán liên tục
    public void SetData(string name, string des, int prices, int number, Sprite spriteBoost, int typeBuy, int indexSlot = 0)
    {

        Txt_Booster_Name.text = name;
        Txt_Booster_Des.text = des;
        // Tối ưu GC (Garbage Collection): Dùng SetText thay vì toán tử cộng chuỗi (+) và ToString()
        // TextMeshPro xử lý SetText mà không sinh ra rác bộ nhớ (Boxing/String Allocation)
        Txt_Booster_Price.SetText("<sprite name=\"coins_1\">{0}", prices);
        Txt_Booster_Number.SetText("X{0}", number);

        Img_Booster.sprite = spriteBoost;
        Img_Booster.SetNativeSize();
        Img_Small_Booster.sprite = spriteBoost;
        Img_Small_Booster.SetNativeSize();

        this.currentType = (BoosterType)typeBuy;
        this.currentPrice = prices;
        this.currentSlotIndex = indexSlot;

    }
    public void Buy()
    {
        AudioManager.Instance.Play("Click");

        AudioManager.Instance.Play("Click");

        // Tối ưu 2: Early Exit - Gom logic kiểm tra tiền lên đầu, thoát sớm nếu không đủ.
        // Giúp loại bỏ hoàn toàn các khối if-else lồng nhau phức tạp.
        if (GameData.Coins < currentPrice)
        {
            GameManager.Instance.Pnl_Shop.SetActive(true);
            gameObject.SetActive(false);
            return;
        }

        // Nếu đủ tiền, tiến hành thanh toán và xử lý vật phẩm
        ProcessPurchase();
    }
    private void ProcessPurchase()
    {

        GameData.Coins -= currentPrice;

        // Tối ưu 3: Dùng Switch-case gọn gàng và có tốc độ thực thi nhỉnh hơn if-else
        switch (currentType)
        {
            case BoosterType.Undo:
                GameData.UndoNumber += 3;
                break;
            case BoosterType.Swap:
                GameData.SwapNumber += 3;
                break;
            case BoosterType.Magnet:
                GameData.HammerNumber += 3;
                break;
            case BoosterType.Slot:
                UnlockSlot();
                break;
        }

        // Tối ưu 4: Giảm thiểu I/O. Code cũ gọi GameData.Save() 2 lần trong trường hợp mua Slot.
        // I/O là thao tác cực kỳ tốn chi phí, chỉ nên gọi 1 lần sau khi đã thay đổi xong mọi Data.
        GameData.Save();

        // Tối ưu 5: Cache singleton instance vào biến cục bộ nếu phải gọi nhiều lần
        // Singleton getter thường đi kèm với các phép check null (== null), cache lại sẽ giảm overhead.
        var gameManager = GameManager.Instance;
        gameManager.ShowCoin();

        if (currentType == BoosterType.Slot)
        {
            var bufferCtrl = gameManager.bufferCtrl;
            bufferCtrl.ShowBuffSlot();
            if (!bufferCtrl.IsFull())
            {
                bufferCtrl.ResetWarning();
            }
        }
        else
        {
            gameManager.boosterManager.LoadBooster();
        }

        gameObject.SetActive(false);
        ToastManager.Instance.ShowToast("Purchase successful!");
    }

    private void UnlockSlot()
    {
        switch (currentSlotIndex)
        {
            case 3: GameData.Slot1 = true; break;
            case 4: GameData.Slot2 = true; break;
            case 5: GameData.Slot3 = true; break;
        }
    }
}
