
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//XU ly tang 3: cacs mon an xep cuoi cung de chon
public class MahjongBoardController : MonoBehaviour
{
    // Lưu trữ "vân tay" của từng vị trí: [Cột_Tầng] -> (Tọa độ, Sorting)
    [HideInInspector]
    // Dùng Vector2Int nhanh hơn string key và dễ quản lý tọa độ (x=col, y=row)

    public struct SlotData
    {
        public Vector3 localPos;
        public int sortingOrder;
        public int layerId; // Nên lưu cả LayerId để Undo không bị sáng/tối sai
        public int rowId;
    }

    // Thay đổi Dictionary sang dùng string làm Key
    public Dictionary<string, SlotData> boardDNA = new Dictionary<string, SlotData>();

    public void CaptureBoardDNA()
    {
        if (boardDNA == null) boardDNA = new Dictionary<string, SlotData>();
        boardDNA.Clear();

        // Duyệt qua toàn bộ cột
        for (int c = 0; c < allTilesOnBoard.Count; c++)
        {
            for (int i = 0; i < allTilesOnBoard[c].Count; i++)
            {
                var tile = allTilesOnBoard[c][i];
                if (tile == null) continue;

                // QUAN TRỌNG: Key phải là sự kết hợp của ColumnID, RowID (Chồng) và LayerID (Tầng)
                // Đây là "tọa độ chết" của slot đó trên bàn chơi
                string key = $"{c}_{tile.rowId}_{tile.layerId}";

                boardDNA[key] = new SlotData
                {
                    localPos = tile.transform.localPosition,
                    sortingOrder = tile.tray.sortingOrder,
                    layerId = tile.layerId,
                    rowId = tile.rowId
                };
            }
        }
        //  Debug.Log($"<color=cyan>DNA Captured: {boardDNA.Count} physical slots locked!</color>");
    }

    // List ngoài cùng đại diện cho các Cột (Columns)
    // List bên trong đại diện cho các Đĩa (Tiles) xếp chồng trong cột đó
    //allTilesOnBoard.Count: Trả về Tổng số cột đang có trên bàn (Ví dụ: 3, 4 hoặc 5 cột tùy Level).

    //allTilesOnBoard[colId].Count: Trả về Số lượng đĩa đang xếp chồng trong cột cụ thể đó.
    public List<List<FoodTile>> allTilesOnBoard = new List<List<FoodTile>>();
    public Transform[] columns;
    private void Awake()
    {
        // Khởi tạo List cho từng cột
        // if(GameData.SavedLevelIndex<=21)
        //     InitializeBoard(3);
        // else
        // { 
        //     InitializeBoard(5);
            
        // }
         InitializeBoard(5);
    }

    public void InitializeBoard(int numColumns)
    {
        // Clear sạch các cột cũ
        allTilesOnBoard.Clear();

        // Khởi tạo các List con tương ứng với số cột của Level đó
        for (int i = 0; i < numColumns; i++)
        {
            allTilesOnBoard.Add(new List<FoodTile>());
        }
        





    }

    public void ResizeBoard(bool isColumn5)
    {

        if (isColumn5)
        {
            columns[0].localPosition=new Vector3(-4,0,0);
            columns[1].localPosition=new Vector3(-2,0,0);
            columns[2].localPosition=new Vector3(0,0,0);
            columns[3].localPosition=new Vector3(2,0,0);
            columns[4].localPosition=new Vector3(4,0,0);

        }
        else
        {
            columns[0].localPosition=new Vector3(-2,0,0);
            columns[1].localPosition=new Vector3(0,0,0);
            columns[2].localPosition=new Vector3(2,0,0);
        }




    }
    // RẤT QUAN TRỌNG: Thuật toán kiểm tra xem mảnh nào được phép nhấn
    public void UpdateClickableStates2()
    {

        for (int colId = 0; colId < 3; colId++)
        {
            for (int i = 0; i < allTilesOnBoard[colId].Count; i++)
            {
                FoodTile tile = allTilesOnBoard[colId][i];

                // Chỉ miếng index 0 trong mỗi cột mới được bấm
                tile.isClickable = (i == 0);

                // Hiệu ứng Visual (Sáng/Tối)
                Color targetColor = tile.isClickable ? Color.white : Color.gray;
                // tile.tray.GetComponent<SpriteRenderer>().DOColor(targetColor, 0.2f);
                BentoTweenHelper.SafeDOColor(tile.tray.GetComponent<SpriteRenderer>(), targetColor, 0.2f);

            }
        }
    }

    public void UpdateClickableStates()
    {
       // Debug.Log("UpdateClickableStates");
        if (!GameManager.Instance.IsStack)
        {
            
            // Duyệt qua từng cột đĩa trên bàn
            for (int colId = 0; colId < allTilesOnBoard.Count; colId++)
            {
                var currentColumn = allTilesOnBoard[colId];
                int count = currentColumn.Count;

                for (int i = 0; i < count; i++)
                {
                    FoodTile tile = currentColumn[i];

                    // LOGIC QUAN TRỌNG: 
                    // LOGIC MỚI: Chỉ đĩa ở đầu danh sách (Index 0) mới được tương tác
                    bool isTopItem = (i == 0);
                    tile.isClickable = isTopItem;

                    // 1. Bật/Tắt Collider để chống bấm xuyên tầng
                    BoxCollider2D col = tile.GetComponent<BoxCollider2D>();
                    if (col != null)
                    {
                        col.enabled = isTopItem;
                    }

                    // 2. Hiệu ứng Visual (Làm tối các khay ở dưới để người chơi dễ nhận biết)
                    Color targetColor = isTopItem ? Color.white : new Color(0.6f, 0.6f, 0.6f, 1f);

                    // Sử dụng Helper để đổi màu mượt mà
                    BentoTweenHelper.SafeDOColor(tile.tray.GetComponent<SpriteRenderer>(), targetColor, 0.2f);
                    BentoTweenHelper.SafeDOColor(tile.icon, targetColor, 0.2f);
                }
            }
        }
        else
        {
            
            // Duyệt qua từng cột đĩa trên bàn
            for (int colId = 0; colId < allTilesOnBoard.Count; colId++)
            {
                var currentColumn = allTilesOnBoard[colId];
                if (currentColumn.Count == 0) continue;

                // 1. Tìm giá trị Layer nhỏ nhất đang còn tồn tại trong cột này (ví dụ: 0)

                // 2. Gom nhóm theo Chồng (Stack)
                //  1. Gom nhóm theo rowId (Mỗi nhóm là 1 chồng đĩa)
                var stacks = currentColumn.GroupBy(t => t.rowId).ToList();

                // 2. Tìm giá trị Layer nhỏ nhất trong số các đĩa đang nằm ở ĐỈNH của mỗi chồng
                // Chúng ta chỉ quan tâm đến những đĩa lộ diện (Index 0 của mỗi stack)


                foreach (var stackGroup in stacks)
                {
                    // 3. Lấy danh sách đĩa trong chồng và sắp xếp theo Layer
                    var tilesInStack = stackGroup.OrderBy(t => t.layerId).ToList();

                    // Đĩa ở Index 0 chính là đĩa có Layer nhỏ nhất của CHỒNG này (Local Min)
                    int minLayerInCol = tilesInStack[0].layerId;
                    for (int i = 0; i < tilesInStack.Count; i++)
                    {
                        FoodTile tile = tilesInStack[i];

                        // ĐIỀU KIỆN TƯƠNG TÁC (QUY TẮC CTO):
                        // - Đĩa phải thuộc Layer thấp nhất hiện có (minLayerInCol)
                        // - Đĩa phải nằm trên cùng của chồng đó (Index 0)
                        bool isLowestLayerNow = (tile.layerId == minLayerInCol);
                        bool isTopOfItsStack = (i == 0);
                        bool canInteract = isLowestLayerNow && isTopOfItsStack;

                        tile.isClickable = canInteract;

                        // 1. Collider: Rất quan trọng để không bấm nhầm vào Layer cao hơn
                        BoxCollider2D col = tile.GetComponent<BoxCollider2D>();
                        if (col != null) col.enabled = canInteract;

                        // 2. Hiển thị Visual (Trạng thái Highlight)
                        Color targetColor;
                        if (canInteract && tile.typeTrayFood == TypeTrayFood.None)
                        {
                            targetColor = Color.white; // Sáng rực để biết là bấm được
                        }
                        else if (isTopOfItsStack)
                        {
                            // Là đỉnh chồng nhưng Layer cao hơn -> Chờ dọn tầng dưới
                            targetColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                        }
                        else
                        {
                            // Bị đè hoàn toàn bên dưới
                            targetColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                        }
                        // 1. Cấu hình hằng số (Nên để Layer 0 gần Camera nhất)
                        float zStep = 0.5f;       // Khoảng cách giữa các tầng Layer (tăng lên để phân lớp rõ rệt)
                        float subStep = 0.02f;    // Khoảng cách giữa các đĩa trong cùng 1 xấp (rowId)

                        // 2. Tính toán targetZ
                        // Công thức: Layer càng lớn (càng sâu) thì Z càng lớn (càng xa Camera)
                        // Ví dụ: 
                        // Layer 0 (Trên cùng) -> targetZ = (0 * 0.5) + (rowId * 0.02) = ~0 (Gần Camera)
                        // Layer 2 (Dưới sâu)  -> targetZ = (2 * 0.5) + (rowId * 0.02) = ~1.0 (Xa Camera)
                        float targetZ = (tile.layerId * zStep) + (tile.rowId * subStep);

                        // 3. Cập nhật vị trí
                        Vector3 currentPos = tile.transform.localPosition;

                        // Chỉ cập nhật nếu có sự thay đổi đáng kể (> 0.001) để tối ưu hiệu năng
                        if (Mathf.Abs(currentPos.z - targetZ) > 0.001f)
                        {
                            tile.transform.localPosition = new Vector3(currentPos.x, currentPos.y, targetZ);
                        }
                        // 3. Hiệu ứng bừng sáng (Feedback)
                        // if (canInteract && tile.icon.color != Color.white)
                        // {
                        //     // Hiệu ứng "Pop" báo hiệu tầng này đã được mở khóa
                        //     tile.transform.DOScale(1.1f, 0.15f).SetLoops(2, LoopType.Yoyo);
                        // }

                        BentoTweenHelper.SafeDOColor(tile.tray.GetComponent<SpriteRenderer>(), targetColor, 0.2f);
                        BentoTweenHelper.SafeDOColor(tile.icon, targetColor, 0.2f);

                        // // 4. Sorting Order: Giữ đúng phối cảnh Layer (Lớn nằm trên nhỏ)
                        // int depthPriority = tilesInStack.Count - i;
                        // tile.tray.GetComponent<SpriteRenderer>().sortingOrder = (tile.layer * 100) + depthPriority;
                        // tile.icon.sortingOrder = (tile.layer * 100) + depthPriority + 1;
                    }
                }
            }
        }

    }
    // private bool IsCovered(FoodTile checkTile)
    // {
    //     // 1. Lấy vị trí Local để so sánh chính xác hơn World
    //     Vector3 checkPos = checkTile.transform.localPosition;

    //     // 2. Lấy kích thước Collider (nên cache size này lại để tối ưu)
    //     Vector2 size = checkTile.GetComponent<BoxCollider2D>().size;

    //     foreach (FoodTile otherTile in allTilesOnBoard)
    //     {
    //         if (otherTile == checkTile) continue;

    //         Vector3 otherPos = otherTile.transform.localPosition;

    //         // LOGIC Z: Miếng ở trên có Z nhỏ hơn (ví dụ -1 < 0)
    //         // Ta dùng một khoảng sai số nhỏ (0.01f) để tránh lỗi làm tròn số thực
    //         if (otherPos.z < checkPos.z - 0.01f)
    //         {
    //             // 3. Kiểm tra va chạm diện tích (Overlap) theo trục X và Y
    //             // Dùng 0.95f để người chơi dễ bấm hơn (chuẩn Voodoo)
    //             bool isOverlapX = Mathf.Abs(otherPos.x - checkPos.x) < size.x * 0.95f;
    //             bool isOverlapY = Mathf.Abs(otherPos.y - checkPos.y) < size.y * 0.95f;

    //             if (isOverlapX && isOverlapY)
    //             {
    //                 // Debug.Log($"{otherTile.name} (Z:{otherPos.z}) ĐÈ LÊN {checkTile.name} (Z:{checkPos.z})");
    //                 return true;
    //             }
    //         }
    //     }
    //     return false;
    // }
    // private bool IsCovered1(FoodTile checkTile)
    // {
    //     // Lấy Bounds (khung bao) 2D của mảnh hiện tại
    //     Bounds checkBounds = checkTile.GetComponent<BoxCollider2D>().bounds;

    //     foreach (FoodTile otherTile in allTilesOnBoard)
    //     {
    //         Debug.Log("UpdateClickableStates 1");
    //         if (otherTile == checkTile) continue;
    //         Debug.Log("UpdateClickableStates 2");
    //         // Nếu mảnh khác có Layer (Z-index) cao hơn VÀ khung bao đè lên nhau
    //         if (otherTile.transform.position.z < checkTile.transform.position.z)
    //         {
    //             Debug.Log("UpdateClickableStates 3");
    //             Bounds otherBounds = otherTile.GetComponent<BoxCollider2D>().bounds;
    //             if (checkBounds.Intersects(otherBounds))
    //             {
    //                 Debug.Log("giao");
    //                 return true;
    //             }
    //         }
    //     }
    //     return false;
    // }
    // // Gọi khi một mảnh bị nhặt đi
    // // public void RemoveTileFromBoard(FoodTile tile)
    // // {
    // //     activeTilesOnBoard.Remove(tile);
    // //     UpdateClickableStates(); // Cập nhật lại trạng thái các mảnh bên dưới

    // //     // Kiểm tra Win Condition ở Tầng 3
    // //     if (activeTilesOnBoard.Count == 0)
    // //     {
    // //         //GameEvents.TriggerBoardCleared();
    // //     }
    // // }

    public void RemoveTile(FoodTile tile)
    {

        UpdateClickableStates();
    }
    // --- HÀM KHÓA/MỞ KHÓA TOÀN BỘ ĐĨA ---
    public void LockAllTiles(bool shouldLock)
    {
        // Duyệt qua 3 cột (Index 0, 1, 2)
        for (int i = 0; i < allTilesOnBoard.Count; i++)
        {
            if (allTilesOnBoard[i] == null) continue;

            // Duyệt qua từng đĩa FoodTile trong danh sách của cột đó
            foreach (FoodTile tile in allTilesOnBoard[i])
            {
                if (tile != null)
                {
                    // Gán trạng thái khóa cho từng đĩa
                    tile.isLocked = shouldLock;

                    // (Tùy chọn) Hiệu ứng thị giác: Làm tối đĩa bị khóa
                    SpriteRenderer sr = tile.tray.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.color = shouldLock ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;
                    }
                }
            }
        }

//        Debug.Log(shouldLock ? "Đã khóa toàn bộ đĩa trên bàn." : "Đã mở khóa toàn bộ đĩa.");
    }
     public int SizeBoard()
    {


        int count=0;
        for (int i = 0; i < allTilesOnBoard.Count; i++)
        {
            if (allTilesOnBoard[i] == null) continue;

            // Duyệt qua từng đĩa FoodTile trong danh sách của cột đó
            foreach (FoodTile tile in allTilesOnBoard[i])
            {
                if (tile != null)
                {
                   count++;
                }
            }
        }
        return count;
    }

    public void SetLockTiles()
    {

//Debug.Log("SetLockTiles");

        for (int i = 0; i < allTilesOnBoard.Count; i++)
        {
            if (allTilesOnBoard[i] == null) continue;

            // Duyệt qua từng đĩa FoodTile trong danh sách của cột đó
            foreach (FoodTile tile in allTilesOnBoard[i])
            {
                if (tile != null)
                {
                    tile.isClickable = false;
                    Color dimColor = new Color(0.6f, 0.6f, 0.6f, 1f);
                    tile.tray.color = dimColor;
                    tile.icon.color = dimColor; // Dùng trực tiếp .color nếu là SpriteRenderer
                }
            }
        }

        

    }

     public bool CheckFrozenTray()
    {



        for (int i = 0; i < allTilesOnBoard.Count; i++)
        {
            if (allTilesOnBoard[i] == null) continue;

            // Duyệt qua từng đĩa FoodTile trong danh sách của cột đó
            foreach (FoodTile tile in allTilesOnBoard[i])
            {
                if (tile != null)
                {
                   if(tile.typeTrayFood==TypeTrayFood.Ice && tile.isClickable)
                   return true;
                }
            }
        }
        return false;

        

    }

     public  List<FoodTile> GetListLockTiles()
    {

        List<FoodTile> foodTiles=new List<FoodTile>();

        for (int i = 0; i < allTilesOnBoard.Count; i++)
        {
            if (allTilesOnBoard[i] == null) continue;

            // Duyệt qua từng đĩa FoodTile trong danh sách của cột đó
            foreach (FoodTile tile in allTilesOnBoard[i])
            {
                if (tile != null)
                {
                   if(tile.typeTrayFood==TypeTrayFood.Lock)
                  foodTiles.Add(tile);
                }
            }
        }
        
return foodTiles;
        

    }
}