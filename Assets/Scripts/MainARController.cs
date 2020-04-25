using GoogleARCore;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class MainARController : MonoBehaviour
{
    private bool isQuiting = false;
    private Touch touch;
    
    public void Awake()
    {
        Application.targetFrameRate = 60;
    }

	public void Start() {
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

    }

    public void Update()
    {
        UpdateAppLifeCycle();

        // if (TouchAndErrorCheck())
        // {
        //     return;
        // }
    }

    private bool TouchAndErrorCheck()
    {
        // If the player has not touch the screen, we are done with this update.
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return true;
        }

        // Should not handle input if the player is pointing on UI.
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return true;
        }
        
        return false;
    }
    
  private void UpdateAppLifeCycle()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (isQuiting)
        {
            return;
        }

        if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            ShowAndroidToastMessage("Camera permission is needed to run this application.");
            isQuiting = true;
            Invoke("DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            ShowAndroidToastMessage("ARCore encountered a problem connecting. Please start the app again.");
            isQuiting = true;
            Invoke("DoQuit", 0.5f);
        }
    }

    private void ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity =
            unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject =
                    toastClass.CallStatic<AndroidJavaObject>(
                        "makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }

    private void DoQuit()
    {
        Application.Quit();
    }
}
