using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GoogleARCore;
using GoogleARCore.Examples.Common;

public class AppController : MonoBehaviour
{
    public Camera FirstPersonCamera;
    private bool mIsQuitting = false;
    private const float mModelRotation = 0.0f;
    ArrayList list = new ArrayList();

    // Use this for initialization
    void Start()
    {
        OnCheckDevice();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateApplicationLifecycle();
    }

    /// <summary>
    /// Check device
    /// </summary>
    private void OnCheckDevice()
    {
        if (Session.Status == SessionStatus.ErrorSessionConfigurationNotSupported)
        {
            ShowAndroidToastMessage("ARCore is not supported on current device or configured correctly！");
            mIsQuitting = true;
            Invoke("DoQuit", 0.5f);
        }
        else if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
        {
            ShowAndroidToastMessage("Please give the permission to use the camera！");
            mIsQuitting = true;
            Invoke("DoQuit", 0.5f);
        }
        else if (Session.Status.IsError())
        {
            ShowAndroidToastMessage("Runtime error, please restart the application！");
            mIsQuitting = true;
            Invoke("DoQuit", 0.5f);
        }
    }

    /// <summary>
    /// Control the lifecycle of application
    /// </summary>
    private void UpdateApplicationLifecycle()
    {
        if (Session.Status != SessionStatus.Tracking)
        {
            const int lostTrackingSleepTimeout = 15;
            Screen.sleepTimeout = lostTrackingSleepTimeout;
        }
        else
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        if (mIsQuitting)
        {
            return;
        }
    }

    /// <summary>
    /// Quit the application
    /// </summary>
    private void DoQuit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Pop-up message
    /// </summary>
    private void ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
