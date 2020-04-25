using System;
using GoogleARCore;
using GoogleARCore.Examples.ObjectManipulation;
using GoogleARCore.Examples.ObjectManipulationInternal;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class ModelManipulator : Manipulator
    {
        public Camera fpsCam;
        
        public GameObject horizontalPlanePrefab; // When user touch hits a horizontal plane.
        public GameObject verticalPlanePrefab;   // When user touch hits a vertical plane.
        public GameObject pointPrefab;           // When user touch hits a feature point.
        
        public GameObject manipulatorPrefab; // Added for manipulation options
        
        private const float prefabRotation = 180f;
        private Touch touch;
        
        private Button lockInstantiationButton;
        private bool lockInstantiation = false;
        private Button chooseModelButton;
        private bool choosingModel = true;

        public void Start()
        {
            lockInstantiationButton = GameObject.Find("LockInstantiateButton").GetComponent<Button>();
            lockInstantiationButton.onClick.AddListener(delegate { LockInstantiation(); });
            
            chooseModelButton = GameObject.Find("ChooseModelButton").GetComponent<Button>();
            chooseModelButton.onClick.AddListener(delegate { LockChoosingModel(); });
        }

        protected override bool CanStartManipulationForGesture(TapGesture gesture)
        {
            if (gesture.TargetObject == null)
            {
                return true;
            }
            
            return false;
        }

        protected override void OnEndManipulation(TapGesture gesture)
        {
            if (gesture.WasCancelled)
            {
                return;
            }
            
            // If gesture is targeting an existing object we are done.
            if (gesture.TargetObject != null)
            {
                return;
            }
            
            PlacePrefabToPlane(gesture);
        }

        private void PlacePrefabToPlane(TapGesture gesture)
        {
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon;
        
            // Raycast from tapGesture's start point.
            bool raycastResult = Frame.Raycast(gesture.StartPosition.x, gesture.StartPosition.y, raycastFilter, out hit);
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
                    GameObject prefab = horizontalPlanePrefab;
                    // if (hit.Trackable is FeaturePoint)
                    // {
                    //     prefab = pointPrefab;
                    // }
                    // else if (hit.Trackable is DetectedPlane)
                    // {
                    //     DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;
                    //     if (detectedPlane.PlaneType == DetectedPlaneType.Vertical)
                    //     {
                    //         prefab = verticalPlanePrefab;
                    //     }
                    //     else
                    //     {
                    //         prefab = horizontalPlanePrefab;
                    //     }
                    // }
                    // else
                    // {
                    //     RaycastHit hitForSelection;
                    //     if (GestureTouchesUtility.RaycastFromCamera(gesture.StartPosition, out hitForSelection))
                    //     {
                    //         var targetObject = hitForSelection.transform.gameObject;
                    //         if (targetObject != null)
                    //         {
                    //             Debug.Log("Target: " + targetObject.transform.position);
                    //         }
                    //     }
                    //
                    //     prefab = horizontalPlanePrefab;
                    // }
                    // else
                    // {
                    //     prefab = horizontalPlanePrefab;
                    // }
        
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
        
        private void LockChoosingModel()
        {
            var manipulators = FindObjectsOfType<Manipulator>();
            choosingModel = !choosingModel;
            if (choosingModel)
            {
                chooseModelButton.GetComponent<Image>().color = new Color(0f, 0.61f, 0f);
                foreach (var manipulator in manipulators)
                {
                    manipulator.enabled = true;
                }
            }
            else
            {
                chooseModelButton.GetComponent<Image>().color = new Color(0.61f, 0f, 0f);
                foreach (var manipulator in manipulators)
                {
                    manipulator.enabled = false;
                }
            }
        }
    }
}
