using Unity.XR.CoreUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR;
using UnityEngine;
using System.Xml.Linq;

/// <summary>
/// - Initial recenter (for the Quest issue of centering in the center of a room scale space instead of where the user is initially)
/// - Disable Recentering button after intial recentering
/// - rotate world for calibration
/// </summary>
[RequireComponent(typeof(XROrigin))]

public class InitRecentering : MonoBehaviour
{
    [Tooltip("Recenter hmd on start")]
    public bool recenterOnStart = true;
    [Tooltip("disable ability to recenter (the long press sys-menu hmd button)")]
    public bool disableRecenter = true;
    [Tooltip(" used for XROrigin features like centering")]
    public XROrigin xrOrigin;
    [Tooltip("World to rotate")]
    public Transform worldTransform;

    private Transform cameraTransform;
    private Transform cameraOffsetTransform;

    private static bool firstStart = true;
    private XRInputSubsystem xrInputSubsystem;
    public IEnumerator Start()
    {
        if (cameraTransform == null)
        {
          //  GameObject cameraObject = GameObject.Find("Main Camera");
          //  if(cameraObject!=null)
          //  {
          //      cameraTransform = cameraObject.transform;
          //  } else
          //  {
          //      Debug.LogError("InitRecentering.Start: couldn't find Main Camera");
          //  }
          //  GameObject cameraOffsetObject = GameObject.Find("Camera Offset");
          //  if (cameraOffsetObject != null)
          //  {
          //      cameraOffsetTransform = cameraOffsetObject.transform;
          //  }
          //  else
          //  {
          //      Debug.LogError("InitRecentering.Start: couldn't find Main Camera");
          //  }
        }

            if (xrOrigin==null)
        {
            xrOrigin = gameObject.GetComponent<XROrigin>();
        }

        if (firstStart && recenterOnStart)
        {
            firstStart = false; // do only on the first call to Start
            // set ability to recenter
            OpenXRSettings.SetAllowRecentering(true);

            // Get the XR Input Subsystem
            List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
            yield return new WaitForSecondsRealtime(0.25f);
            SubsystemManager.GetSubsystems(subsystems);
            if (subsystems.Count > 0)
            {
                xrInputSubsystem = subsystems[0];
            }
            if (xrInputSubsystem != null)
            {
                yield return new WaitForSecondsRealtime(0.25f);
                Recenter();
            }
        } else {
            OpenXRSettings.SetAllowRecentering(!disableRecenter);
        }
    }
    public void Recenter()
    {
        Transform target = xrOrigin.transform;
        xrOrigin.transform.position = new Vector3(target.position.x, 0, target.position.z); //ignore height
        xrOrigin.MoveCameraToWorldLocation(target.position);
        xrOrigin.MatchOriginUpCameraForward(target.up, target.forward);
        xrOrigin.transform.position = new Vector3(xrOrigin.transform.position.x, 0, xrOrigin.transform.position.z); // ignore height
    }
    private bool IsHmdOn()
    {
        InputDevice headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        bool userPresent = false;
        bool presenceFeatureSupported = false;
        if (headDevice.isValid == true)
        {
            presenceFeatureSupported
                = headDevice.TryGetFeatureValue(CommonUsages.userPresence, out userPresent);
        } else {
            Debug.Log("InitRecentering: IsHmdOn: headDevice is not valid");
        }
        if (!presenceFeatureSupported) {
            Debug.Log("InitRecentering: IsHmdOn: presenceFeatureSupported is false");
        }
        return userPresent;
    }

    [Tooltip("Show when headset is currently on or off head - currently for debugging")]
    public bool hmdOnState = true; //default assumption is headset is on head at start - todo: handle as a signal, make this private
    
    private bool recentered = false;

    private void Update()
    {
        if (recentered)
        {
            recentered = false;
            if(disableRecenter)
                OpenXRSettings.SetAllowRecentering(false);
        }

        // optional - debug /////////////
        bool present = IsHmdOn();
        if (!present && hmdOnState)
        {
            hmdOnState = false; //TODO: create signal to notify on change?
        }
        else if (present && !hmdOnState)
        {
            hmdOnState = true;
        }
        ////////////////////////////////
    }
    public void RotateWorld()
    {

        transform.rotation = Quaternion.Euler(0, cameraTransform.localRotation.y + cameraOffsetTransform.localRotation.y, 0);
    }
}
