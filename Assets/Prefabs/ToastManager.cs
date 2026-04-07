using TMPro;
using UnityEngine;

public class ToastManager : MonoBehaviour
{
    public static ToastManager Instance;
    private Animator toastAnim;
    public GameObject ToastObject;
    public TMP_Text text;

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;



        //   DontDestroyOnLoad(gameObject);
    }
    void Start()
    {
        toastAnim = ToastObject.GetComponent<Animator>();
    }

    public void ShowToast(string message)
    {
        AudioManager.Instance.Play("Toast");
        text.text = message;
        if (toastAnim == null)
        {
            toastAnim = ToastObject.GetComponent<Animator>();
        }
        toastAnim.SetTrigger("Toast");
    }
}
