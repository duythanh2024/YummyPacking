using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

//Quản lý lưới không gian Tầng 2: xep do an vo khay bento lon
public class BentoGridController : MonoBehaviour
{
    private int[,] gridCells; // Ma trận 2D lưu trạng thái lưới (0: Trống, 1: Có đồ ăn)
    private List<FoodTile> packedTiles; // Các món đã xếp vào hộp
                                        // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int gridWidth;
    private int gridHeight;
    [Header("--- GRID VISUAL SETUP ---")]
    [Tooltip("Kích thước của một ô vuông (ví dụ: 1.0 cho 1 đơn vị Unity)")]
    public float cellSize = 1.1f;
    public Transform BentoPos;
    private int occupiedCells = 0;
    private int totalCells;

    [Header("--- PREFABS & POSITIONS ---")]
    // public GameObject bentoPrefab;      // Prefab cái khay gỗ
    public Transform spawnPoint;        // Điểm xuất phát (Bên phải màn hình)
    public Transform centerPoint;       // Điểm chơi chính (Giữa màn hình)
    public Transform exitPoint;         // Điểm biến mất (Bên trái màn hình)

    [Header("--- SETTINGS ---")]
    public float moveDuration = 0.5f;   // Tốc độ bay của khay

    private GameObject currentBentoUI;  // Khay đang nằm trên màn hình
    public int typeTray;

    public void InitializeGrid(int width, int height, int typeTray)
    {
        // Gán giá trị vào biến thành viên
        this.gridWidth = width;
        this.gridHeight = height;
        this.typeTray = typeTray;
        gridCells = new int[width, height];
        totalCells = gridWidth * gridHeight;
        occupiedCells = 0;
        packedTiles = new List<FoodTile>();
        // Debug.Log("SpawnNewBento  this.typeTray " + this.typeTray);
        SpawnNewBento();

    }

    void ResetData(int width, int height, int typeTray)
    {

    }

    public string GetTagTray()
    {
        String tagTray = "";
        if (this.typeTray == 0)
        {
            tagTray = "Tray";

        }
        else if (this.typeTray == 1)
        {
            tagTray = "Tray33";
        }
        else if (this.typeTray == 2)
        {
            tagTray = "Tray66";
        }

        return tagTray;
    }
    private void SpawnNewBento()
    {

        if (GameManager.Instance.orderCtrl.activeOrders.Count > 0)
        {
            this.gridWidth = GameManager.Instance.orderCtrl.activeOrders[0].targetGridSize.x;
            this.gridHeight = GameManager.Instance.orderCtrl.activeOrders[0].targetGridSize.y;
            this.typeTray = GameManager.Instance.orderCtrl.activeOrders[0].typeTray;

        }
        Debug.Log("SpawnNewBento  this.typeTray " + this.typeTray + "Tag: " + GetTagTray());
        gridCells = new int[gridWidth, gridHeight];

        totalCells = gridWidth * gridHeight;
        occupiedCells = 0;
        // Tạo khay mới tại điểm Spawn
        // currentBentoUI = Instantiate(bentoPrefab, spawnPoint.position, Quaternion.identity, BentoPos);
        String tagTray = GetTagTray();


        currentBentoUI = ObjectPooler.Instance.SpawnFromPool(tagTray, spawnPoint.position, Quaternion.identity);
        currentBentoUI.transform.SetParent(BentoPos);
        TrayOrder trayOrder = currentBentoUI.GetComponent<TrayOrder>();
        trayOrder.SetDefault();
        // Bay từ phải vào giữa
        BentoTweenHelper.SafeMove(currentBentoUI.transform, centerPoint.position, moveDuration, Ease.OutBack, () =>
        {
            GameManager.Instance.CheckBuff();
        });

        // Reset các dữ liệu logic của Grid tại đây (ví dụ: mảng gridCells = 0)

    }


    // 2. Hàm gọi khi hoàn thành 1 đơn hàng (CompleteOrder)
    public void MoveOutAndRefresh()
    {
        if (currentBentoUI == null) return;

        GameObject oldBento = currentBentoUI;

        TrayOrder trayOrder = oldBento.GetComponent<TrayOrder>();
        if (trayOrder != null)
        {

            trayOrder.locks.SetActive(true);
            AudioManager.Instance.Play("WoodClack");

            Vector2 targetPos = new Vector2(0, trayOrder.locks.transform.localPosition.y);

            BentoTweenHelper.SafeDOLocalMove(trayOrder.locks.transform, targetPos, moveDuration, Ease.OutQuad, () =>
            {
                // 3. HIỆU ỨNG VA CHẠM (IMPACT SHAKE)
                // Vì nắp bay từ trái qua, cú va chạm sẽ làm hộp rung nhẹ theo chiều NGANG (trục X)
                // transform.DOShakePosition(0.15f, new Vector3(0.1f, 0, 0), 10, 90f);

                BentoTweenHelper.SafeDOShakePosition(transform, 0.15f, new Vector3(0.1f, 0, 0), 10, 90f);




                // Nắp hộp nhún nhẹ (Squash) theo chiều ngang để tạo cảm giác "hít" vào
                BentoTweenHelper.SafeDOScale(transform, new Vector3(0.95f, 1.05f, 1f), 0.1f, LoopType.Yoyo);
                // trayOrder.locks.transform.DOScale(new Vector3(0.95f, 1.05f, 1f), 0.1f).SetLoops(2, LoopType.Yoyo);

                // GỌI PARTICLE & SFX TẠI ĐÂY
                // ParticlePool.Instance.Play("LidSlideImpact", transform.position); // Particle tỏa dọc
                // AudioSource.PlayClipAtPoint(lidCloseSound, Camera.main.transform.position);

                // 4. BAY RA KHỎI MÀN HÌNH (EXPORT)
                // Đợi một chút để người chơi kịp nhìn thấy hộp đã đóng hoàn chỉnh
                DOVirtual.DelayedCall(0.1f, () =>
                 {
                     // Khay cũ bay qua trái và tự hủy
                     BentoTweenHelper.SafeMove(oldBento.transform, exitPoint.position, moveDuration, Ease.InBack, () =>
                     {
                         //Lay food    
                         for (int i = oldBento.transform.childCount - 1; i >= 0; i--)
                         {
                             Transform child = oldBento.transform.GetChild(i);

                             // So sánh Tag dùng CompareTag sẽ nhanh hơn và tránh lỗi chính tả
                             if (child.CompareTag("Food"))
                             {
                                 // Trả về pool (thường sẽ làm mất child này khỏi parent hiện tại)
                                 ObjectPooler.Instance.ReturnToPool("Food", child.gameObject);
                                 FoodTile foodTile = child.gameObject.GetComponent<FoodTile>();
                                 foodTile.SetDefault();
                             }
                         }
                         Debug.Log("AN TRAY");
                         ObjectPooler.Instance.ReturnToPool(GetTagTray(), oldBento);
                         //  Destroy(oldBento);
                         // Xóa các Object cũ trong khay Bento
                         // Kiểm tra xem còn đơn hàng nào không (Logic từ OrderManager)
                         if (GameManager.Instance.orderCtrl.activeOrders.Count > 0)
                         {

                             SpawnNewBento();
                             GameManager.Instance.isWin = false;
                         }
                         else
                         {
                             //Kiem tra

                             GameManager.Instance.CheckWin();
                         }

                     });

                     //    oldBento.transform.DOMove(exitPoint.position, moveDuration).SetEase(Ease.InBack).OnComplete(() =>
                     //     {
                     //         Destroy(oldBento);
                     //         // Xóa các Object cũ trong khay Bento
                     //         // Kiểm tra xem còn đơn hàng nào không (Logic từ OrderManager)
                     //         if (GameManager.Instance.orderCtrl.activeOrders.Count > 0)
                     //         {
                     //             SpawnNewBento();

                     //         }
                     //         else
                     //         {
                     //             //Kiem tra

                     //             GameManager.Instance.CheckWin();
                     //         }

                     //     });
                 });
            });

            //     trayOrder.locks.transform.DOLocalMove(Vector2.zero, moveDuration).SetEase(Ease.OutQuad).OnComplete(() =>
            //    {








            //    });
        }

    }
    public Vector3 GetWorldPositionOfGrid(Vector2 coord)
    {

        float localX = coord.x * cellSize;
        float localY = coord.y * cellSize;
        return currentBentoUI.transform.TransformPoint(new Vector3(localX, localY, 0));

        // Giả sử (0,0) là tâm của Grid, tính toán Local Offset


        // Chuyển tọa độ Local này sang World dựa trên Transform của BentoGrid (D)
        // transform ở đây chính là đối tượng D (khay bento)
        //  return transform.TransformPoint(new Vector3(localX, localY, 0));
        // // Debug.Log(coord);
        // // Với lưới 3x3, cellSize = 1, tâm tổng thể là (0,0):
        // // Cột (coord.x): 0 -> -1 | 1 -> 0 | 2 -> 1
        // // Hàng (coord.y): 0 -> 1  | 1 -> 0 | 2 -> -1

        // float startX = 0f; // Tâm của cột đầu tiên (bên trái)
        // float startY = 0f;  // Tâm của hàng đầu tiên (trên cùng)

        // float localX = startX + (coord.x * cellSize);
        // float localY = startY - (coord.y * cellSize);

        // // Chuyển từ Local sang World để Tween bay đúng vị trí
        // return transform.TransformPoint(new Vector3(coord.x, coord.y, 0));
    }
    // THUẬT TOÁN AUTO-FIT: Tìm xem mảnh đồ ăn có nhét vừa chỗ nào trong lưới không
    public bool TryAutoFitTile(FoodTile tile, out Vector2Int targetCoord)
    {
        targetCoord = new Vector2Int(-1, -1);
        List<Vector2Int> blocks = tile.data.shapeBlocks;

        // Bây giờ gridHeight và gridWidth đã tồn tại
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (CanPlaceShapeAt(x, y, blocks))
                {
                    targetCoord = new Vector2Int(x, y);
                    return true;
                }
            }
        }
        return false;
    }
    public bool GetFixedTargetCoord(FoodTile tile, OrderData currentOrder, out Vector2 targetCoord)
    {
        targetCoord = new Vector2(-1, -1);
        List<FoodPlacement> requiredLayouts = currentOrder.requiredLayout;
        //       Debug.Log(requiredLayouts.Count);

        foreach (var placement in requiredLayouts)
        {
            // Debug.Log(placement.gridCoord);
            // Nếu loại món khớp (ví dụ: "Rice") 
            // VÀ ô đó trong lưới thực tế đang trống (gridCells == 0)
            if (placement.foodType == tile.data.foodType &&
                gridCells[placement.gridCoord.x, placement.gridCoord.y] == 0)
            {

                targetCoord = placement.gridValues;

                gridCells[placement.gridCoord.x, placement.gridCoord.y] = 1;
                tile.gridCoordX = placement.gridCoord.x;
                tile.gridCoordY = placement.gridCoord.y;
                occupiedCells++;
                return true;
            }
        }
        return false;
    }

    private bool CanPlaceShapeAt(int gridX, int gridY, List<Vector2Int> blocks)
    {
        foreach (Vector2Int blockOffset in blocks)
        {
            int checkX = gridX + blockOffset.x;
            int checkY = gridY + blockOffset.y;

            if (checkX < 0 || checkX >= gridWidth || checkY < 0 || checkY >= gridHeight)
            {
                return false;
            }

            if (gridCells[checkX, checkY] == 1)
            {
                return false;
            }
        }
        return true;
    }

    // VIẾT ĐẦY ĐỦ HÀM PLACETILE ĐỂ KHÓA LƯỚI
    public void PlaceTile(FoodTile tile, Vector2 coord)
    {
        AudioManager.Instance.TriggerVibrate(30);
        // 1. ĐỔI CHA: Đưa thức ăn làm con của đối tượng D (currentBentoUI)
        tile.transform.SetParent(currentBentoUI.transform);
        tile.tray.transform.localPosition = Vector3.zero;
        tile.icon.transform.localPosition = Vector3.zero;
        // 2. RESET LOCAL POSITION: 
        // Vì ta đã bay đến đúng vị trí World của ô đó, 
        // và vị trí World đó tương ứng với (localX, localY) trong cha mới D
        float targetLocalX = coord.x * cellSize;
        float targetLocalY = coord.y * cellSize;

        tile.transform.localPosition = new Vector3(targetLocalX, targetLocalY, 0);
        tile.transform.localRotation = Quaternion.identity; // Đảm bảo không bị nghiêng

        // 3. Khóa tương tác
        if (tile.TryGetComponent<BoxCollider2D>(out var col)) col.enabled = false;

        packedTiles.Add(tile);


        // // 1. ĐỔI CHA: Đưa món ăn vào làm con của Khay Bento
        // tile.transform.SetParent(currentBentoUI.transform);

        // // 2. TÍNH TOÁN LOCAL POSITION (Dựa trên quy tắc cellSize = 1 và tâm 0,0)
        // // Cột 0 -> -1 | Cột 1 -> 0 | Cột 2 -> 1
        // float startX = 0f;
        // float startY = 0f; // Hàng 0 là Top (Y dương)

        // float targetLocalX = startX + (coord.x * cellSize);
        // float targetLocalY = startY + (coord.y * cellSize);

        // // 3. THIẾT LẬP VỊ TRÍ CON: 
        // // Vì đã là con, nên (0,0,0) chính là tâm của Hộp Bento
        // tile.transform.localPosition = new Vector3(targetLocalX, targetLocalY, 0);
        // // 4. KHÓA DỮ LIỆU: Đánh dấu ô này đã có đồ
        // //gridCells[coord.x, coord.y] = 1;

        // // 5. ĐỒNG BỘ HIỂN THỊ: Đảm bảo món ăn nằm trên hình nền Bento
        // // if (tile.TryGetComponent<SpriteRenderer>(out var tileSR))
        // // {
        // //     var bentoSR = GetComponent<SpriteRenderer>();
        // //     tileSR.sortingOrder = bentoSR.sortingOrder + 1;
        // // }

        // // 6. TẮT TƯƠNG TÁC: Không cho người chơi bấm lại vào món đã xếp xong
        // if (tile.TryGetComponent<BoxCollider2D>(out var col)) col.enabled = false;

        // packedTiles.Add(tile);
    }

    public void ClearGrid()
    {
        // Reset ma trận về 0
        if (gridCells != null)
        {
            System.Array.Clear(gridCells, 0, gridCells.Length);

        }
        if (packedTiles != null)
        {
            packedTiles.Clear();
        }
        // Xóa các Object cũ trong khay Bento
        for (int i = BentoPos.childCount - 1; i >= 0; i--)
        {
            Transform childTray = BentoPos.GetChild(i);

            // So sánh Tag dùng CompareTag sẽ nhanh hơn và tránh lỗi chính tả
            if (childTray.CompareTag(GetTagTray()))
            {

                for (int j = childTray.transform.childCount - 1; j >= 0; j--)
                {
                    Transform childFood = childTray.transform.GetChild(j);

                    // So sánh Tag dùng CompareTag sẽ nhanh hơn và tránh lỗi chính tả
                    if (childFood.CompareTag("Food"))
                    {
                        // Trả về pool (thường sẽ làm mất child này khỏi parent hiện tại)
                        ObjectPooler.Instance.ReturnToPool("Food", childFood.gameObject);
                        FoodTile foodTile = childFood.gameObject.GetComponent<FoodTile>();
                        foodTile.SetDefault();
                    }
                }
                // Trả về pool (thường sẽ làm mất child này khỏi parent hiện tại)
                ObjectPooler.Instance.ReturnToPool(GetTagTray(), childTray.gameObject);
            }
        }

    }

    public bool CheckFullBento()
    {

        return occupiedCells >= totalCells;
    }
    // --- THÊM VÀO LỚP BENTOGRIDCONTROLLER ---

    public void RemoveLastTileFromBento(FoodTile tileToUndo)
    {
        if (tileToUndo == null || packedTiles == null || packedTiles.Count == 0) return;

        // 1. Kiểm tra xem đĩa này có thực sự nằm trong danh sách đã đóng gói không
        if (packedTiles.Contains(tileToUndo))
        {
            // 2. TÌM TỌA ĐỘ TRONG LƯỚI ĐỂ GIẢI PHÓNG (LOGIC REVERSE)
            // Vì mỗi món ăn được đặt dựa trên cellSize, ta tính ngược lại tọa độ Grid
            // Công thức từ hàm PlaceTile: targetLocalX = x * cellSize => x = localX / cellSize


            int gridX = tileToUndo.gridCoordX;
            int gridY = tileToUndo.gridCoordY;

            // 3. CẬP NHẬT LOGIC LƯỚI
            if (gridCells[gridX, gridY] == 1)
            {
                gridCells[gridX, gridY] = 0; // Giải phóng ô trong ma trận
                occupiedCells--;             // Giảm số lượng ô đã chiếm
//                Debug.Log("cap nhat");
            }



            // 4. CẬP NHẬT DANH SÁCH PACKED TILES
            packedTiles.Remove(tileToUndo);

            // 5. TRẢ LẠI TƯƠNG TÁC (QUAN TRỌNG)
            // Khi Undo, món ăn phải bấm được lại để người chơi nhặt tiếp
            if (tileToUndo.TryGetComponent<BoxCollider2D>(out var col))
            {
                col.enabled = true;
            }

            // 6. THIẾT LẬP LẠI CHA (PARENT)
            // Đưa món ăn thoát khỏi khay Bento (BentoPos) để nó tự do bay về bàn chơi
            // Thường sẽ trả về transform gốc của game hoặc null
            tileToUndo.transform.SetParent(null);

            //  Debug.Log($"<color=cyan>Undo thành công món: {tileToUndo.foodName} tại ô [{gridX},{gridY}]</color>");
        }
    }


}
