using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public struct UndoStep
{
    public FoodTile tile;
    public Vector3 originalPosition;
    public Vector3 originalScale;
    public int columnId;      // Cột của đĩa trước khi nhặt
    public int originalIndex;   // Vị trí của đĩa trong List của cột đó
    public int layerId;
    public int rowId;
    // --- BƯỚC MỚI: CHỤP ẢNH HIỂN THỊ ---
    public Vector3 originalLocalPos;    // Tọa độ vật lý (độ so le, độ khít)
    public int originalSortingOrder;   // Thứ tự hiển thị (Lớp đè lên lớp)
}
public class BoosterManager : MonoBehaviour
{
    public RectTransform swapBoost;
    public RectTransform undoBoost;
    public RectTransform hammerBoost;
    public static BoosterManager Instance;
    [Header("Settings")]
    private Stack<UndoStep> undoStack = new Stack<UndoStep>();
    private const int MAX_UNDO_STEPS = 3; // Giới hạn 3 bước
    public BoosterOverlayUI boosterOverlayUI;
    public Sprite shuffleIconTo;
    public Sprite undoIconTo;
    public Sprite hammerIconTo;
    public Sprite extraSlotIconTo;
    public GameObject Pnl_Booster;
    public GameObject Pnl_Booster_Buy;
    public Image Img_Booster;
    public TextMeshProUGUI Txt_Booster_Name;
    public TextMeshProUGUI Txt_Booster_Des;
    private int typeBooster;
    public ItemBoost[] itemBoosts;
    private void Awake()
    {
        Instance = this;
    }
    public void LoadBooster()
    {
        int currentLevelIndex = GameData.SavedLevelIndex;

        foreach (ItemBoost itemBoost in itemBoosts)
        {
            if (itemBoost.level <= currentLevelIndex)
            {
                itemBoost.Img_UnLock.SetActive(true);
                itemBoost.Img_Locks.SetActive(false);
            }
            else
            {
                itemBoost.Img_UnLock.SetActive(false);
                itemBoost.Img_Locks.SetActive(true);
            }

            if (itemBoost.typeBooter == 1)
            {
                itemBoost.txt_Level.text = GameData.UndoNumber.ToString();
            }
            if (itemBoost.typeBooter == 2)
            {
                itemBoost.txt_Level.text = GameData.SwapNumber.ToString();
            }
            if (itemBoost.typeBooter == 3)
            {
                itemBoost.txt_Level.text = GameData.HammerNumber.ToString();
            }

        }
    }

    // ==========================================
    // 1. CHỨC NĂNG UNDO (HOÀN TÁC)
    // ==========================================
    public void ShowBoosterDes(int typeBooster)
    {
        this.typeBooster = typeBooster;
        if (typeBooster == 1) //Undo
        {
            Img_Booster.sprite = undoIconTo;
            Img_Booster.SetNativeSize();
            Txt_Booster_Name.text = "Undo";
            Txt_Booster_Des.text = "Go back one step";
            Pnl_Booster.SetActive(true);
        }
        else if (typeBooster == 2) //Swap
        {
            Img_Booster.sprite = shuffleIconTo;
            Img_Booster.SetNativeSize();
            Txt_Booster_Name.text = "Swap";
            Txt_Booster_Des.text = "Rearrange all dishes";
            Pnl_Booster.SetActive(true);
        }
        else if (typeBooster == 3) //Hamer
        {
            Img_Booster.sprite = hammerIconTo;
            Img_Booster.SetNativeSize();
            Txt_Booster_Name.text = "Magnet";
            Txt_Booster_Des.text = "Instantly collects dishes!";
            Pnl_Booster.SetActive(true);
        }
    }

    public void OpenTutorial()
    {
        if (this.typeBooster == 1)//Undo
        {
            TutorialManager.Instance.StartUndoTutorial();
        }
        else if (this.typeBooster == 2)//Undo
        {
            SwapTutorial();
        }
        else if (this.typeBooster == 3)//Undo
        {

            HamerTutorial();
        }
        Pnl_Booster.SetActive(false);
    }
    public void UndoTutorial()
    {
        if (!GameData.UndoBoostTutorial)
        {
            TutorialManager.Instance.StartBoosterFocus(undoBoost, "Tap to Undo", 1);
            GameData.UndoBoostTutorial = true;
            GameData.Save();
        }

    }
    public void SwapTutorial()
    {
        if (!GameData.SwapBoostTutorial)
        {
            TutorialManager.Instance.StartBoosterFocus(swapBoost, "Tap to Swap", 2);
            GameData.SwapBoostTutorial = true;
            GameData.Save();
        }


    }
    public void HamerTutorial()
    {
        if (!GameData.HammerBoostTutorial)
        {
            TutorialManager.Instance.StartBoosterFocus(hammerBoost, "Tap to Collect", 3);
            GameData.HammerBoostTutorial = true;
            GameData.Save();
        }
    }
    public void ExecuteUndo()
    {


        if (GameManager.Instance.IsWinOrFail())
            return;

        if (GameManager.Instance.IsProcessing)
            return;

        GameManager.Instance.IsProcessing = true;

        GameManager.Instance.PlayClick();

        if (itemBoosts[0].level > GameData.SavedLevelIndex)
        {
            GameManager.Instance.IsProcessing = false;
            ToastManager.Instance.ShowToast("Unlock Level " + (itemBoosts[0].level + 1));
            return;
        }



        //  Debug.Log("undoStack " + undoStack.Count);

        if (GameData.UndoNumber <= 0)
        {
            GameManager.Instance.IsProcessing = false;
            Pnl_Booster_Buy.SetActive(true);
            BoosterBuyManager boosterBuyManager = Pnl_Booster_Buy.GetComponent<BoosterBuyManager>();
            boosterBuyManager.SetData("Undo", "Go back one step", 1800, 3, undoIconTo, 0);

            return;
        }
        // . Kiểm tra nếu không còn bước nào để Undo
        if (undoStack.Count == 0)
        {
            GameManager.Instance.IsProcessing = false;
            ToastManager.Instance.ShowToast("No undo steps available");
            return;
        }

        //Tat huong dan
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.EndBoosterTutorial();
        }

        // Giả sử bạn đã kéo thả BoosterOverlayUI vào Inspector
        // boosterOverlayUI.PlayBoosterAnim("UNDO", undoIconTo, () =>
        // {
        AudioManager.Instance.Play("Touch");
        // Chức năng thực sự chạy
        // 2. Lấy dữ liệu bước đi gần nhất ra khỏi Stack

        UndoStep lastStep = undoStack.Pop();
        FoodTile tile = lastStep.tile;

        // 1. Khôi phục danh tính ngay lập tức
        tile.columnId = lastStep.columnId;
        tile.rowId = lastStep.rowId;
        tile.layerId = lastStep.layerId;

        //   Debug.Log("lastStep.rowId "+lastStep.rowId);

        int colId = lastStep.columnId;
        int oldIndex = lastStep.originalIndex;

        // 3. Xử lý giải phóng đĩa khỏi khay Bento (Grid)
        // Hàm này bạn đã viết để set gridCells về 0 và occupiedCells--
        GameManager.Instance.gridCtrl.RemoveLastTileFromBento(tile);

        // 3.1 Xử lý giải phóng đĩa khỏi khay cho
        GameManager.Instance.bufferCtrl.RemoveLastTileFromBuff(tile);
        // 4. ĐƯA ĐĨA VỀ ĐÚNG CỘT VẬT LÝ (QUAN TRỌNG NHẤT)
        // Lấy Transform của cột từ mảng columns đã kéo trong Inspector
        Transform targetColTransform = GameManager.Instance.boardCtrl.columns[colId];
        tile.transform.SetParent(targetColTransform);

        // 5. CHÈN LẠI VÀO DANH SÁCH DỮ LIỆU (LIST)
        // Dùng Insert để đẩy các phần tử hiện có xuống dưới
        GameManager.Instance.boardCtrl.allTilesOnBoard[colId].Insert(oldIndex, tile);

        // 6. KHÔI PHỤC TRẠNG THÁI CHO ĐĨA
        tile.transform.localScale = lastStep.originalScale;
        tile.columnId = colId; // Đảm bảo ID cột đồng bộ
        tile.isLocked = false;
        tile.tray.gameObject.SetActive(true);
        tile.tray.gameObject.transform.localScale = Vector3.one;

        //set grid[i][j]=0;

        // Bật lại Collider để có thể chạm vào
        if (tile.TryGetComponent<BoxCollider2D>(out var col))
        {
            col.enabled = true;
        }
        GameData.UndoNumber -= 1;
        GameManager.Instance.boosterManager.LoadBooster();
        // 7. CHẠY HIỆU ỨNG ĐẨY HÀNG XUỐI (SHIFT DOWN)
        // Hàm này sẽ dùng DOTween để di chuyển toàn bộ đĩa trong cột về vị trí Y mới
        //        Debug.Log("colId " + colId + "oldIndex " + oldIndex);
        if (!GameManager.Instance.IsStack)
        {
            GameManager.Instance.ShiftTilesDownInColumn(colId, oldIndex);
        }
        else
        {
            //    Debug.Log("lastStep.originalIndex "+lastStep.originalIndex);
            // Gọi hàm Shift với logic Layer mới
            GameManager.Instance.ShiftTilesLayerDownInColumn(lastStep.columnId, lastStep.originalIndex);
        }
        GameManager.Instance.IsProcessing = false;
        StartCoroutine(ResetBuff());
        //  });


    }
    IEnumerator ResetBuff()
    {
        yield return new WaitForSeconds(1.0f);
        if (!GameManager.Instance.bufferCtrl.IsFull())
        {
            GameManager.Instance.bufferCtrl.ResetWarning();
        }


    }
    public void RecordMove(UndoStep step)
    {
        // Nếu stack đã đạt giới hạn 3 bước, chúng ta xóa bước cũ nhất (dưới cùng)
        // Trong Stack không hỗ trợ xóa đáy dễ dàng, nên ta chuyển sang List tạm hoặc kiểm tra trước khi Push
        if (undoStack.Count >= MAX_UNDO_STEPS)
        {
            // Cách chuyên nghiệp: Chuyển sang mảng, bỏ phần tử cũ nhất, nạp lại
            var list = undoStack.ToList();
            list.RemoveAt(list.Count - 1); // Xóa phần tử cũ nhất
            list.Reverse(); // Đảo lại đúng thứ tự stack
            undoStack = new Stack<UndoStep>(list);
        }
        undoStack.Push(step);
        //   Debug.Log($"<color=yellow>Recorded move:</color> {step.tile.name} from Col {step.columnId} at RowId {step.rowId} LayerId {step.layerId}");
    }
    // ==========================================
    // 2. CHỨC NĂNG SHUFFLE (XÁO TRỘN)
    // ==========================================

    public void OnShuffleButtonClicked()
    {


        if (GameManager.Instance.IsWinOrFail())
            return;
        if (GameManager.Instance.IsProcessing)
            return;

        GameManager.Instance.IsProcessing = true;
        GameManager.Instance.PlayClick();
        if (itemBoosts[1].level > GameData.SavedLevelIndex)
        {
            GameManager.Instance.IsProcessing = false;
            ToastManager.Instance.ShowToast("Unlock Level " + (itemBoosts[1].level + 1));
            return;
        }
        if (GameData.SwapNumber <= 0)
        {
            GameManager.Instance.IsProcessing = false;
            Pnl_Booster_Buy.SetActive(true);
            BoosterBuyManager boosterBuyManager = Pnl_Booster_Buy.GetComponent<BoosterBuyManager>();
            boosterBuyManager.SetData("Swap", "Rearrange all dishes", 1900, 3, shuffleIconTo, 1);
            return;
        }
        // Giả sử bạn đã kéo thả BoosterOverlayUI vào Inspector
        // boosterOverlayUI.PlayBoosterAnim("SHUFFLE", shuffleIconTo, () =>
        // {
        AudioManager.Instance.Play("CollectingFood");
        // 1. Thu thập tất cả các đĩa đang còn trên bàn từ 3 cột

        List<FoodTile> allActiveTiles = new List<FoodTile>();

        // Lấy số lượng cột thực tế từ BoardController
        int columnCount = GameManager.Instance.boardCtrl.allTilesOnBoard.Count;

        for (int i = 0; i < columnCount; i++)
        {
            // Kiểm tra null để tránh lỗi crash nếu cột đó chưa được khởi tạo
            if (GameManager.Instance.boardCtrl.allTilesOnBoard[i] != null)
            {
                allActiveTiles.AddRange(GameManager.Instance.boardCtrl.allTilesOnBoard[i]);
            }
        }

        // 2. Kiểm tra nếu không có đĩa nào thì không chạy
        if (allActiveTiles.Count <= 1) return;

        ClearStack();
        // 3. Gọi hàm thực thi xáo trộn
        ExecuteShuffle(allActiveTiles);

        // (Tùy chọn) Trừ tiền hoặc trừ lượt dùng Booster của người chơi ở đây
        //Tat huong dan
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.EndBoosterTutorial();
        }



    }
    private string shuffleSeqId = "Booster_Shuffle_Seq";
    public void ExecuteShuffle(List<FoodTile> activeTiles)
    {
        if (activeTiles == null || activeTiles.Count <= 1) return;

        DOTween.Kill(shuffleSeqId);

        // BƯỚC 1: Tắt tương tác & Làm tối ngay để tránh bug trong lúc đang bay
        foreach (var t in activeTiles)
        {
            t.isClickable = false;
            Color dimColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            t.tray.GetComponent<SpriteRenderer>().color = dimColor;
            t.icon.color = dimColor; // Dùng trực tiếp .color nếu là SpriteRenderer
        }

        // BƯỚC 2: SNAPSHOT - Chụp ảnh toàn bộ thông số của "Ô CẮM"
        List<Vector3> originalPositions = new List<Vector3>();
        List<int> originalSortingOrders = new List<int>();
        List<Vector2Int> originalLogicCoords = new List<Vector2Int>();
        List<int> originalLayers = new List<int>(); // QUAN TRỌNG: Thêm LayerId vào snapshot

        foreach (var tile in activeTiles)
        {
            originalPositions.Add(tile.transform.localPosition);
            originalSortingOrders.Add(tile.tray.sortingOrder);
            originalLogicCoords.Add(new Vector2Int(tile.columnId, tile.rowId));
            originalLayers.Add(tile.layerId); // Lưu layer của ô này
        }

        // BƯỚC 3: XÁO TRỘN DANH SÁCH THỰC THỂ (ĐĨA)
        for (int i = 0; i < activeTiles.Count; i++)
        {
            int randomIndex = Random.Range(i, activeTiles.Count);
            FoodTile temp = activeTiles[i];
            activeTiles[i] = activeTiles[randomIndex];
            activeTiles[randomIndex] = temp;
        }

        // Dọn dẹp logic board trước khi nạp lại
        int totalColumns = GameManager.Instance.boardCtrl.allTilesOnBoard.Count;
        for (int i = 0; i < totalColumns; i++)
            GameManager.Instance.boardCtrl.allTilesOnBoard[i].Clear();

        Sequence shuffleSeq = DOTween.Sequence().SetId(shuffleSeqId);

        // BƯỚC 4: GÁN LẠI VÀ CHẠY TWEEN
        for (int i = 0; i < activeTiles.Count; i++)
        {
            FoodTile tile = activeTiles[i];

            // Đĩa i nhảy vào vị trí i -> Copy toàn bộ thuộc tính của ô i
            Vector3 targetPos = originalPositions[i];
            int targetOrder = originalSortingOrders[i];
            int colId = originalLogicCoords[i].x;
            int rowId = originalLogicCoords[i].y;
            int targetLayer = originalLayers[i]; // Lấy LayerId của ô mới

            // Cập nhật Logic cho đĩa
            tile.columnId = colId;
            tile.rowId = rowId;
            tile.layerId = targetLayer; // CẬP NHẬT LAYER: Fix lỗi sáng ở đáy

            GameManager.Instance.boardCtrl.allTilesOnBoard[colId].Add(tile);
            tile.transform.SetParent(GameManager.Instance.boardCtrl.columns[colId]);

            // Cập nhật Sorting Order ngay lập tức để đĩa không bay xuyên qua nhau
            tile.tray.sortingOrder = targetOrder;
            tile.icon.sortingOrder = targetOrder + 1;

            // Tween bay về vị trí
            shuffleSeq.Join(tile.transform.DOLocalJump(targetPos, 2.0f, 1, 0.65f)
                .SetEase(Ease.OutQuad)
                .SetLink(tile.gameObject));
        }

        shuffleSeq.OnComplete(() =>
        {
            // SAU KHI BAY XONG: Gọi hàm này để tính toán lại minLayer và Sáng/Tối
            GameManager.Instance.boardCtrl.UpdateClickableStates();

            ClearStack();
            GameData.SwapNumber -= 1;
            GameManager.Instance.boosterManager.LoadBooster();
            GameManager.Instance.IsProcessing = false;
        });
    }
    public void OnHammerButtonClicked()
    {
        ///   Debug.Log("OnHammerButtonClicked");



        if (GameManager.Instance.IsWinOrFail())
            return;

        if (GameManager.Instance.IsProcessing)
            return;

        GameManager.Instance.IsProcessing = true;

        GameManager.Instance.PlayClick();
        if (itemBoosts[2].level > GameData.SavedLevelIndex)
        {
            GameManager.Instance.IsProcessing = false;
            ToastManager.Instance.ShowToast("Unlock Level " + (itemBoosts[2].level + 1));
            return;
        }
        if (GameData.HammerNumber <= 0)
        {
            GameManager.Instance.IsProcessing = false;
            Pnl_Booster_Buy.SetActive(true);
            BoosterBuyManager boosterBuyManager = Pnl_Booster_Buy.GetComponent<BoosterBuyManager>();
            boosterBuyManager.SetData("Magnet", "Instantly collects dishes!", 1900, 3, hammerIconTo, 2);
            return;
        }

        // if (GameData.HammerBoostTutorial)
        // {
        //   Debug.Log("Pick a bomb disk to destroy!");
        //Lay taat ca food so sanh ddown hang
        FoodTile foodTileTarget = null;

        OrderData orderData = GameManager.Instance.orderCtrl.GetCurrentActiveOrder();

        if (orderData != null)
        {
            List<FoodPlacement> requiredLayouts = orderData.requiredLayout;

            List<FoodTile> allActiveTiles = new List<FoodTile>();

            // Lấy số lượng cột thực tế từ BoardController
            int columnCount = GameManager.Instance.boardCtrl.allTilesOnBoard.Count;

            for (int i = 0; i < columnCount; i++)
            {
                // Kiểm tra null để tránh lỗi crash nếu cột đó chưa được khởi tạo
                if (GameManager.Instance.boardCtrl.allTilesOnBoard[i] != null)
                {

                    allActiveTiles.AddRange(GameManager.Instance.boardCtrl.allTilesOnBoard[i]);
                }
            }

            foreach (FoodTile foodTile in allActiveTiles)
            {
                foreach (FoodPlacement foodPlacement in requiredLayouts)
                {
                    if (foodTile.data.foodType.Equals(foodPlacement.foodType))
                    {
                        foodTileTarget = foodTile;
                        break;
                    }
                }


            }

            if (foodTileTarget != null)
            {
                GameData.HammerNumber -= 1;
                GameManager.Instance.boosterManager.LoadBooster();
                //hien klen vaf bay
                foodTileTarget.tray.sortingOrder = 998;
                foodTileTarget.icon.sortingOrder = 999;
                foodTileTarget.isClickable = true;
                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.EndBoosterTutorial();
                }
                BentoTweenHelper.DoScale(foodTileTarget.transform, 1.2f, 0.5f, () =>
                {

                    foodTileTarget.tray.color = Color.white;
                    foodTileTarget.icon.color = Color.white;

                   
                    BentoTweenHelper.DoScale(foodTileTarget.transform, 1.0f, 0.2f, () =>
                    {
                        StartCoroutine(ResetUndo());
                        GameManager.Instance.OnTileTapped(foodTileTarget);
                        GameManager.Instance.IsProcessing = false;
                    });

                });
            }
            else
            {
                GameManager.Instance.IsProcessing = false;
                TutorialManager.Instance.EndTutorial();
                ToastManager.Instance.ShowToast("No Disc Found!");
            }
        }

    }
    IEnumerator ResetUndo()
    {
        yield return new WaitForSeconds(1.1f);
        ClearStack();
    }
    public void ExecuteHammer(FoodTile targetTile)
    {
        int colId = targetTile.columnId;

        // 1. Khóa xử lý để tránh người chơi bấm đĩa khác lúc đang nổ
        //GameManager.Instance.processing = true;

        // 2. Xóa khỏi List dữ liệu của Board
        GameManager.Instance.boardCtrl.allTilesOnBoard[colId].Remove(targetTile);
        Vector3 bomPosition = targetTile.transform.position;
        // 3. Hiệu ứng Visual: Lắc nhẹ rồi biến mất
        targetTile.transform.DOShakePosition(0.3f, 0.2f);

        AudioManager.Instance.Play("bomb");
        targetTile.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
        {
            // Trả về Pool
            ObjectPooler.Instance.ReturnToPool("Food", targetTile.gameObject);


            // 4. Quan trọng: Dồn hàng lên để lấp chỗ trống
            GameManager.Instance.ShiftTilesUpInColumn(colId);

            // Xóa Stack Undo vì cấu trúc cột đã thay đổi
            ClearStack();

        });
        TutorialManager.Instance.EnableHameHint(false);
        // Rung màn hình (Haptic Feedback)
        Camera.main.transform.DOShakePosition(0.2f, 0.1f);
        GameData.HammerNumber -= 1;
        //  Debug.Log("<color=red>Hammer destroyed targeted tile!</color>");
        TutorialManager.Instance.EndBoosterTutorial();
        GameManager.Instance.boosterManager.LoadBooster();

    }
    public void ClearStack()
    {
        if (undoStack != null)
        {
            undoStack.Clear();
            Debug.Log("DA CELEAR " + undoStack.Count);
        }
    }

    public void ShowBuySlot(int index)
    {
        int slotPrices = 0;
        if (index == 3)
            slotPrices = GameData.slot1Price;
        else if (index == 4)
            slotPrices = GameData.slot2Price;
        else if (index == 5)
            slotPrices = GameData.slot3Price;

        Pnl_Booster_Buy.SetActive(true);
        BoosterBuyManager boosterBuyManager = Pnl_Booster_Buy.GetComponent<BoosterBuyManager>();
        boosterBuyManager.SetData("Expand Slot", "Add one more queue space.", slotPrices, 1, extraSlotIconTo, 3, index);
    }
}
