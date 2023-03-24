using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using UnityEngine.SceneManagement;

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif


//#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
//private ArrayList permissionList = new ArrayList() { Permission.Camera, Permission.Microphone };
//#endif




public class NewBehaviourScript : MonoBehaviour
{
    static IVideoChatClient app = null;
    // Fill in your app ID.
    private string _appID = "a2dc077ec06a4fd8bbd83c4e9f4dc8ee";
    // Fill in your channel name.
    private string _channelName = "ABC";
    // Fill in the temporary token you obtained from Agora Console.
    private string _token = "007eJxTYFhYVC2cL6cZM1l/7qI/enYd4VUd7ifC6hJVrhQoZhwxj1RgSDRKSTYwN09NNjBLNElLsUhKSrEwTjZJtUwzSUm2SE2dqiCV0hDIyHCtZjYDIxSC+MwMjk7ODAwAo3EdQA==";
    // A variable to save the remote user uid.
    private uint remoteUid;
    internal VideoSurface LocalView;
    internal VideoSurface RemoteView;
    internal IRtcEngine RtcEngine;

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
    }

    // Update is called once per frame
    void Update()
    {
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
        //GameObject go = GameObject.Find("LocalView");
        //LocalView = go.AddComponent<VideoSurface>();
        //go.transform.Rotate(0.0f, 0.0f, 90.0f);
        //go = GameObject.Find("RemoteView");
        //RemoteView = go.AddComponent<VideoSurface>();
        //go.transform.Rotate(0.0f, 0.0f, 90.0f);
        GameObject go = GameObject.Find("Join");
        go.GetComponent<Button>().onClick.AddListener(Join);
        go = GameObject.Find("Create");
        go.GetComponent<Button>().onClick.AddListener(Create);
    }

    public void Create()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        SceneManager.LoadScene("Broadcast");
        
    }

    public void Join()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
        SceneManager.LoadScene("Audience");
        app = new Audience();
        app.LoadEngine(_appID);
        app.Join();
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
        private readonly NewBehaviourScript _videoSample;

        internal UserEventHandler(NewBehaviourScript videoSample)
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
            _videoSample.RemoteView.SetForUser(uid, connection.channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
            // Save the remote user ID in a variable.
            _videoSample.remoteUid = uid;
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
