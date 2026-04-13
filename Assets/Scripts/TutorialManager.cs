

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening; // Yêu cầu cài đặt DOTween

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("UI References")]
    public GameObject tutorialCanvas;
    public Image maskPanel;           // Gán Material dùng Shader AntiAliasHole
    public RectTransform pointingFinger;
    public RectTransform tutorialTextRect;
    public TextMeshProUGUI tutorialText;
    public GameObject Pnl_bom;
    public Camera mainCamera;

    [Header("Visual Settings")]
    public float baseRadius = 130f;   // Bán kính mặc định cho đĩa
    public float boosterRadius = 180f; // Bán kính mặc định cho Booster
    public float pulseAmount = 25f;
    public float animDuration = 0.5f;
    public UnmaskRaycastFilter raycastFilter;

    // Quản lý trạng thái
    private List<Transform> stepTargets = new List<Transform>();
    private int currentStepIndex = 0;
    private bool isTutorialActive = false;
    private bool isBoosterMode = false;
    private int typeBooster = 0;//1 : undo 2: swap 3: hamer
    // Shader IDs (Tối ưu Mobile)
    private int holePosID;
    private int holeRadiusID;
    private Sequence masterSequence;

    public bool IsActive => isTutorialActive;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        holePosID = Shader.PropertyToID("_HolePosition");
        holeRadiusID = Shader.PropertyToID("_HoleRadius");

        tutorialCanvas.SetActive(false);
    }

    // ==========================================
    // 1. HƯỚNG DẪN LEVEL 1 (WORLD SPACE TILES)
    // ==========================================
    public void StartLevel1Tutorial()
    {
        List<Transform> targets = SetStepLevel1();
        if (targets == null || targets.Count == 0) return;
        stepTargets = targets;
        currentStepIndex = 0;
        isTutorialActive = true;
        isBoosterMode = false;
        typeBooster = 0;
        tutorialCanvas.SetActive(true);

        ShowCurrentStep();
    }

    List<Transform> SetStepLevel1()
    {
        List<Transform> targets = new List<Transform>();
        for (int i = 0; i < 3; i++)
        {
            var column = GameManager.Instance.boardCtrl.allTilesOnBoard[i];
            if (column.Count > 0)
            {
                targets.Add(column[0].gameObject.transform);
            }
        }
        return targets;
    }




    // ==========================================
    // 2. HƯỚNG DẪN BOOSTER (UI SCREEN SPACE)
    // ==========================================

    public void StartUndoTutorial()
    {

        List<Transform> targets = new List<Transform>();
        var column = GameManager.Instance.boardCtrl.allTilesOnBoard[2];
        if (column.Count > 0)
        {
            targets.Add(column[0].gameObject.transform);
        }
        if (targets == null || targets.Count == 0) return;
        stepTargets = targets;
        currentStepIndex = 0;
        isTutorialActive = true;
        isBoosterMode = false;
        tutorialCanvas.SetActive(true);
        typeBooster = 1;
        ShowCurrentStep();
    }
    public void StartHamerTutorial()
    {

        List<Transform> targets = new List<Transform>();
        var column = GameManager.Instance.boardCtrl.allTilesOnBoard[0];
        if (column.Count > 0)
        {
            targets.Add(column[0].gameObject.transform);
        }
        if (targets == null || targets.Count == 0) return;
        stepTargets = targets;
        currentStepIndex = 0;
        isTutorialActive = true;
        isBoosterMode = false;
        tutorialCanvas.SetActive(true);
        typeBooster = 3;
        ShowCurrentStep();
    }

    public void StartBoosterFocus(RectTransform boosterRect, string message, int typeBooster)
    {

        if (boosterRect == null) return;

        isTutorialActive = true;
        isBoosterMode = true;
        this.typeBooster = typeBooster;
        tutorialCanvas.SetActive(true);
        GameManager.Instance.boardCtrl.LockAllTiles(true);
        // Ép Layout Group cập nhật ngay lập tức
        Canvas.ForceUpdateCanvases();

        // Lấy 4 góc trong không gian Thế giới
        Vector3[] corners = new Vector3[4];
        boosterRect.GetWorldCorners(corners);

        // Tâm của nút Booster
        Vector3 worldCenter = (corners[0] + corners[2]) / 2f;

        // Chuyển sang Screen Space (Dùng cho Shader và UI)
        // Nếu Canvas của bạn là Overlay, mainCamera truyền vào là null
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, worldCenter);

        PlayStepAnimation(screenPos, message, 150.0f);
    }

    // ==========================================
    // LOGIC ĐIỀU KHIỂN CHÍNH
    // ==========================================
    private void ShowCurrentStep()
    {
        if (currentStepIndex >= stepTargets.Count)
        {
            EndTutorial();
            if (typeBooster == 1)
            {
                GameManager.Instance.boosterManager.UndoTutorial();
            }

            return;
        }

        Transform target = stepTargets[currentStepIndex];
        string msg = GetStepMessage(currentStepIndex);
        GameManager.Instance.boardCtrl.LockAllTiles(true);
        // Mở khóa đĩa mục tiêu
        FoodTile tile = target.GetComponent<FoodTile>();
        if (tile != null) tile.isLocked = false;

        // Chuyển tọa độ World sang Screen
        Vector2 screenPos = mainCamera.WorldToScreenPoint(target.position);
        PlayStepAnimation(screenPos, msg, baseRadius);
    }
    [Header("Current Hole State")]
    public Vector2 holeScreenPos; // Biến lưu tọa độ tâm lỗ (Pixel)
    public float holeRadius;     // Biến lưu bán kính lỗ
    private void PlayStepAnimation(Vector2 screenPos, string message, float radius)
    {
        // QUAN TRỌNG: Lưu giá trị vào biến toàn cục để dùng cho logic Check Click
        this.holeScreenPos = screenPos;
        this.holeRadius = radius;

        // --- Logic cũ của bạn ---
        masterSequence.Kill();
        masterSequence = DOTween.Sequence();

        // Gửi tọa độ vào Shader
        maskPanel.material.SetVector(holePosID, new Vector4(screenPos.x, screenPos.y, 0, 0));
        // 1. Dọn dẹp Tween cũ
        masterSequence.Kill();

        // 2. Thiết lập trạng thái ban đầu
        tutorialText.text = message;
        float yOffset = (screenPos.y < Screen.height * 0.35f) ? 250f : -250f;
        tutorialTextRect.position = screenPos + new Vector2(0, yOffset);
        maskPanel.material.SetVector(holePosID, new Vector4(screenPos.x, screenPos.y, 0, 0));

        pointingFinger.gameObject.SetActive(true);
        pointingFinger.position = screenPos + new Vector2(80f, -80f);
        pointingFinger.localScale = Vector3.one;

        // 3. Tạo Sequence mới và CHỈ Loop ở đây
        masterSequence = DOTween.Sequence();

        // Nhấn xuống
        masterSequence.Append(pointingFinger.DOScale(0.8f, animDuration).SetEase(Ease.InOutQuad));
        masterSequence.Join(maskPanel.material.DOFloat(radius - pulseAmount, holeRadiusID, animDuration));

        // Nhả ra
        masterSequence.Append(pointingFinger.DOScale(1.0f, animDuration).SetEase(Ease.OutBack));
        masterSequence.Join(maskPanel.material.DOFloat(radius, holeRadiusID, animDuration));

        // ĐẶT LOOP Ở ĐÂY (Cho toàn bộ Sequence)
        masterSequence.SetLoops(-1, LoopType.Restart);

        // Cập nhật tọa độ cho Filter để nó biết chỗ nào được phép xuyên qua
        if (raycastFilter != null)
        {
            raycastFilter.holeScreenPos = screenPos;
            raycastFilter.holeRadius = radius;
        }
    }

    public void OnTilePicked(FoodTile pickedTile)
    {
        if (!isTutorialActive || isBoosterMode) return;

        if (pickedTile.transform == stepTargets[currentStepIndex])
        {
            currentStepIndex++;
            masterSequence.Kill();
            pointingFinger.gameObject.SetActive(false);

            // Chờ đĩa bay vào Bento (0.6s) rồi hiện bước tiếp theo
            DOVirtual.DelayedCall(0.6f, ShowCurrentStep);
        }
    }


    public void EnableHameHint(bool display)
    {
        tutorialCanvas.SetActive(display);
        Pnl_bom.SetActive(display);
        maskPanel.gameObject.SetActive(!display);
        pointingFinger.gameObject.SetActive(!display);
        tutorialTextRect.gameObject.SetActive(!display);
        tutorialText.gameObject.SetActive(!display);

        GameManager.Instance.boardCtrl.LockAllTiles(false);
    }
    public void EndTutorial()
    {
        isTutorialActive = false;
        isBoosterMode = false;
        masterSequence.Kill();
        tutorialCanvas.SetActive(false);

        // Mở khóa toàn bộ Board để chơi bình thường
        GameManager.Instance.boardCtrl.LockAllTiles(false);
    }
    public void EndBoosterTutorial()
    {
        if (typeBooster == 0)
            return;
        isTutorialActive = false;
        isBoosterMode = false;
        typeBooster = 0;
        masterSequence.Kill();
        pointingFinger.gameObject.SetActive(false);
        tutorialCanvas.SetActive(false);
        // Mở khóa toàn bộ Board để chơi bình thường
        GameManager.Instance.boardCtrl.LockAllTiles(false);
    }
    public bool IsPositionInsideHole(Vector2 mouseScreenPos)
    {
        // Nếu tutorial không bật, cho phép click bình thường
        if (!isTutorialActive) return true;

        // Tính khoảng cách từ chuột đến tâm lỗ thủng
        // holeScreenPos là tọa độ chúng ta đã tính ở các bước trước
        float dist = Vector2.Distance(mouseScreenPos, holeScreenPos);

        // Nếu khoảng cách < bán kính => Hợp lệ
        return dist < (baseRadius + 10f);
    }
    private string GetStepMessage(int index)
    {
        string[] hints = {
            "Tap the plate!",
            "Well done! Tap the one.",
            "Finally, tap this to <color=green>Finish</color>!"
        };
        return index < hints.Length ? hints[index] : "Tap the next plate!";
    }
}