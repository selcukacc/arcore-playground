using GoogleARCore;
using GoogleARCore.Examples.ObjectManipulation;
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
    public GameObject manipulatorPrefab;     // Added for manipulation options

    private bool isQuiting = false;
    private const float prefabRotation = 180f;
    private Touch touch;
        
    private Button lockInstantiationButton;
    private bool lockInstantiation = false;
    private Button fixPositionsButton;
    private bool fixingPosition = false;
    
    public void Awake()
    {
        Application.targetFrameRate = 60;
    }

	public void Start() {
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
        lockInstantiationButton = GameObject.Find("LockInstantiateButton").GetComponent<Button>();
        lockInstantiationButton.onClick.AddListener(delegate { LockInstantiation(); });
            
        fixPositionsButton = GameObject.Find("FixPositionsButton").GetComponent<Button>();
        fixPositionsButton.onClick.AddListener(delegate { FixPositions(); });
    }

    public void Update()
    {
        UpdateAppLifeCycle();

        // If the player has not touched the screen, we are done with this update.
        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        // Should not handle input if the player is pointing on UI.
        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
        {
            return;
        }
        
        // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon;

            if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
            {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(fpsCam.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                else
                {
                    // Choose the prefab based on the Trackable that got hit.
                    GameObject prefab;
                    if (hit.Trackable is FeaturePoint)
                    {
                        prefab = horizontalPlanePrefab;
                    }
                    else if (hit.Trackable is DetectedPlane)
                    {
                        DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;
                        if (detectedPlane.PlaneType == DetectedPlaneType.Vertical)
                        {
                            prefab = horizontalPlanePrefab;
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
                        // Instantiate game object at hit pose.
                        var gameObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                        // Rotation for user experience
                        //gameObject.transform.Rotate(0, prefabRotation, 0, Space.Self);

                        var manipulator = Instantiate(manipulatorPrefab, hit.Pose.position, hit.Pose.rotation);
                        gameObject.transform.parent = manipulator.transform;

                        // Creating anchor for tracking on hitpoint.
                        var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                        manipulator.transform.parent = anchor.transform;

                        // Select the placed object.
                        manipulator.GetComponent<Manipulator>().Select();
                    }
                }
            }
        
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

    private void FixPositions()
    {
        var manipulators = FindObjectsOfType<Manipulator>();
        fixingPosition = !fixingPosition;
        if (fixingPosition)
        {
            fixPositionsButton.GetComponent<Image>().color = new Color(0f, 0.61f, 0f);
            foreach (var manipulator in manipulators)
            {
                manipulator.enabled = false;
                manipulator.GetComponent<SelectionManipulator>().enabled = false;
            }
        }
        else
        {
            fixPositionsButton.GetComponent<Image>().color = new Color(0.61f, 0f, 0f);
            foreach (var manipulator in manipulators)
            {
                manipulator.enabled = true;
                manipulator.GetComponent<SelectionManipulator>().enabled = true;
            }
        }
    }

    private void LockInstantiation()
    {
        lockInstantiation = !lockInstantiation;
        if (lockInstantiation)
        {
            lockInstantiationButton.GetComponent<Image>().color = new Color(0f, 0.61f, 0f);
                
        }
        else
        {
            lockInstantiationButton.GetComponent<Image>().color = new Color(0.61f, 0f, 0f);            
        }
    }
    
    private void DoQuit()
    {
        Application.Quit();
    }
}
