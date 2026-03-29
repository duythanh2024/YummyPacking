using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("--- DATA ---")]
    public List<LevelData> allLevels; // Kéo tất cả các file LevelData vào list này
    public int currentLevelIndex = 0;   // Chỉ số level hiện tại (0 là Level 1)
    public FoodDatabase foodDb; // Kéo file Database vào đây

    [Header("--- UI REFERENCES ---")]
    public Transform mahjongBoardPos;  // Kéo GameObject Mahjong_Board vào đây
    public Transform bentoGridUI;      // Kéo BentoGrid_UI vào đây
    public Transform bufferUI;         // Kéo Buffer_UI vào đây

    [Header("--- MANAGERS ---")]
    public BentoGridController gridCtrl;
    public OrderManager orderCtrl;
    public MahjongBoardController boardCtrl;
    public BufferController bufferCtrl;
    public GameObject Win_Pnl;
    public static GameManager Instance { get; private set; }
    private bool processing = false;

    private void Awake()
    {
        // 2. Thiết lập Singleton
        if (Instance == null)
        {
            Instance = this;
            // (Tùy chọn) Giữ GameManager không bị xóa khi đổi Scene
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); // Đảm bảo chỉ có duy nhất 1 GameManager
        }
    }
    void Start()
    {
        // Lấy chỉ số level đã lưu từ PlayerPrefs (mặc định là 0 nếu mới chơi lần đầu)
        currentLevelIndex = PlayerPrefs.GetInt("SavedLevelIndex", 0);

        // Gọi hàm nạp level lần đầu tiên
        InitializeLevel(currentLevelIndex);
    }

    public void InitializeLevel(int index)
    {
        // 1. Kiểm tra an toàn dữ liệu
        if (allLevels == null || allLevels.Count == 0 || index >= allLevels.Count)
        {
            Debug.LogError("Danh sách LevelData trống hoặc Index vượt quá giới hạn!");
            return;
        }

        LevelData currentData = allLevels[index];

        // 2. Dọn dẹp rác từ level cũ (nếu có)
        ClearCurrentScene();

        // 3. Nạp dữ liệu cho các Controller con



        // Nạp khay chờ (Buffer)
        bufferCtrl.capacity = currentData.bufferCapacity;

        // Nạp danh sách đơn hàng (Order Queue)
        orderCtrl.InitializeOrders(currentData.orderQueue);

        //Hien UI don hang



        // Khởi tạo Lưới Bento (Dựa trên đơn hàng đầu tiên của level đó)
        if (orderCtrl.activeOrders.Count > 0)
        {
            gridCtrl.InitializeGrid(
                orderCtrl.activeOrders[0].targetGridSize.x,
                orderCtrl.activeOrders[0].targetGridSize.y
            );
        }

        // 4. Sinh ra bàn Mahjong (Tầng 3)
        SpawnLevelBoard(currentData.boardItems);

        Debug.Log($"<color=green>🎮 LEVEL {index + 1} LOADED SUCCESSFULLY</color>");


    }
    private void ClearCurrentScene()
    {
        // Xóa tất cả các Tile còn trên bàn Mahjong
        for (int i = mahjongBoardPos.transform.childCount - 1; i >= 0; i--)
        {
            Transform child = mahjongBoardPos.transform.GetChild(i);

            // So sánh Tag dùng CompareTag sẽ nhanh hơn và tránh lỗi chính tả
            if (child.CompareTag("Food"))
            {
                // Trả về pool (thường sẽ làm mất child này khỏi parent hiện tại)
                ObjectPooler.Instance.ReturnToPool("Food", child.gameObject);
                FoodTile foodTile = child.gameObject.GetComponent<FoodTile>();
                foodTile.SetDefault();
            }
        }


        foreach (var colList in boardCtrl.allTilesOnBoard) colList.Clear();

        // // Reset Buffer (Khay chờ)
        bufferCtrl.ClearBuffer();

        // // Reset Grid (Khay Bento)
        gridCtrl.ClearGrid();

        // // Reset Order (Đơn hàng)
        // orderCtrl.ClearOrders();
    }
    private void SpawnLevelBoard(List<BoardItemSetup> items)
    {
        // boardCtrl.allTilesOnBoard.Clear();

        // foreach (var itemSetup in items)
        // {
        //     // Sinh ra miếng đồ ăn
        //     GameObject newObj = Instantiate(foodPrefab, mahjongBoardPos);
        //     FoodTile tileComponent = newObj.GetComponent<FoodTile>();

        //     // Nạp Data và Hình ảnh
        //     tileComponent.data = itemSetup.foodAsset;
        //     tileComponent.icon.sprite = itemSetup.foodAsset.icon;

        //     // Xếp vị trí và Layer (Lớp đè)
        //     // Z được trừ đi để Layer cao (1) nằm gần Camera hơn Layer thấp (0)
        //     newObj.transform.localPosition = new Vector3(itemSetup.position.x, itemSetup.position.y, -itemSetup.layer);
        //     // newObj.GetComponent<SpriteRenderer>().sortingOrder = itemSetup.layer;
        //     tileComponent.tray.sortingOrder = itemSetup.layer + 1;
        //     tileComponent.icon.sortingOrder = itemSetup.layer + 1;


        //     boardCtrl.allTilesOnBoard.Add(tileComponent);
        // }

        // // Quét ngay lần đầu tiên để xem miếng Cá hồi có bị 2 miếng Cơm đè không
        // Clear dữ liệu cũ của cả 3 cột
        foreach (var colList in boardCtrl.allTilesOnBoard) colList.Clear();
        int numColumns = 3;
        float spacingX = 2.0f;
        float spacingY = 2.0f; ; // Tăng Y một chút để nhìn rõ hàng sau

        //     // BƯỚC 1: TÍNH TOÁN SỐ HÀNG TỔNG CỘNG
        //     int totalItems = items.Count;
        //     int numRows = Mathf.CeilToInt((float)totalItems / numColumns);

        //     // BƯỚC 2: TÍNH TOÁN ĐIỂM OFFSET ĐỂ CĂN GIỮA
        //     // Chúng ta trừ đi để tâm của cả khối nằm đúng vào (0,0) của mahjongBoardPos
        //     float offsetX = ((numColumns - 1) * spacingX) / 2f;
        //     float offsetY = ((numRows - 1) * spacingY) / 2f;
        //         for (int i = 0; i < items.Count; i++)
        //         {
        //             var itemSetup = items[i];

        //             // Sinh ra miếng đồ ăn
        //             GameObject newObj = Instantiate(foodPrefab, mahjongBoardPos);
        //             FoodTile tileComponent = newObj.GetComponent<FoodTile>();

        //             // Nạp Data và Hình ảnh
        //             tileComponent.data = itemSetup.foodAsset;
        //             tileComponent.icon.sprite = itemSetup.foodAsset.icon;

        //           // BƯỚC 3: TÍNH VỊ TRÍ TỪNG Ô VỚI OFFSET
        //         int row = i / numColumns;
        //         int col = i % numColumns;

        //         // X = (vị trí cột * khoảng cách) - Offset để lùi về bên trái
        //         // Y = (vị trí hàng * khoảng cách) + Offset để đẩy lên trên (vì trục Y trong Unity đi lên là dương)
        //         float posX = (col * spacingX) - offsetX;
        //         float posY = -(row * spacingY) + offsetY; 

        //         newObj.transform.localPosition = new Vector3(posX, posY, -itemSetup.layer);

        //         // Sorting Order để đĩa trước che đĩa sau
        //         tileComponent.tray.sortingOrder = itemSetup.layer + 1;
        //             tileComponent.icon.sortingOrder = itemSetup.layer + 2;

        //             boardCtrl.allTilesOnBoard.Add(tileComponent);
        //         }
        for (int i = 0; i < items.Count; i++)
        {
            //GameObject newObj = Instantiate(foodPrefab, mahjongBoardPos);

            GameObject newObj = ObjectPooler.Instance.SpawnFromPool("Food", mahjongBoardPos.position, Quaternion.identity);
            newObj.transform.SetParent(mahjongBoardPos);
            FoodTile tile = newObj.GetComponent<FoodTile>();
            //     // Nạp Data và Hình ảnh
            tile.data = items[i].foodAsset;
            tile.icon.sprite = items[i].foodAsset.icon;
            // Xác định cột (0, 1, 2)
            int colId = i % numColumns;
            int rowIdInCol = boardCtrl.allTilesOnBoard[colId].Count; // Vị trí hiện tại trong cột đó

            // Gán dữ liệu và lưu vào List cột tương ứng
            tile.columnId = colId;
            boardCtrl.allTilesOnBoard[colId].Add(tile);

            // Tính toán vị trí (Centering logic)
            float posX = (colId - 1) * spacingX; // -1, 0, 1 để căn giữa 3 cột
            float posY = -(rowIdInCol * spacingY);

            newObj.transform.localPosition = new Vector3(posX, posY, 0);

            // Cập nhật Sorting Order theo hàng trong cột
            tile.tray.sortingOrder = (10 - rowIdInCol) * 10;
            tile.icon.sortingOrder = (10 - rowIdInCol) * 10 + 1;
        }

        boardCtrl.UpdateClickableStates();
    }
    // LOGIC CHÍNH CỦA GAME NẰM Ở ĐÂY
    public void LoadNextLevel()
    {
        Win_Pnl.SetActive(false);
        // 1. Tăng chỉ số level
        currentLevelIndex++;

        // 2. Kiểm tra nếu đã chơi hết danh sách level (Tránh bị Apple Reject vì hết content)
        if (currentLevelIndex >= allLevels.Count)
        {
            // Quay lại chơi một level ngẫu nhiên để giữ chân người chơi
            currentLevelIndex = UnityEngine.Random.Range(0, allLevels.Count);
            Debug.Log("🔄 Chơi lại level ngẫu nhiên vì đã hết danh sách!");
        }

        // 3. Lưu tiến trình vào máy (Để tắt game mở lại vẫn ở level đó)
        PlayerPrefs.SetInt("SavedLevelIndex", currentLevelIndex);
        PlayerPrefs.Save();

        // 4. Gọi hàm khởi tạo lại toàn bộ bàn chơi
        InitializeLevel(currentLevelIndex);


    }
    public void CheckBuff()
    {
        //Kiem tra hang doi
        List<FoodTile> currentBufferTiles = bufferCtrl.CheckBufferInTray();
        if (currentBufferTiles.Count > 0)
        {
            // Duyệt ngược từ cuối danh sách về 0
            for (int i = currentBufferTiles.Count - 1; i >= 0; i--)
            {
                MoveBuffToTray(currentBufferTiles[i]);
            }
        }
    }
    void MoveBuffToTray(FoodTile tappedTile)
    {
        if (tappedTile == null) return;
        OrderData currentOrder = orderCtrl.GetCurrentActiveOrder();
        if (currentOrder == null)
        {
            return;

        }
        // Lấy tọa độ đích cố định từ yêu cầu của Đơn hàng
        if (gridCtrl.GetFixedTargetCoord(tappedTile, currentOrder, out Vector2 targetCoord))
        {
            bufferCtrl.RemoveTile(tappedTile);
            Vector3 targetWorldPos = gridCtrl.GetWorldPositionOfGrid(targetCoord);
            BentoTweenHelper.ParabolicMove(tappedTile.icon.transform, targetWorldPos, () =>
            {

                gridCtrl.PlaceTile(tappedTile, targetCoord);
                BentoTweenHelper.PackPopEffect(tappedTile.transform, () =>
                {

                });


            });
        }
    }
    // PHẢI CÓ TỪ KHÓA 'public' ĐỂ FoodTile CÓ THỂ GỌI
    public void OnTileTapped(FoodTile tappedTile)
    {
        // Debug.Log("OnTileTapped");
        if (processing || tappedTile == null) return;
        processing = true;
        OrderData currentOrder = orderCtrl.GetCurrentActiveOrder();
        if (currentOrder == null)
        {
            processing = false;
            return;

        }
        int colId = tappedTile.columnId; // Lấy ID cột của miếng vừa bấm
        // Lấy tọa độ đích cố định từ yêu cầu của Đơn hàng
        if (gridCtrl.GetFixedTargetCoord(tappedTile, currentOrder, out Vector2 targetCoord))
        {


            // 1. Xóa khỏi danh sách của cột đó
            boardCtrl.allTilesOnBoard[colId].Remove(tappedTile);
            // boardCtrl.RemoveTile(tappedTile);
            // Tính vị trí World để Tween biết đường bay tới (trước khi đổi cha)
            Vector3 targetWorldPos = gridCtrl.GetWorldPositionOfGrid(targetCoord);
            if (gridCtrl.CheckFullBento())
            {
                // Nếu đã đủ ô (occupiedCells >= totalCells), gọi hàm xử lý thắng
                // Lưu ý: Có thể delay một chút để đợi Tween cuối cùng bay tới nơi
                ExecuteWinSequence();
            }
            HideUpperTray(tappedTile.tray.gameObject, 0.3f, null);
            //  tappedTile.tray.transform.DOScale(Vector3.zero,0.5f);
            BentoTweenHelper.ParabolicMove(tappedTile.icon.transform, targetWorldPos, () =>
            {

                gridCtrl.PlaceTile(tappedTile, targetCoord);
                BentoTweenHelper.PackPopEffect(tappedTile.transform, () =>
                {
                    processing = false;
                    ShiftTilesUpInColumn(colId);
                    //  CheckWinCondition();
                });


            });
        }
        // 2. Nếu không vừa Bento, thử cho vào Khay chờ (Buffer)
        else if (!bufferCtrl.IsFull())
        {
            Debug.Log("HANG CHO");
            // boardCtrl.RemoveTile(tappedTile);
            boardCtrl.allTilesOnBoard[colId].Remove(tappedTile);
            bool isEmpty = bufferCtrl.AddToBuffer(tappedTile);
            if (isEmpty)
            {
                // tappedTile.tray.enabled = false;
                Vector3 bufferPos = bufferCtrl.GetNextAvailableSlotPos();
                Debug.Log(bufferPos);
                HideUpperTray(tappedTile.tray.gameObject, 0.02f, () =>
                {
                    BentoTweenHelper.ParabolicMove(tappedTile.transform, bufferPos);
                    processing = false;
                    tappedTile.isClickable = false;
                    ShiftTilesUpInColumn(colId);
                });

            }
            else
            {

                BentoTweenHelper.ErrorShake(tappedTile.transform);
                Handheld.Vibrate();
                processing = false;

            }


        }
        else
        {

            Debug.Log("ERROE");
            // Trường hợp kẹt: Rung lắc báo lỗi
            BentoTweenHelper.ErrorShake(tappedTile.transform);
            processing = false;
            Handheld.Vibrate();
        }

    }
    private void ShiftTilesUpInColumn(int colId)
    {
        float spacingY = 2.0f;
        // Sử dụng ID để dễ dàng quản lý và tránh xung đột
        string sequenceId = "ShiftColumn_" + colId;
        DOTween.Kill(sequenceId);
        Sequence seq = DOTween.Sequence().SetId(sequenceId);
        List<FoodTile> targetColumn = boardCtrl.allTilesOnBoard[colId];

        for (int i = 0; i < targetColumn.Count; i++)
        {
            FoodTile tile = targetColumn[i];

            // Giữ nguyên X, chỉ thay đổi Y dựa trên Index i mới trong List cột
            float newPosY = -(i * spacingY);
            Vector3 targetPos = new Vector3(tile.transform.localPosition.x, newPosY, 0);

            // Hiệu ứng đẩy lên dọc

            seq.Join(tile.transform.DOLocalMove(targetPos, 0.3f)
                        .SetEase(Ease.OutBack)
                        .SetTarget(tile)
                        .SetLink(tile.gameObject, LinkBehaviour.KillOnDestroy));
            // Cập nhật lại Sorting Order cho đúng tầng
            tile.tray.sortingOrder = (10 - i) * 10;
            tile.icon.sortingOrder = (10 - i) * 10 + 1;
        }

        seq.OnComplete(() =>
        {
            boardCtrl.UpdateClickableStates();

        });
    }
    //     private void ShiftTilesUp()
    // {
    //     int numColumns = 3;    // Số cột cố định
    //     float spacingX = 2.0f; // Khoảng cách ngang
    //     float spacingY = 2.0f; // Khoảng cách dọc
    //    // 1. Tính toán lại số hàng dựa trên số lượng đĩa còn lại
    //     int totalItems = boardCtrl.allTilesOnBoard.Count;
    //     int numRows = Mathf.CeilToInt((float)totalItems / numColumns);

    //     // Offset để căn giữa toàn bộ khối
    //     float offsetX = ((numColumns - 1) * spacingX) / 2f;
    //     float offsetY = ((numRows - 1) * spacingY) / 2f;

    //     Sequence shiftSequence = DOTween.Sequence();

    //     for (int i = 0; i < boardCtrl.allTilesOnBoard.Count; i++)
    //     {
    //         FoodTile tile = boardCtrl.allTilesOnBoard[i];

    //         // LOGIC QUAN TRỌNG: 
    //         // i % numColumns: Xác định đĩa này thuộc cột nào (0, 1, hoặc 2)
    //         // i / numColumns: Xác định đĩa này đang ở hàng thứ mấy trong cột đó
    //         int col = i % numColumns; 
    //         int row = i / numColumns; 

    //         // Tính tọa độ mục tiêu
    //         float posX = (col * spacingX) - offsetX;
    //         float posY = -(row * spacingY) + offsetY; // Trục Y âm để hàng sau nằm dưới hàng trước

    //         Vector3 targetPos = new Vector3(posX, posY, tile.transform.localPosition.z);

    //         // Hiệu ứng nhích lên mượt mà
    //         shiftSequence.Join(tile.transform.DOLocalMove(targetPos, 0.3f).SetEase(Ease.OutBack));

    //         // Cập nhật Layer để đĩa trước luôn đè lên đĩa sau
    //         tile.tray.sortingOrder = (10 - row) * 10;
    //         tile.icon.sortingOrder = (10 - row) * 10 + 1;
    //     }

    //     // 4. Hoàn tất và mở khóa tương tác
    //     shiftSequence.OnComplete(() => {
    //        boardCtrl.UpdateClickableStates();

    //     });
    // }
    public void HideUpperTray(GameObject tray, float timeScale, Action callBack)
    {
        if (tray == null) return;
        tray.transform.DOKill();


        // Tạo một Sequence để các hiệu ứng chạy đồng thời (Join) hoặc kế tiếp (Append)
        Sequence exitSeq = DOTween.Sequence();

        // Bước 1: Nảy lên nhẹ (Anticipation)
        exitSeq.Append(tray.transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutQuad));

        // Bước 2: Thu nhỏ về 0, Xoay nhẹ và Mờ dần chạy CÙNG LÚC
        exitSeq.Append(tray.transform.DOScale(0f, timeScale).SetEase(Ease.InBack));
        exitSeq.SetLink(tray, LinkBehaviour.KillOnDestroy);
        // Bước 3: Dọn dẹp sau khi xong
        exitSeq.OnComplete(() =>
        {
            callBack?.Invoke();
            //tray.SetActive(false);
            // Hoặc Destroy(tray) nếu bạn không dùng Pooling
        });
    }
    private void ExecuteWinSequence()
    {
        // Đợi một khoảng ngắn (ví dụ 0.3s) để ô cuối cùng kịp bay vào khay rồi mới hiện UI thắng
        StartCoroutine(DelayedWin(0.5f));
    }

    IEnumerator DelayedWin(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Winner! Tất cả ô đã vào vị trí.");
        // Gọi UI thắng hoặc Next Level ở đây
        OrderData currentOrder = orderCtrl.GetCurrentActiveOrder();
        if (currentOrder != null)
        {

            orderCtrl.CompleteOrder(currentOrder, () =>
            {
                gridCtrl.MoveOutAndRefresh(); //(Đẩy khay cũ đi, kéo khay mới vào).
            });



        }
    }

    public void CheckWin()
    {

        // 3. Logic thắng cuộc: Phải dọn sạch bàn VÀ hoàn thành hết đơn hàng
        bool isAllOrdersDone = orderCtrl.activeOrders.Count == 0 && orderCtrl.IsQueueEmpty();

        if (isAllOrdersDone)
        {
            OnLevelVictory();
        }
    }



    private void OnLevelVictory()
    {
        Debug.Log("🎉 CHIẾN THẮNG! Toàn bộ đơn hàng đã xong và bàn đã sạch.");

        // TODO: Hiển thị Popup Victory UI
        // Tạm thời dừng game hoặc load level tiếp theo
        // UIManager.Instance.ShowWinPopup();
        Win_Pnl.SetActive(true);
        //   LoadNextLevel();
    }
    private void CheckLoseCondition()
    {
        // Bế tắc: Khay chờ đầy VÀ không mảnh nào trong khay có thể đưa vào lưới Bento
        // -> Kích hoạt màn hình Thua cuộc hoặc gợi ý dùng Booster
    }
}