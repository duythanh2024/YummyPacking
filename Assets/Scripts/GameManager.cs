using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static MahjongBoardController;

public class GameManager : MonoBehaviour, IInputHandler
{
    [Header("--- DATA ---")]
    public List<LevelData> allLevels; // Kéo tất cả các file LevelData vào list này
    public int currentLevelIndex = 0;   // Chỉ số level hiện tại (0 là Level 1)
    public FoodDatabase foodDb; // Kéo file Database vào đây

    [Header("--- UI REFERENCES ---")]
    public Transform mahjongBoardPos;  // Kéo GameObject Mahjong_Board vào đây

    [Header("--- MANAGERS ---")]
    public BentoGridController gridCtrl;
    public OrderManager orderCtrl;
    public MahjongBoardController boardCtrl;
    public BufferController bufferCtrl;
    public GameObject Win_Pnl;
    public GameObject Fail_Pnl;
    public TextMeshProUGUI Txt_Levels;
    public TextMeshProUGUI Txt_Complete;
    public TextMeshProUGUI Txt_Order;
    public BoosterManager boosterManager;
    public GameObject Pnl_Shop;
    public GameObject Pnl_Setting;
    public GameObject Pnl_Restart;
    public TextMeshProUGUI Txt_Coins;
    public GameObject Booster;
    public Animator catAnimator;
    public static GameManager Instance { get; private set; }
    // public bool processing = false;
    public bool isWin = false;
    public bool isFail = false;
    private FoodTile selectedTile;
    private BuffSlot buffSlot;
    private float tileWSpacing = 1.6f;
    private float tileHSpacing = 1.5f;
    private bool isHome;
    public int rewardLevel;
    [HideInInspector]
    public bool IsStack;
    [HideInInspector]
    public bool IsProcessing;
    public GameObject ErrorPanel;
    public TextMeshProUGUI Txt_Error;
    public GameObject torch;
    public GameObject Img_Hint;
     public GameObject Img_Title;
    
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
        AudioManager.Instance.PlayBackground(AudioManager.Instance.gameplayMusic);
        isWin = false;
        isFail = false;
        //test
        // GameData.SavedLevelIndex = currentLevelIndex;
        // GameData.Save();
        currentLevelIndex = GameData.SavedLevelIndex;
         if (currentLevelIndex >= allLevels.Count)
        {
            // Quay lại chơi một level ngẫu nhiên để giữ chân người chơi
            currentLevelIndex = UnityEngine.Random.Range(8, allLevels.Count-1);
            Debug.Log("🔄 Chơi lại level ngẫu nhiên vì đã hết danh sách!");
     
        }
       


        // Gọi hàm nạp level lần đầu tiên
        InitializeLevel(currentLevelIndex);
        // try
        // {
        //     ErrorPanel.SetActive(false);
        //     // Lấy chỉ số level đã lưu từ PlayerPrefs (mặc định là 0 nếu mới chơi lần đầu)
        //     AudioManager.Instance.PlayBackground(AudioManager.Instance.gameplayMusic);
        //     isWin = false;
        //     isFail = false;
        //     //test
        //     GameData.SavedLevelIndex = currentLevelIndex;
        //     GameData.Save();
        //     currentLevelIndex = GameData.SavedLevelIndex;


        //     // Gọi hàm nạp level lần đầu tiên
        //     InitializeLevel(currentLevelIndex);

        // }
        // catch (Exception ex)
        // {
        //     ErrorPanel.SetActive(true);
        //     Txt_Error.SetText(ex.Message);
        // }

    }

    public void InitializeLevel(int index)
    {
        selectedTile = null;
        Booster.SetActive(true);
        GameData.SortLayer = 700;
        GameData.Save();
        Txt_Complete.gameObject.SetActive(false);
        // 1. Kiểm tra an toàn dữ liệu
        if (allLevels == null || allLevels.Count == 0 || index >= allLevels.Count)
        {
            //   Debug.LogError("Danh sách LevelData trống hoặc Index vượt quá giới hạn!");
            return;
        }
        Txt_Levels.text = "Level " + (currentLevelIndex + 1).ToString();

        LevelData currentData = allLevels[index];

        // 2. Dọn dẹp rác từ level cũ (nếu có)
        ClearCurrentScene();

        // 3. Nạp dữ liệu cho các Controller con



        // Nạp khay chờ (Buffer)

        bufferCtrl.ShowBuffSlot();
        bufferCtrl.GetCapacity();
        // Nạp danh sách đơn hàng (Order Queue)
        orderCtrl.InitializeOrders(currentData.orderQueue);

        // Khởi tạo Lưới Bento (Dựa trên đơn hàng đầu tiên của level đó)
        if (orderCtrl.activeOrders.Count > 0)
        {
            gridCtrl.InitializeGrid(
                orderCtrl.activeOrders[0].targetGridSize.x,
                orderCtrl.activeOrders[0].targetGridSize.y,
                orderCtrl.activeOrders[0].typeOfTray
            );
        }
        rewardLevel = GameData.GetRewardLevel(currentData.difficultLevel);
        IsStack = currentData.IsStack;

       
        Camera.main.orthographicSize = currentData.sizeCamera;

        // 4. Sinh ra bàn Mahjong (Tầng 3)
        SpawnLevelBoard(currentData.boardItems);
        boosterManager.LoadBooster();
        ShowCoin();
        boardCtrl.CaptureBoardDNA();
        boosterManager.ClearStack();

        Debug.Log($"<color=green>🎮 LEVEL {index + 1} LOADED SUCCESSFULLY</color>");
        //TutorialManager
        if (currentLevelIndex == 0)
        {
            TutorialManager.Instance.StartLevel1Tutorial();

        }
        else if (currentLevelIndex == 2 && !GameData.UndoBoostTutorial) //undo
        {
            boosterManager.ShowBoosterDes(1);
            // TutorialManager.Instance.StartUndoTutorial();
        }
        else if (currentLevelIndex == 4 && !GameData.SwapBoostTutorial)
        {

            boosterManager.ShowBoosterDes(2);

        }
        else if (currentLevelIndex == 11 && !GameData.HammerBoostTutorial)
        {

            boosterManager.ShowBoosterDes(3);

        }


    }

    public void ShowCoin()
    {
        Txt_Coins.text = GameData.Coins.ToKMB();
    }
    private void ClearCurrentScene()
    {
        // Xóa tất cả các Tile còn trên bàn tang 3

        for (int i = mahjongBoardPos.transform.childCount - 1; i >= 0; i--)
        {
            Transform child0 = mahjongBoardPos.transform.GetChild(i);

            for (int j = child0.transform.childCount - 1; j >= 0; j--)
            {
                Transform child = child0.transform.GetChild(j);

                // So sánh Tag dùng CompareTag sẽ nhanh hơn và tránh lỗi chính tả
                if (child.CompareTag("Food"))
                {
                    // Trả về pool (thường sẽ làm mất child này khỏi parent hiện tại)
                    ObjectPooler.Instance.ReturnToPool("Food", child.gameObject);
                    FoodTile foodTile = child.gameObject.GetComponent<FoodTile>();
                    foodTile.SetDefault();
                }
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
        // 1. Clear dữ liệu cũ
        foreach (var colList in boardCtrl.allTilesOnBoard) colList.Clear();

        // Dictionary để đếm số đĩa gỗ trong từng Layer của từng Cột: <ColumnID, <LayerID, Count>>
        Dictionary<int, Dictionary<int, int>> layerCounters = new Dictionary<int, Dictionary<int, int>>();

        // Thông số cấu hình độ lệch (Vị CTO có thể điều chỉnh tùy Art style)
        float deltaX = 0.08f;    // Độ lệch ngang giữa các Layer (tạo hiệu ứng 2.5D)
        float deltaY = -0.15f;   // Khoảng cách nhảy giữa các Layer
        float spacingY = 2.0f;  // Khoảng cách nhỏ giữa các món TRONG CÙNG MỘT LAYER (để không trùng Y)
        float zStep = 0.1f;
        foreach (var itemSetup in items)
        {
            if (itemSetup.foodAsset == null) continue;

            int cId = itemSetup.columnId;
            int rowId = itemSetup.rowId;
            int lId = itemSetup.layer;

            if (cId >= boardCtrl.columns.Length) continue;

            // Khởi tạo bộ đếm nội bộ cho Layer
            if (!layerCounters.ContainsKey(cId)) layerCounters[cId] = new Dictionary<int, int>();
            if (!layerCounters[cId].ContainsKey(lId)) layerCounters[cId][lId] = 0;

            int subIndex = layerCounters[cId][lId];
            if (IsStack)
            {
                subIndex = rowId;
            }

            Transform targetColTransform = boardCtrl.columns[cId];

            // 2. Spawn từ Pool
            GameObject newObj = ObjectPooler.Instance.SpawnFromPool("Food", targetColTransform.position, Quaternion.identity);
            newObj.transform.SetParent(targetColTransform);

            FoodTile tile = newObj.GetComponent<FoodTile>();
            if (tile != null)
            {
                tile.data = itemSetup.foodAsset;
                tile.icon.sprite = itemSetup.foodAsset.icon;
                tile.columnId = cId;
                tile.rowId = subIndex;
                tile.layerId = lId;
                tile.typeTrayFood = itemSetup.typeTrayFood;
                tile.gameObject.SetActive(true);
                tile.SetStatus();
                // 3. CÔNG THỨC VỊ TRÍ MỚI: Tránh trùng Y hoàn toàn
                // Tính theo: Gốc của Layer + Độ lệch của món trong Layer đó
                // deltaX += itemSetup.customOffset.x;
                // deltaY += itemSetup.customOffset.y;
                float posX = lId * deltaX + itemSetup.customOffset.x;
                float posY = (lId * deltaY) - (subIndex * spacingY) + itemSetup.customOffset.y;
                float posZ = (lId * 1.0f) + (subIndex * zStep);
                newObj.transform.localPosition = new Vector3(posX, posY, posZ);

                // 4. CẬP NHẬT SORTING ORDER
                // Ưu tiên Layer trước, sau đó là subIndex (món sau đè lên món trước trong cùng layer)
                // Giả sử tối đa bạn có khoảng 10 Layer (0 đến 9)
                int maxLayers = 10;

                // CÔNG THỨC MỚI: Layer càng nhỏ (0) thì Sorting càng lớn
                // (maxLayers - lId) giúp đảo ngược thứ tự: Layer 0 -> 200, Layer 1 -> 180...
                int baseSorting = 100 + ((maxLayers - lId) * 20) + (subIndex * 2);

                tile.tray.sortingOrder = baseSorting;
                tile.icon.sortingOrder = baseSorting + 1;
                tile.iconStatus.sortingOrder = baseSorting + 2;
                // 5. Thêm vào danh sách quản lý
                boardCtrl.allTilesOnBoard[cId].Add(tile);

                // Tăng bộ đếm cho món tiếp theo cùng Layer trong cột này
                layerCounters[cId][lId]++;
            }
        }

        // 6. Cập nhật trạng thái Clickable
        boardCtrl.UpdateClickableStates();


    }
    private void SpawnLevelBoard2(List<BoardItemSetup> items)
    {
        // 1. Clear dữ liệu cũ trên BoardController
        foreach (var colList in boardCtrl.allTilesOnBoard) colList.Clear();

        // Thông số Delta so le (Vị CTO có thể lấy từ LevelData hoặc config chung)
        float deltaX = 0.08f; // Tương đương 8px trong không gian World Unit
        float deltaY = -0.15f; // Tương đương -15px

        foreach (var itemSetup in items)
        {
            if (itemSetup.foodAsset == null) continue;

            // 2. Xác định cột mục tiêu dựa trên columnId trong data
            int colId = itemSetup.columnId;
            if (colId >= boardCtrl.columns.Length) continue; // Phòng lỗi Designer nhập quá số cột

            Transform targetColTransform = boardCtrl.columns[colId];

            // 3. Spawn từ Pool
            GameObject newObj = ObjectPooler.Instance.SpawnFromPool("Food", targetColTransform.position, Quaternion.identity);
            newObj.transform.SetParent(targetColTransform);

            FoodTile tile = newObj.GetComponent<FoodTile>();
            if (tile != null)
            {
                // 4. Đổ dữ liệu từ BoardItemSetup vào Tile
                tile.data = itemSetup.foodAsset;
                tile.icon.sprite = itemSetup.foodAsset.icon;
                tile.columnId = colId;
                tile.layerId = itemSetup.layer; // Lưu layer để xử lý logic khóa/mở sau này
                tile.gameObject.SetActive(true);

                // 5. TÍNH TOÁN VỊ TRÍ SO LE THEO LAYER
                // Công thức: Pos = layer * Delta
                float posX = itemSetup.layer * deltaX;
                float posY = itemSetup.layer * deltaY;
                newObj.transform.localPosition = new Vector3(posX, posY, 0);

                // 6. CẬP NHẬT SORTING ORDER (Quan trọng nhất)
                // Layer càng cao (ở trên) thì Sorting Order càng lớn
                int baseSorting = 100 + (itemSetup.layer * 10);
                tile.tray.sortingOrder = baseSorting;
                tile.icon.sortingOrder = baseSorting + 1;

                // 7. Thêm vào danh sách quản lý của Controller
                boardCtrl.allTilesOnBoard[colId].Add(tile);
            }
        }

        // 8. Cập nhật trạng thái Clickable (Chỉ cho phép click layer cao nhất mỗi cột)
        boardCtrl.UpdateClickableStates();
    }

    private void SpawnLevelBoard1(List<BoardItemSetup> items)
    {

        // Clear dữ liệu cũ của cả 3 cột
        foreach (var colList in boardCtrl.allTilesOnBoard) colList.Clear();
        int numColumns = 3;
        float spacingY = 2.0f; ; // Tăng Y một chút để nhìn rõ hàng sau


        for (int i = 0; i < items.Count; i++)
        {
            // Xác định ID cột (0, 1, 2)
            int colId = i % numColumns;
            Transform targetColTransform = boardCtrl.columns[colId];

            // 2. Spawn từ Pool tại vị trí của Cột tương ứng
            GameObject newObj = ObjectPooler.Instance.SpawnFromPool("Food", targetColTransform.position, Quaternion.identity);

            // 3. GÁN CHA LÀ CỘT CỤ THỂ
            newObj.transform.SetParent(targetColTransform);

            FoodTile tile = newObj.GetComponent<FoodTile>();
            if (tile != null)
            {
                if (items[i].foodAsset != null)
                {
                    tile.data = items[i].foodAsset;
                    tile.icon.sprite = items[i].foodAsset.icon;
                    tile.gameObject.SetActive(true);
                }
                else
                {
                    newObj.SetActive(false);
                    continue; // Bỏ qua nếu không có dữ liệu
                }

                // 4. CẬP NHẬT LOGIC VÀ DANH SÁCH
                int rowIdInCol = boardCtrl.allTilesOnBoard[colId].Count;
                tile.columnId = colId;
                boardCtrl.allTilesOnBoard[colId].Add(tile);

                // 5. TÍNH TOÁN VỊ TRÍ LOCAL TRONG CỘT
                // posX = 0 vì tâm của đĩa trùng với tâm của cột
                float posY = -(rowIdInCol * spacingY);
                newObj.transform.localPosition = new Vector3(0, posY, 0);

                // 6. CẬP NHẬT SORTING ORDER (Theo hàng trong cột)
                tile.tray.sortingOrder = (10 - rowIdInCol) * 10;
                tile.icon.sortingOrder = (10 - rowIdInCol) * 10 + 1;

                // Đảm bảo trạng thái ban đầu
                tile.isLocked = false;
            }
        }

        boardCtrl.UpdateClickableStates();
    }
    // LOGIC CHÍNH CỦA GAME NẰM Ở ĐÂY
    public void LoadNextLevel()
    {


        Win_Pnl.gameObject.SetActive(false);
        isWin = false;
        isFail = false;
        // 1. Tăng chỉ số level
        currentLevelIndex++;

        // 2. Kiểm tra nếu đã chơi hết danh sách level (Tránh bị Apple Reject vì hết content)
        if (currentLevelIndex >= allLevels.Count)
        {
            // Quay lại chơi một level ngẫu nhiên để giữ chân người chơi
            currentLevelIndex = UnityEngine.Random.Range(8, allLevels.Count-1);
            Debug.Log("🔄 Chơi lại level ngẫu nhiên vì đã hết danh sách!");
        }

        // 3. Lưu tiến trình vào máy (Để tắt game mở lại vẫn ở level đó)
        GameData.SavedLevelIndex++;
        GameData.Save();

        // 4. Gọi hàm khởi tạo lại toàn bộ bàn chơi
        InitializeLevel(currentLevelIndex);


    }
    public void RestartLevel()
    {

        Win_Pnl.gameObject.SetActive(false);
        Fail_Pnl.gameObject.SetActive(false);
        isWin = false;
        isFail = false;
        // 2. Kiểm tra nếu đã chơi hết danh sách level (Tránh bị Apple Reject vì hết content)
        if (currentLevelIndex >= allLevels.Count)
        {
            // Quay lại chơi một level ngẫu nhiên để giữ chân người chơi
            currentLevelIndex = UnityEngine.Random.Range(8, allLevels.Count-1);
            //  Debug.Log("🔄 Chơi lại level ngẫu nhiên vì đã hết danh sách!");
        }
        // 4. Gọi hàm khởi tạo lại toàn bộ bàn chơi
        InitializeLevel(currentLevelIndex);

    }

    //Kiem tra hang doi: neu co cho leen bento
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
        StartCoroutine(ResetBuff());

    }
    IEnumerator ResetBuff()
    {
        yield return new WaitForSeconds(1.0f);


        if (!bufferCtrl.IsFull())
        {
            bufferCtrl.ResetWarning();
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

            if (gridCtrl.CheckFullBento())
            {
                ExecuteWinSequence();
            }
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
        if (tappedTile == null || tappedTile.IsClick) return;
        tappedTile.IsClick = true;
        OrderData currentOrder = orderCtrl.GetCurrentActiveOrder();
        if (currentOrder == null)
        {
            tappedTile.IsClick = false;
            return;

        }
        // Debug.Log("OnTileTapped");
        int colId = tappedTile.columnId; // Lấy ID cột của miếng vừa bấm
        // Lấy tọa độ đích cố định từ yêu cầu của Đơn hàng
        if (gridCtrl.GetFixedTargetCoord(tappedTile, currentOrder, out Vector2 targetCoord) && !tappedTile.data.isBuff)
        {
            //  Debug.Log("OnTileTapped");
            // TRƯỚC KHI REMOVE: Ghi lại dữ liệu Undo
            RecordUndoStep(tappedTile, colId);
            // Debug.Log("OnTileTapped");
            // 1. Xóa khỏi danh sách của cột đó
            boardCtrl.allTilesOnBoard[colId].Remove(tappedTile);
            // Debug.Log("OnTileTapped");
            // boardCtrl.RemoveTile(tappedTile);
            // Tính vị trí World để Tween biết đường bay tới (trước khi đổi cha)
            Vector3 targetWorldPos = gridCtrl.GetWorldPositionOfGrid(targetCoord);
            if (gridCtrl.CheckFullBento())
            {


                //Debug.Log("OnTileTapped");
                // Nếu đã đủ ô (occupiedCells >= totalCells), gọi hàm xử lý thắng
                // Lưu ý: Có thể delay một chút để đợi Tween cuối cùng bay tới nơi
                ExecuteWinSequence();
            }
            HideUpperTray(tappedTile.tray.gameObject, 0.3f, null);
            //Cho food bay leen luon o tren
            int sortingOrder = GameData.SortLayer;
            tappedTile.tray.sortingOrder = sortingOrder + 1;
            tappedTile.icon.sortingOrder = sortingOrder + 2;
            GameData.SortLayer += 2;
            GameData.Save();
            //  tappedTile.tray.transform.DOScale(Vector3.zero,0.5f);
            BentoTweenHelper.ParabolicMove(tappedTile.icon.transform, targetWorldPos, () =>
            {

                gridCtrl.PlaceTile(tappedTile, targetCoord);
                BentoTweenHelper.PackPopEffect(tappedTile.transform, () =>
                {
                    //    Debug.Log("Kiem tra " + IsStack);
                    tappedTile.IsClick = false;
                    if (!IsStack)
                    {
                        if (isWin)
                        {
                            boardCtrl.SetLockTiles();
                        }
                        else
                        {
                            ShiftTilesUpInColumn(colId);
                        }


                    }
                    else
                    {


                        if (isWin)
                        {
                            Debug.Log("Kiem tra " + IsStack);
                            GameManager.Instance.boardCtrl.SetLockTiles();
                        }
                        else
                        {
                            boardCtrl.UpdateClickableStates();
                        }
                    }

                    //  CheckWinCondition();
                });


            });
        }
        // 2. Nếu không vừa Bento, thử cho vào Khay chờ (Buffer)
        else if (!bufferCtrl.IsFull())
        {
            AudioManager.Instance.TriggerVibrate(30);
            //Debug.Log("HANG CHO");
            // TRƯỚC KHI REMOVE: Ghi lại dữ liệu Undo
            RecordUndoStep(tappedTile, colId);
            // boardCtrl.RemoveTile(tappedTile);
            boardCtrl.allTilesOnBoard[colId].Remove(tappedTile);
            bool isEmpty = bufferCtrl.AddToBuffer(tappedTile);
            if (isEmpty)
            {
                // tappedTile.tray.enabled = false;
                BuffSlot emptySlot = bufferCtrl.GetFirstEmptySlot();
                if (emptySlot != null)
                {
                    emptySlot.Occupy();
                    tappedTile.currentBuffSlot = emptySlot;
                    // Code xử lý hiệu ứng bay (Tweening) món ăn vào emptySlot.anchorPoint ở đây
                    //  Debug.Log("Món ăn đã vào slot!");

                    Vector3 bufferPos = emptySlot.anchorPoint.position;

                    HideUpperTray(tappedTile.tray.gameObject, 0.02f, () =>
                    {
                        BentoTweenHelper.ParabolicMove(tappedTile.transform, bufferPos);

                        tappedTile.IsClick = false;
                        tappedTile.isClickable = false;
                        if (!IsStack)
                            ShiftTilesUpInColumn(colId);
                        else
                        {

                            boardCtrl.UpdateClickableStates();
                        }

                    });
                }
                if (bufferCtrl.IsFull())
                {
                    bufferCtrl.PlayWarning();
                }

            }
            else
            {

                BentoTweenHelper.ErrorShake(tappedTile.transform);
                tappedTile.IsClick = false;

            }


        }
        else
        {

            // Trường hợp kẹt: Rung lắc báo lỗi
            BentoTweenHelper.ErrorShake(tappedTile.transform);
            tappedTile.IsClick = false;
            //Handheld.Vibrate();
            if (bufferCtrl.IsFull())
            {
                // Khóa tương tác
                isFail = true;
                CheckGameOver();
            }
        }

    }
    // HÀM TRỢ GIÚP ĐỂ GIỮ CODE SẠCH SẼ (CLEAN CODE)
    private void RecordUndoStep(FoodTile tile, int colId)
    {
        List<FoodTile> targetColumn = boardCtrl.allTilesOnBoard[colId];
        int currentIndex = targetColumn.IndexOf(tile);



        //    Debug.Log("RecordUndoStep currentIndex " + currentIndex);

        UndoStep step = new UndoStep
        {
            tile = tile,
            originalPosition = tile.transform.localPosition, // Lưu Local để khớp với logic Shift
            originalScale = tile.transform.localScale,
            columnId = colId,
            rowId = tile.rowId,
            layerId = tile.layerId,
            originalIndex = currentIndex,
            originalSortingOrder = tile.tray.sortingOrder,


        };

        boosterManager.RecordMove(step);
    }
    public void ShiftTilesUpInColumn(int colId)
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
    public void ShiftTilesLayerDownInColumn(int colId, int insertedIndex)
    {
        string sequenceId = "ShiftColumn_Undo_" + colId;
        DOTween.Kill(sequenceId);

        Sequence seq = DOTween.Sequence().SetId(sequenceId);

        List<FoodTile> targetColumn = boardCtrl.allTilesOnBoard[colId];

        for (int i = insertedIndex; i < targetColumn.Count; i++)
        {
            FoodTile tile = targetColumn[i];
            if (tile == null) continue;

            // TRA CỨU THEO DANH TÍNH VẬT LÝ
            string dnaKey = $"{colId}_{tile.rowId}_{tile.layerId}";

            if (boardCtrl.boardDNA.TryGetValue(dnaKey, out SlotData dna))
            {
                // Ép đĩa về đúng tọa độ XYZ và Sorting tuyệt đối
                seq.Join(tile.transform.DOLocalMove(dna.localPos, 0.4f).SetEase(Ease.OutQuad));

                tile.tray.sortingOrder = dna.sortingOrder;
                tile.icon.sortingOrder = dna.sortingOrder + 1;

                // Không cần gán lại rowId vì nó là định danh gốc rồi
            }
        }
        seq.OnComplete(() => boardCtrl.UpdateClickableStates());
    }
    public void ShiftTilesDownInColumn(int colId, int insertedIndex)
    {
        float spacingY = 2.0f;
        // Đảm bảo không có Sequence nào của cột này đang chạy chồng chéo
        string sequenceId = "ShiftColumn_Down_" + colId;
        DOTween.Kill(sequenceId);

        Sequence seq = DOTween.Sequence().SetId(sequenceId);
        List<FoodTile> targetColumn = boardCtrl.allTilesOnBoard[colId];

        // Duyệt từ vị trí được chèn trở đi để đẩy các đĩa phía sau xuống
        for (int i = insertedIndex; i < targetColumn.Count; i++)
        {
            FoodTile tile = targetColumn[i];
            if (tile == null) continue;

            // TÍNH TOÁN VỊ TRÍ Y MỚI
            float newPosY = -(i * spacingY);

            // FIX LỖI SAI COLUMN: Ép X = 0 vì đĩa đã là con của Transform Cột (Col_0, Col_1...)
            // Điều này đảm bảo đĩa không bị lệch sang trái/phải khi Undo
            Vector3 targetPos = new Vector3(0, newPosY, 0);

            // Thực hiện di chuyển mượt mà về vị trí mới
            seq.Join(tile.transform.DOLocalMove(targetPos, 0.4f)
                        .SetEase(Ease.OutQuad) // OutQuad tạo cảm giác lùi xuống có lực hơn
                        .SetTarget(tile)
                        .SetLink(tile.gameObject, LinkBehaviour.KillOnDestroy));

            // CẬP NHẬT THỨ TỰ HIỂN THỊ (Sorting Order)
            // Càng ở trên (index nhỏ) thì sorting order càng cao để đè lên miếng dưới
            int baseOrder = (10 - i) * 10;
            tile.tray.sortingOrder = baseOrder;
            tile.icon.sortingOrder = baseOrder + 1;

            // Đảm bảo đĩa biết mình đang thuộc cột nào sau khi quay về
            tile.columnId = colId;
        }

        // Sau khi toàn bộ Sequence hoàn tất, cập nhật khả năng Click
        seq.OnComplete(() =>
        {
            boardCtrl.UpdateClickableStates();
            //     Debug.Log($"<color=cyan>Column {colId} Re-aligned after Undo.</color>");
        });
    }


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
        isWin = true;

        // Đợi một khoảng ngắn (ví dụ 0.3s) để ô cuối cùng kịp bay vào khay rồi mới hiện UI thắng
        StartCoroutine(DelayedWin(0.5f));
    }

    IEnumerator DelayedWin(float delay)
    {

        // catAnimator.SetBool("isWin",true);
        //catAnimator.Play("happy");
        //yield return new WaitForSeconds(catAnimator.GetCurrentAnimatorStateInfo(0).length);

        // 3. Chuyển sang Animation thứ hai
        //catAnimator.Play("Idle");
        yield return new WaitForSeconds(delay);


        //   Debug.Log("Winner! Tất cả ô đã vào vị trí.");
        boosterManager.ClearStack();
        // isWin = true;

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
            // catAnimator.SetBool("isWin",true);
            catAnimator.Play("happy");
            Txt_Complete.gameObject.SetActive(true);
            //yield return new WaitForSeconds(catAnimator.GetCurrentAnimatorStateInfo(0).length);

            // 3. Chuyển sang Animation thứ hai
            //catAnimator.Play("Idle");
            isWin = true;
            OnLevelVictory();
        }
    }



    private void OnLevelVictory()
    {
        //   Debug.Log("🎉 CHIẾN THẮNG! Toàn bộ đơn hàng đã xong và bàn đã sạch.");
        AudioManager.Instance.Play("Vic");
        StartCoroutine(WinRoutine());

    }

    private IEnumerator WinRoutine()
    {

        // Chờ đến cuối frame để đảm bảo mọi render đã hoàn tất

        yield return new WaitForSeconds(1.5f);
        Booster.SetActive(false);
        Win_Pnl.gameObject.SetActive(true);
        catAnimator.Play("Idle");
        GameData.Stars += 1;
        WinScreenManager winScreenManager = Win_Pnl.GetComponent<WinScreenManager>();
        winScreenManager.ShowWinScreen();
    }
    public void CheckGameOver()
    {
        //     // Bế tắc: Khay chờ đầy VÀ không mảnh nào trong khay có thể đưa vào lưới Bento
        //     // -> Kích hoạt màn hình Thua cuộc hoặc gợi ý dùng Booster
        //     // 1. Lấy số lượng đĩa hiện có trong khay Bento (Grid)
        // Khóa tương tác

        //    processing = true; 
        StartCoroutine(PlayAnimationFailSequence());

    }

    IEnumerator PlayAnimationFailSequence()
    {
        AudioManager.Instance.Play("Retry");
        // 1. Chạy Animation thứ nhất
        catAnimator.Play("sad");

        // 2. Chờ cho đến khi Anim1 chạy xong
        // Chúng ta lấy độ dài của clip hiện tại ở Layer 0
        yield return new WaitForSeconds(catAnimator.GetCurrentAnimatorStateInfo(0).length);

        // 3. Chuyển sang Animation thứ hai

        yield return new WaitForSeconds(0.01f);
        int percent = orderCtrl.GetPerCent();
        Fail_Pnl.gameObject.SetActive(true);
        catAnimator.Play("Idle");
        if (bufferCtrl.IsFull())
        {
            bufferCtrl.ResetWarning();
        }
        FailScreenManager failScreenManager = Fail_Pnl.GetComponent<FailScreenManager>();
        failScreenManager.ShowFailScreen(percent);
    }

    public void ShowOrderActive(int orderActive, int totalOrder)
    {
        Txt_Order.text = "Order: " + orderActive + "/" + totalOrder;
    }

    public bool IsWinOrFail()
    {
        if (isWin || isFail)
        {
            return true;
        }

        return false;
    }

    public void HandleTouchDown(Vector2 touch)
    {
        //    Debug.Log("HandleTouchDown");
        buffSlot = null;
        if (isWin || isFail || IsProcessing)
            return;
        selectedTile = TileCloseToPoint(touch);
        if (selectedTile != null)
        {
            // Kiểm tra xem chuột có đang nằm trên UI không
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // Nếu có, dừng xử lý logic của Tile
                return;
            }
            if (boosterManager.isMagnet)
            {
                 Debug.Log("Đã click vào Tile!");
                if (selectedTile.typeTrayFood!=TypeTrayFood.Ice)
                    return;
                boosterManager.ExecuteMagnet(selectedTile);
                return; // Thoát hàm, không chạy logic nhặt đĩa bên dưới
            }
            // Logic xử lý khi click vào Tile của bạn ở đây
            //   Debug.Log("Đã click vào Tile!");
            if (selectedTile.isLocked) return;
            // Debug.Log("Đã click vào Tile! 1");
            // --- LOGIC BÚA (ƯU TIÊN HÀNG ĐẦU) ---
           
            // Debug.Log("Đã click vào Tile!");
            // 1. Nếu Tutorial đang bật
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsActive)
            {
                // Lấy vị trí chuột trên màn hình
                Vector2 mousePos = Input.mousePosition;

                // Tính khoảng cách từ chuột tới tâm cái lỗ
                float dist = Vector2.Distance(mousePos, TutorialManager.Instance.holeScreenPos);

                // Nếu khoảng cách LỚN HƠN bán kính (nghĩa là click vào vùng đen)
                if (dist > TutorialManager.Instance.holeRadius)
                {
                    Debug.Log("<color=red>Tutorial: Chặn click vào đĩa ngoài vùng sáng!</color>");
                    return; // Dừng hàm tại đây, không chạy code nhặt đĩa bên dưới
                }
            }
            // Debug.Log("Đã click vào Tile! 3");


            //            Debug.Log("Nhan " + selectedTile.isClickable);
            // Kiểm tra xem miếng Cá này có đang bị miếng khác đè không
            if (selectedTile.isClickable && !selectedTile.data.isBuff)
            {
                //Debug.Log("Đã click vào Tile! 4");
                AudioManager.Instance.Play("Click");
                // Thông báo cho TutorialManager đã click đúng
                if (TutorialManager.Instance != null)
                    TutorialManager.Instance.OnTilePicked(selectedTile);
                // Gửi thông tin miếng Cá này sang bộ não trung tâm
                // Trước khi bay vào Bento, ghi lại thông tin để Undo
                OnTileTapped(selectedTile);


            }
            else if (isWin || selectedTile.data.isBuff)
            {
                // Debug.Log("NO Nhan ");
                AudioManager.Instance.Play("Error");
                // Hiệu ứng rung lắc nhẹ báo hiệu miếng Cá đang bị kẹt
                BentoTweenHelper.ErrorShake(selectedTile.transform);
            }
        }
        else
        {

            buffSlot = BuffSlotCloseToPoint(touch);

            if (buffSlot != null && buffSlot.isUnlocked && !buffSlot.isBuyed)
            {
                AudioManager.Instance.Play("Click");
                boosterManager.ShowBuySlot(buffSlot.index);
            }

        }


    }
    public void PlayClick()
    {
        AudioManager.Instance.Play("Click");
    }

    public void HandleTouchUp(Vector2 touch)
    {
        //throw new NotImplementedException();
    }

    public void HandleTouchMove(Vector2 touch)
    {
        //throw new NotImplementedException();
    }
    private FoodTile TileCloseToPoint(Vector2 point)
    {
        var worldPoint = Camera.main.ScreenToWorldPoint(point);
        worldPoint.z = 0; // Đưa về mặt phẳng 2D để tính khoảng cách ngang

        FoodTile bestTile = null;
        float minDistance = float.MaxValue;
        int bestLayer = int.MaxValue; // Để tìm thằng trên cùng

        float threshold = Mathf.Min(tileWSpacing, tileHSpacing) * 0.6f; // Tăng nhẹ vùng chạm cho cảm giác mượt hơn
        List<List<FoodTile>> allTilesOnBoard = boardCtrl.allTilesOnBoard;

        for (int colId = 0; colId < allTilesOnBoard.Count; colId++)
        {
            for (int i = 0; i < allTilesOnBoard[colId].Count; i++)
            {
                FoodTile food = allTilesOnBoard[colId][i];

                // 1. Kiểm tra tồn tại và Active (dùng ?. để an toàn)
                if (food == null || !food.gameObject.activeInHierarchy) continue;

                float distance = Vector2.Distance(food.transform.position, worldPoint);

                // 2. Chỉ xét các đĩa nằm trong vùng chạm
                if (distance < threshold)
                {
                    /* LOGIC CTO: ƯU TIÊN LAYER TRƯỚC, KHOẢNG CÁCH SAU
                       - Ưu tiên đĩa cho phép bấm (isClickable).
                       - Nếu cùng có thể bấm, đĩa nào Layer thấp hơn (nằm trên) thắng.
                       - Nếu cùng Layer, đĩa nào gần tâm hơn thắng.
                    */
                    bool isBetter = false;

                    if (bestTile == null)
                    {
                        isBetter = true;
                    }
                    else
                    {
                        // Nếu đĩa này bấm được mà đĩa cũ không bấm được -> Lấy ngay
                        if (food.isClickable && !bestTile.isClickable) isBetter = true;
                        // Nếu cả hai cùng trạng thái bấm -> So sánh Layer
                        else if (food.isClickable == bestTile.isClickable)
                        {
                            if (food.layerId < bestLayer) isBetter = true;
                            else if (food.layerId == bestLayer && distance < minDistance) isBetter = true;
                        }
                    }

                    if (isBetter)
                    {
                        minDistance = distance;
                        bestLayer = food.layerId;
                        bestTile = food;
                    }
                }
            }
        }

        return bestTile;
    }
    private FoodTile TileCloseToPoint1(Vector2 point)
    {
        var worldPoint = Camera.main.ScreenToWorldPoint(point);
        worldPoint.z = 0;

        FoodTile closestTile = null;
        float closestDistance = float.MaxValue;
        List<List<FoodTile>> allTilesOnBoard = boardCtrl.allTilesOnBoard;
        // Duyệt qua tất cả các tile để tìm tile gần nhất
        for (int colId = 0; colId < allTilesOnBoard.Count; colId++)
        {
            for (int i = 0; i < allTilesOnBoard[colId].Count; i++)
            {
                FoodTile food = allTilesOnBoard[colId][i];

                if (food != null && food.gameObject.transform.parent.gameObject.activeSelf)
                {

                    float distance = Vector3.Distance(food.transform.position, worldPoint);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTile = food;
                    }
                }

            }
        }
        // Chỉ trả về tile nếu nó đủ gần (trong phạm vi nửa tile)
        if (closestTile != null && closestDistance < Mathf.Min(tileWSpacing, tileHSpacing) * 0.5f)
        {

            return closestTile;
        }

        return null;
    }
    //Buffer
    private BuffSlot BuffSlotCloseToPoint(Vector2 point)
    {
        var worldPoint = Camera.main.ScreenToWorldPoint(point);
        worldPoint.z = 0;

        BuffSlot closestTile = null;
        float closestDistance = float.MaxValue;
        List<BuffSlot> allTilesOnBoard = bufferCtrl.allSlots;
        // Duyệt qua tất cả các tile để tìm tile gần nhất
        for (int i = 0; i < allTilesOnBoard.Count; i++)
        {
            BuffSlot buffSlot = allTilesOnBoard[i];

            if (buffSlot != null && buffSlot.gameObject.transform.parent.gameObject.activeSelf)
            {
                float distance = Vector3.Distance(buffSlot.transform.position, worldPoint);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTile = buffSlot;
                }
            }

        }

        // Chỉ trả về tile nếu nó đủ gần (trong phạm vi nửa tile)
        if (closestTile != null && closestDistance < Mathf.Min(tileWSpacing, tileHSpacing) * 0.5f)
        {

            return closestTile;
        }

        return null;
    }

    public void OpenSetting(bool display)
    {
        if (IsWinOrFail())
            return;
        if (IsProcessing)
            return;
        AudioManager.Instance.Play("Click");
        Pnl_Setting.SetActive(display);
    }
    public void OpenRestart(bool display)
    {
        if (IsWinOrFail())
            return;
        if (IsProcessing)
            return;
        AudioManager.Instance.Play("Click");
        Pnl_Restart.SetActive(display);
    }
    public void OpenShop(bool display)
    {
        if (IsWinOrFail())
            return;
        if (IsProcessing)
            return;
        AudioManager.Instance.Play("Click");
        Pnl_Shop.SetActive(display);
    }

    public void RestartGame()
    {
        AudioManager.Instance.Play("Click");
        if (isHome) //ve home
        {
            isHome = false;
            Pnl_Restart.SetActive(false);
            SceneManager.LoadSceneAsync("Home");
        }
        else
        {
            if (GameData.Heart > 0)
            {
                GameData.Heart -= 1;
                GameData.Save();
                RestartLevel();
                Pnl_Restart.SetActive(false);
            }
            else
            {

                Pnl_Restart.SetActive(false);
                SceneManager.LoadSceneAsync("Home");
            }
        }

    }
    public void GotoHome()
    {
        AudioManager.Instance.Play("Click");
        isHome = true;
        Pnl_Setting.SetActive(false);
        Pnl_Restart.SetActive(true);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // GameData.Coins = 10000;
            // GameData.Save();
            GameManager.Instance.boardCtrl.SetLockTiles();

        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Win_Pnl.gameObject.SetActive(true);
            WinScreenManager winScreenManager = Win_Pnl.GetComponent<WinScreenManager>();
            winScreenManager.ShowWinScreen();
        }


    }
}