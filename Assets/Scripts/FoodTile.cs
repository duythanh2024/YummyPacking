using System;
using UnityEngine;
//Lớp này gắn trực tiếp lên Prefab của mỗi miếng đồ ăn.
public class FoodTile : MonoBehaviour
{
    public FoodData data;
    public SpriteRenderer tray;
    public SpriteRenderer icon;
    public SpriteRenderer iconStatus;
    public bool isClickable = false; // Trạng thái có bị đè hay không   
    public int columnId;
    public int rowId;
    public bool isLocked = false;
    public int gridCoordX = 0;
    public int gridCoordY = 0;
    public int layerId;
    public TypeTrayFood typeTrayFood;
    public BuffSlot currentBuffSlot;
    public FoodSlot currentFoodSlot;
    [HideInInspector]
    public bool IsClick;

    public Sprite[] iconStatusTile; //0 ice, 1 lock, 1 nap
    public void SetDefault()
    {
        tray.gameObject.SetActive(true);
        tray.transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        GetComponent<BoxCollider2D>().enabled = true;
        icon.gameObject.SetActive(true);
        currentBuffSlot = null;
        currentFoodSlot = null;
        isLocked = false;
        IsClick = false;
        isClickable=true;
        gridCoordX = 0;
        gridCoordY = 0;
    }
    public void SetStatus()
    {
        iconStatus.gameObject.SetActive(false);
        isLocked=false;
        if (typeTrayFood == TypeTrayFood.None)
        {
            iconStatus.gameObject.SetActive(false);
            isLocked=false;
        }
        else if (typeTrayFood == TypeTrayFood.Ice)
        {
            iconStatus.gameObject.SetActive(true);
            iconStatus.sprite=iconStatusTile[0];
            isLocked=true;
        }
         else if (typeTrayFood == TypeTrayFood.Lock)
        {
            iconStatus.gameObject.SetActive(true);
            iconStatus.sprite=iconStatusTile[1];
            isLocked=true;
        }
         else if (typeTrayFood == TypeTrayFood.Hidden)
        {
            iconStatus.gameObject.SetActive(true);
            iconStatus.sprite=iconStatusTile[2];
            isLocked=true;
        }

    }
    //     private void OnMouseDown1()
    //     {

    // // Kiểm tra xem chuột có đang nằm trên UI không
    //         if (EventSystem.current.IsPointerOverGameObject())
    //         {
    //             // Nếu có, dừng xử lý logic của Tile
    //             return;
    //         }

    //         // Logic xử lý khi click vào Tile của bạn ở đây
    //         Debug.Log("Đã click vào Tile!");
    //         if (isLocked) return;
    //   Debug.Log("Đã click vào Tile! 1");
    //         // --- LOGIC BÚA (ƯU TIÊN HÀNG ĐẦU) ---
    //         if (BoosterManager.Instance.isHammerActive)
    //         {
    //             if(!data.isBuff)
    //             return;
    //             BoosterManager.Instance.ExecuteHammer(this);
    //             return; // Thoát hàm, không chạy logic nhặt đĩa bên dưới
    //         }
    //           Debug.Log("Đã click vào Tile!");
    //         // 1. Nếu Tutorial đang bật
    //         if (TutorialManager.Instance != null && TutorialManager.Instance.IsActive)
    //         {
    //             // Lấy vị trí chuột trên màn hình
    //             Vector2 mousePos = Input.mousePosition;

    //             // Tính khoảng cách từ chuột tới tâm cái lỗ
    //             float dist = Vector2.Distance(mousePos, TutorialManager.Instance.holeScreenPos);

    //             // Nếu khoảng cách LỚN HƠN bán kính (nghĩa là click vào vùng đen)
    //             if (dist > TutorialManager.Instance.holeRadius)
    //             {
    //                 Debug.Log("<color=red>Tutorial: Chặn click vào đĩa ngoài vùng sáng!</color>");
    //                 return; // Dừng hàm tại đây, không chạy code nhặt đĩa bên dưới
    //             }
    //         }
    //    Debug.Log("Đã click vào Tile! 3");

    //         //Debug.Log("Nhan "+isClickable);
    //         // Kiểm tra xem miếng Cá này có đang bị miếng khác đè không
    //         if (isClickable && !GameManager.Instance.isWin)
    //         {
    //              AudioManager.Instance.Play("Click");
    //             // Thông báo cho TutorialManager đã click đúng
    //             if (TutorialManager.Instance != null)
    //                 TutorialManager.Instance.OnTilePicked(this);
    //             // Gửi thông tin miếng Cá này sang bộ não trung tâm
    //             // Trước khi bay vào Bento, ghi lại thông tin để Undo




    //             // UndoStep currentStep = new UndoStep
    //             // {
    //             //     tile = this,
    //             //     originalPosition = transform.localPosition, // Dùng Local để khớp với logic Column
    //             //     originalScale = transform.localScale,
    //             //     columnId = this.columnId, // Đảm bảo FoodTile có biến columnId
    //             //                               // Lấy vị trí của nó trong List cột ngay trước khi nhặt
    //             //     originalIndex = GameManager.Instance.boardCtrl.allTilesOnBoard[this.columnId].IndexOf(this)
    //             // };
    //             // BoosterManager.Instance.RecordMove(currentStep);
    //             GameManager.Instance.OnTileTapped(this);


    //         }
    //         else if (GameManager.Instance.isWin)
    //         {
    //             Debug.Log("NO Nhan ");
    //              AudioManager.Instance.Play("Error");

    //             // Hiệu ứng rung lắc nhẹ báo hiệu miếng Cá đang bị kẹt
    //             BentoTweenHelper.ErrorShake(transform);
    //         }
    //     }


    // Hàm thực hiện Animation bay
    public void MoveToTarget(Vector3 targetPosition, Action onComplete)
    {
        // Dùng DOTween (Unity) hoặc cc.tween (Cocos) để di chuyển mượt mà
    }

    // Thay đổi trạng thái hiển thị (sáng/tối) dựa trên việc có bị đè không
    public void SetHighlight(bool clickable)
    {
        isClickable = clickable;
        // Logic đổi màu material/sprite ở đây
    }
}