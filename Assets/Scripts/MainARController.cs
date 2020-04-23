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
    public Camera fpsCam;
    public GameObject horizontalPlanePrefab; // When user touch hits a horizontal plane.
    public GameObject verticalPlanePrefab;   // When user touch hits a vertical plane.
    public GameObject pointPrefab;           // When user touch hits a feature point.

    private const float prefabRotation = 180f;
    private bool isQuiting = false;
    private Touch touch;
    private bool lockInstantiation = false;
    private Button lockInstantiationButton;
    
    public void Awake()
    {
        Application.targetFrameRate = 60;
    }

	public void Start() {
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
        lockInstantiationButton = GameObject.Find("LockInstantiateButton").GetComponent<Button>();
        lockInstantiationButton.onClick.AddListener(delegate { LockInstantiation(); });
    }

    private void LockInstantiation()
    {
        lockInstantiation = !lockInstantiation;
        if (lockInstantiation)
        {
            lockInstantiationButton.GetComponent<Image>().color = new Color(0.61f, 0f, 0f);
        }
        else
        {
            lockInstantiationButton.GetComponent<Image>().color = new Color(0f, 0.61f, 0f);            
        }
    }

    public void Update()
    {
        UpdateAppLifeCycle();

        if (TouchAndErrorCheck())
        {
            return;
        }
        
        PlacePrefabToPlane();
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
    
    private void PlacePrefabToPlane()
    {
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                                          TrackableHitFlags.FeaturePointWithSurfaceNormal;

        bool raycastResult = Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit);
        if (raycastResult)
        {
            // If hit happens back of the plane, then no need to create anchor.
            if ((hit.Trackable is DetectedPlane) &&
                Vector3.Dot(fpsCam.transform.position - hit.Pose.position, 
                    hit.Pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("Hit at back of the current DetectedPlane");
            }
            else
            {
                GameObject prefab;
                if (hit.Trackable is FeaturePoint)
                {
                    prefab = pointPrefab;
                }
                else if (hit.Trackable is DetectedPlane)
                {
                    DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;
                    if (detectedPlane.PlaneType == DetectedPlaneType.Vertical)
                    {
                        prefab = verticalPlanePrefab;
                    }
                    else
                    {
                        prefab = horizontalPlanePrefab;
                    }
                }
                else
                {
                    prefab = horizontalPlanePrefab;
                }

                if (!lockInstantiation)
                {
                    var gameObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);
                    gameObject.transform.Rotate(0, prefabRotation, 0, Space.Self);

                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                    gameObject.transform.parent = anchor.transform;
                }
            }
        }
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
