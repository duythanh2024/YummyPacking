using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(IInputHandler))]
public class InputController : MonoBehaviour
{
    private IInputHandler handler;
    private bool touchDown;
    private void Awake()
    {
        handler = GetComponent<IInputHandler>();
        Input.simulateMouseWithTouches = false;
    }

    //     void Start()
    //     {
    //        // InitializeInput();
    //     }
    //     public void InitializeInput()
    //     {
    // #if UNITY_EDITOR
    //         DebugLog.WriteLog("Bạn đang chạy trong Unity Editor");
    //         isMobile = false;
    // #elif UNITY_WEBGL
    //                     DebugLog.WriteLog("Code này chỉ tồn tại trong bản build WebGL");
    //                     isMobile=IsMobileBrowser();
    // #elif UNITY_ANDROID || UNITY_IOS
    //                     DebugLog.WriteLog("Code này chỉ dành cho Mobile Native");
    //                     isMobile=true;
    // #endif
    //     }
    private bool IsMobileBrowser()
    {
        // Kiểm tra thông qua cấu hình thiết bị cơ bản
        return Application.isMobilePlatform ||
               SystemInfo.deviceType == DeviceType.Handheld;
    }
    // Hàm tiện ích để kiểm tra UI cho Touch an toàn hơn
    private bool IsPointerOverUI(Touch touch)
    {
        // Cách kiểm tra UI chuẩn cho Touch ID
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return true;

        // Phòng hờ một số trường hợp WebGL trên Android cũ
        if (EventSystem.current.IsPointerOverGameObject()) return true;

        return false;
    }
    void Update()
    {
        if (handler == null) return;

        // 1. Xử lý cho Touch (Mobile)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // KIỂM TRA UI CHO TOUCH
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;

            if (touch.phase == TouchPhase.Began)
            {
                handler.HandleTouchDown(touch.position);
                touchDown = true;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                handler.HandleTouchUp(touch.position);
                touchDown = false;
            }
            else if (touch.phase == TouchPhase.Moved && touchDown)
            {
                handler.HandleTouchMove(touch.position);
            }
            // return; // Đã là touch thì không xuống check mouse nữa
        }
        else
        {

            // 2. Xử lý cho Mouse (Editor/PC)
            if (Input.GetMouseButtonDown(0))
            {

                if (EventSystem.current.IsPointerOverGameObject()) return;
                handler.HandleTouchDown(Input.mousePosition);
                touchDown = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;
                // Thường Up cũng nên check UI hoặc check biến touchDown
                handler.HandleTouchUp(Input.mousePosition);
                touchDown = false;
            }
            else if (touchDown)
            {
                //   if (EventSystem.current.IsPointerOverGameObject()) return;
                handler.HandleTouchMove(Input.mousePosition);
            }
        }


    }
}