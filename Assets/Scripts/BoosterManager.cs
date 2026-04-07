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
}
public class BoosterManager : MonoBehaviour
{
    public RectTransform swapBoost;
    public RectTransform undoBoost;
    public RectTransform hammerBoost;

    public static BoosterManager Instance;

    [Header("Settings")]
    private Stack<UndoStep> undoStack = new Stack<UndoStep>();
    public bool isHammerActive = false; // Trạng thái đang cầm búa
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

    // IEnumerator Start()
    // {
    //     // Đợi đến cuối Frame để Layout Group tính toán xong vị trí
    //     yield return new WaitForEndOfFrame();
    //     //TutorialManager.Instance.StartBoosterFocus(swapBoost, "Swap Food");
    // }
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
            Txt_Booster_Name.text = "Hammer";
            Txt_Booster_Des.text = "Smash a bomb plate";
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
            TutorialManager.Instance.StartBoosterFocus(undoBoost, "Undo", 1);
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
            TutorialManager.Instance.StartBoosterFocus(hammerBoost, "Hamer", 3);
            GameData.HammerBoostTutorial = true;
            GameData.Save();
        }
    }
    public void ExecuteUndo()
    {
        if (GameManager.Instance.IsWinOrFail())
            return;
        GameManager.Instance.PlayClick();
        if (itemBoosts[0].level > GameData.SavedLevelIndex)
        {
            ToastManager.Instance.ShowToast("Unlock Level " + (itemBoosts[0].level + 1));
            return;
        }

        Debug.Log("undoStack " + undoStack.Count);

        if (GameData.UndoNumber <= 0)
        {
            Pnl_Booster_Buy.SetActive(true);
            BoosterBuyManager boosterBuyManager = Pnl_Booster_Buy.GetComponent<BoosterBuyManager>();
            boosterBuyManager.SetData("Undo Booster", "Undo Steps", 1800, 3, undoIconTo, 0);
            return;
        }

        // . Kiểm tra nếu không còn bước nào để Undo
        if (undoStack.Count == 0)
        {
            ToastManager.Instance.ShowToast("No undo steps available");

            return;
        }

        //Tat huong dan
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.EndBoosterTutorial();
        }

        // Giả sử bạn đã kéo thả BoosterOverlayUI vào Inspector
        boosterOverlayUI.PlayBoosterAnim("UNDO", undoIconTo, () =>
        {
            AudioManager.Instance.Play("Touch");
            // Chức năng thực sự chạy
            // 2. Lấy dữ liệu bước đi gần nhất ra khỏi Stack
            UndoStep lastStep = undoStack.Pop();
            FoodTile tile = lastStep.tile;
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
            Debug.Log("colId " + colId + "oldIndex " + oldIndex);
            GameManager.Instance.ShiftTilesDownInColumn(colId, oldIndex);
            StartCoroutine(ResetBuff());
        });


    }
  IEnumerator ResetBuff()
    {
        yield return new WaitForSeconds(1.0f);
        if (!GameManager.Instance.bufferCtrl.IsFull())
        {
             GameManager.Instance.bufferCtrl.ResetWarning();
        }
       
        
    }
    // // Hàm này phải được gọi trong FoodTile khi người chơi Click nhặt đĩa
    // public void RecordMove(FoodTile tile, Vector3 pos, Vector3 scale)
    // {
    //     undoStack.Push(new UndoStep
    //     {
    //         tile = tile,
    //         originalPosition = pos,
    //         originalScale = scale
    //     });
    // }
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
        Debug.Log($"<color=yellow>Recorded move:</color> {step.tile.name} from Col {step.columnId} at Index {step.originalIndex}");
    }
    // ==========================================
    // 2. CHỨC NĂNG SHUFFLE (XÁO TRỘN)
    // ==========================================

    public void OnShuffleButtonClicked()
    {
        if (GameManager.Instance.IsWinOrFail())
            return;
        GameManager.Instance.PlayClick();
        if (itemBoosts[1].level > GameData.SavedLevelIndex)
        {
            ToastManager.Instance.ShowToast("Unlock Level " + (itemBoosts[1].level + 1));
            return;
        }
        if (GameData.SwapNumber <= 0)
        {
            Pnl_Booster_Buy.SetActive(true);
            BoosterBuyManager boosterBuyManager = Pnl_Booster_Buy.GetComponent<BoosterBuyManager>();
            boosterBuyManager.SetData("Swap Booster", "Swap", 1900, 3, shuffleIconTo, 1);
            return;
        }
        // Giả sử bạn đã kéo thả BoosterOverlayUI vào Inspector
        boosterOverlayUI.PlayBoosterAnim("SHUFFLE", shuffleIconTo, () =>
        {
            AudioManager.Instance.Play("CollectingFood");
            // 1. Thu thập tất cả các đĩa đang còn trên bàn từ 3 cột
            List<FoodTile> allActiveTiles = new List<FoodTile>();
            for (int i = 0; i < 3; i++)
            {
                allActiveTiles.AddRange(GameManager.Instance.boardCtrl.allTilesOnBoard[i]);
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
            Debug.Log("Shuffle executed!");
        });


    }
    private string shuffleSeqId = "Booster_Shuffle_Seq";

    public void ExecuteShuffle(List<FoodTile> activeTiles)
    {
        if (activeTiles == null || activeTiles.Count <= 1) return;

        // 1. Dọn dẹp các Tween Shuffle cũ đang chạy (nếu có) để tránh xung đột
        DOTween.Kill(shuffleSeqId);

        // Khóa tương tác người chơi
        //GameManager.Instance.processing = true;

        // 2. XÁO TRỘN DỮ LIỆU (LIST) TRƯỚC
        // Gom tất cả vào một danh sách tạm để xáo
        for (int i = 0; i < activeTiles.Count; i++)
        {
            int randomIndex = Random.Range(i, activeTiles.Count);
            FoodTile temp = activeTiles[i];
            activeTiles[i] = activeTiles[randomIndex];
            activeTiles[randomIndex] = temp;
        }

        // Xóa dữ liệu cũ trong 3 cột của Board
        for (int i = 0; i < 3; i++) GameManager.Instance.boardCtrl.allTilesOnBoard[i].Clear();

        // 3. THIẾT LẬP SEQUENCE DI CHUYỂN
        Sequence shuffleSeq = DOTween.Sequence().SetId(shuffleSeqId);
        float spacingY = 2.0f;

        for (int i = 0; i < activeTiles.Count; i++)
        {
            int colId = i % 3; // Chia đều vào 3 cột
            int rowId = GameManager.Instance.boardCtrl.allTilesOnBoard[colId].Count;

            FoodTile tile = activeTiles[i];
            tile.columnId = colId;
            GameManager.Instance.boardCtrl.allTilesOnBoard[colId].Add(tile);

            // Đổi cha về đúng cột vật lý mới
            tile.transform.SetParent(GameManager.Instance.boardCtrl.columns[colId]);

            // Tính toán vị trí đích cục bộ (Local)
            Vector3 targetLocalPos = new Vector3(0, -(rowId * spacingY), 0);

            // Thêm hiệu ứng Jump vào Sequence
            // SetLink để tự động Kill Tween nếu đĩa bị hủy đột ngột
            shuffleSeq.Join(tile.transform.DOLocalJump(targetLocalPos, 1.5f, 1, 0.6f)
                .SetEase(Ease.OutQuad)
                .SetLink(tile.gameObject));

            // Cập nhật Sorting Order ngay lập tức để không bị đè lỗi khi bay
            tile.tray.sortingOrder = (10 - rowId) * 10;
            tile.icon.sortingOrder = (10 - rowId) * 10 + 1;
        }

        // 4. KẾT THÚC: CẬP NHẬT TRẠNG THÁI
        shuffleSeq.OnComplete(() =>
        {
            // Mở khóa xử lý
            //GameManager.Instance.processing = false;

            // Gọi hàm của bạn (không đổi) để cập nhật sáng/tối và khả năng Click
            GameManager.Instance.boardCtrl.UpdateClickableStates();

            // Xóa Stack Undo vì bàn chơi đã thay đổi hoàn toàn
            ClearStack();
            GameData.SwapNumber -= 1;
            GameManager.Instance.boosterManager.LoadBooster();
            Debug.Log("<color=green>Shuffle Finalized & Logic Updated</color>");
        });
    }
    // ==========================================
    // 3. CHỨC NĂNG HAMMER (BÚA)
    // ==========================================
    public void OnHammerButtonClicked()
    {
        if (GameManager.Instance.IsWinOrFail())
            return;
        GameManager.Instance.PlayClick();
        if (itemBoosts[2].level > GameData.SavedLevelIndex)
        {
            ToastManager.Instance.ShowToast("Unlock Level " + (itemBoosts[2].level + 1));
            return;
        }
        if (GameData.HammerNumber <= 0)
        {
            Pnl_Booster_Buy.SetActive(true);
            BoosterBuyManager boosterBuyManager = Pnl_Booster_Buy.GetComponent<BoosterBuyManager>();
            boosterBuyManager.SetData("Hamer Booster", "Hamer", 1900, 3, hammerIconTo, 2);
            return;
        }
        isHammerActive = !isHammerActive;
        // Có thể thay đổi Cursor thành hình cái búa ở đây
        if (isHammerActive)
        {
            ClearStack();
            Debug.Log("Pick a bomb disk to destroy!");
            if (GameData.HammerBoostTutorial)
            {

                //Kiem tra co bom o 3 hang dau
                List<Transform> targets = new List<Transform>();
                try
                {
                    for (int i = 0; i < 3; i++)
                    {
                        var column = GameManager.Instance.boardCtrl.allTilesOnBoard[i];
                        if (column.Count > 0)
                        {
                            if (column[0].gameObject != null)
                            {
                                targets.Add(column[0].gameObject.transform);
                            }

                            if (column[1].gameObject != null)
                            {
                                targets.Add(column[1].gameObject.transform);
                            }
                            if (column[2].gameObject != null)
                            {
                                targets.Add(column[2].gameObject.transform);
                            }
                        }
                    }
                }
                catch { }


                bool hasBom = false;

                if (targets.Count > 0)
                {
                    foreach (var target in targets)
                    {
                        FoodTile foodTile = target.GetComponent<FoodTile>();
                        if (foodTile != null)
                        {
                            if (foodTile.data.isBuff)
                            {
                                hasBom = true;
                                break;
                            }
                        }
                    }
                }

                if (hasBom)
                {
                    TutorialManager.Instance.EnableHameHint(true);
                }
                else
                {
                    Debug.Log("Ko co bom");
                    ToastManager.Instance.ShowToast("no bom");
                    isHammerActive = false;
                }



            }
            else
            {
                TutorialManager.Instance.EndTutorial();
                TutorialManager.Instance.StartHamerTutorial();
            }


        }
    }
    public void ExecuteHammer(FoodTile targetTile)
    {
        int colId = targetTile.columnId;

        // 1. Khóa xử lý để tránh người chơi bấm đĩa khác lúc đang nổ
        //GameManager.Instance.processing = true;

        // 2. Xóa khỏi List dữ liệu của Board
        GameManager.Instance.boardCtrl.allTilesOnBoard[colId].Remove(targetTile);

        // 3. Hiệu ứng Visual: Lắc nhẹ rồi biến mất
        targetTile.transform.DOShakePosition(0.3f, 0.2f);
        targetTile.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            // Trả về Pool
            ObjectPooler.Instance.ReturnToPool("Food", targetTile.gameObject);

            // 4. Quan trọng: Dồn hàng lên để lấp chỗ trống
            GameManager.Instance.ShiftTilesUpInColumn(colId);

            // Mở khóa xử lý và tắt trạng thái búa
            //GameManager.Instance.processing = false;
            isHammerActive = false;

            // Xóa Stack Undo vì cấu trúc cột đã thay đổi
            ClearStack();
        });
        TutorialManager.Instance.EnableHameHint(false);
        // Rung màn hình (Haptic Feedback)
        Camera.main.transform.DOShakePosition(0.2f, 0.1f);
        GameData.HammerNumber -= 1;
        Debug.Log("<color=red>Hammer destroyed targeted tile!</color>");
        TutorialManager.Instance.EndBoosterTutorial();
        GameManager.Instance.boosterManager.LoadBooster();

    }


    public void UseHammerOnTile(FoodTile targetTile)
    {
        if (!isHammerActive) return;

        // Hiệu ứng nổ/biến mất
        targetTile.transform.DOScale(0, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            // Xóa đĩa khỏi logic game
            Destroy(targetTile.gameObject);
        });

        // Rung màn hình nhẹ tạo cảm giác lực
        Camera.main.transform.DOShakePosition(0.2f, 0.1f);

        isHammerActive = false; // Dùng xong 1 lần thì tắt
    }
    public void ClearStack()
    {
        if (undoStack != null)
        {
            undoStack.Clear();
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
