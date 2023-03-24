using Agora.Rtc;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;

public class VuforiaIntegration : MonoBehaviour
{
    static IVideoChatClient app = null;
    // Fill in your app ID.
    private string _appID = "a2dc077ec06a4fd8bbd83c4e9f4dc8ee";
    // Fill in your channel name.
    private string _channelName = "ABC";
    // Fill in the temporary token you obtained from Agora Console.
    private string _token = "007eJxTYPj+S/CoPedaK66Ue5k++4qf8cVUT5ed9F/Cfqut/Mle4zcKDIlGKckG5uapyQZmiSZpKRZJSSkWxskmqZZpJinJFqmpH3NlUhoCGRkkPhqwMjJAIIjPzODo5MzAAAAtWx7L";
    // A variable to save the remote user uid.
    private uint remoteUid;
    internal VideoSurface LocalView;
    internal VideoSurface RemoteView;
    internal IRtcEngine RtcEngine;
    // Vuforia variables
    private Camera vuforiaCamera;

    private Texture2D texture;
    private byte[] bytes;

    private void SetupVideoSDKEngine()
    {
        // Create an instance of the video SDK.
        RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        // Specify the context configuration to initialize the created instance.
        RtcEngineContext context = new RtcEngineContext(_appID, 0,
        CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION,
        AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT);
        // Initialize the instance.
        RtcEngine.Initialize(context);
    }


    //    private void CheckPermissions()
    //    {
    //#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
    //    foreach (string permission in permissionList)
    //    {
    //        if (!Permission.HasUserAuthorizedPermission(permission))
    //        {
    //            Permission.RequestUserPermission(permission);
    //        }
    //    }
    //#endif
    //    }

   
    
    // Start is called before the first frame update
    void Start()
    {
        SetupVideoSDKEngine();
        InitEventHandler();
        SetupUI();
        //Join();
    }

    // Update is called once per frame
    void Update()
    {
        // Capture frame from Vuforia camera
        Camera cam = Camera.main;
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();
        bytes = texture.GetRawTextureData();

        // Send frame to Agora video call
        RtcEngine.PushVideoFrame(new ExternalVideoFrame()
        {
            buffer = bytes,
            stride = Screen.width,
            height = Screen.height,
            format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA,
            //timeSpan = Time.realtimeSinceStartup * 1000
        });
        //CheckPermissions();
    }

    void OnApplicationQuit()
    {
        if (RtcEngine != null)
        {
            Leave();
            RtcEngine.Dispose();
            RtcEngine = null;
        }
    }

    private void InitEventHandler()
    {
        // Creates a UserEventHandler instance.
        UserEventHandler handler = new UserEventHandler(this);
        RtcEngine.InitEventHandler(handler);
    }

    private void SetupUI()
    {
        GameObject go = GameObject.Find("LocalView");
        LocalView = go.AddComponent<VideoSurface>();
        go.transform.Rotate(0.0f, 0.0f, 180.0f);
        Vuforia.VuforiaApplication.Instance.OnVuforiaInitialized += OnVuforiaStarted;
        texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        bytes = new byte[texture.width * texture.height * 3];
        //go.transform.Rotate(0.0f, 0.0f, 90.0f);
        //go = GameObject.Find("RemoteView");
        //RemoteView = go.AddComponent<VideoSurface>();
        //go.transform.Rotate(0.0f, 0.0f, 90.0f);
        //go = GameObject.Find("Join");
        //go.GetComponent<Button>().onClick.AddListener(Join);
        //go = GameObject.Find("Create");
        //go.GetComponent<Button>().onClick.AddListener(Create);
        //RtcEngine.SetupLocalVideo()
    }

    private void OnVuforiaStarted(VuforiaInitError obj)
    {

        RtcEngine.EnableVideo();
        // Set the user role as broadcaster.
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        // Set the local video view.
        LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);
        // Start rendering local video.
        LocalView.SetEnable(true);
        // Join a channel.
        RtcEngine.JoinChannel(_token, _channelName);
    }

    public void Create()
    {

    }

    public void Join()
    {
        
    }

    public void Leave()
    {
        // Leaves the channel.
        RtcEngine.LeaveChannel();
        // Disable the video modules.
        RtcEngine.DisableVideo();
        // Stops rendering the remote video.
        RemoteView.SetEnable(false);
        // Stops rendering the local video.
        LocalView.SetEnable(false);
    }



    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly VuforiaIntegration _videoSample;

        internal UserEventHandler(VuforiaIntegration videoSample)
        {
            _videoSample = videoSample;
        }
        // This callback is triggered when the local user joins the channel.
        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("You joined channel: " + connection.channelId);
        }
        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            // Setup remote view.
            //_videoSample.RemoteView.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            //// Save the remote user ID in a variable.
            //_videoSample.remoteUid = uid;
        }
        // This callback is triggered when a remote user leaves the channel or drops offline.
        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _videoSample.RemoteView.SetEnable(false);
        }

    }

    public void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {

        if (!ReferenceEquals(app, null))
        {
            app.OnSceneLoaded(); // call this after scene is loaded
        }

        SceneManager.sceneLoaded -= OnLevelFinishedLoading;

    }


}
