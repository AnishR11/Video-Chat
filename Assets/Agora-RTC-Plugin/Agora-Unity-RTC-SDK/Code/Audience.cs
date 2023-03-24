using Agora.Rtc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Audience : MonoBehaviour,IVideoChatClient
{
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

    private void OnLeaveButtonClicked()
    {
        Leave(); // leave channel
        UnloadEngine(); // delete engine
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
        //GameObject gameObject = GameObject.Find("GameController");
        //Object.Destroy(gameObject);
    }

    private void updateChannelPublishOptions(bool publishMediaPlayer)
    {
        ChannelMediaOptions channelOptions = new ChannelMediaOptions();
        channelOptions.publishScreenTrack.SetValue(publishMediaPlayer);
        //channelOptions.publishCustomAudioTrack.SetValue(true);
        channelOptions.publishSecondaryScreenTrack.SetValue(publishMediaPlayer);
        channelOptions.publishCameraTrack.SetValue(!publishMediaPlayer);
        RtcEngine.UpdateChannelMediaOptions(channelOptions);
    }

    private void SetupUI()
    {
        GameObject go = GameObject.Find("Audience");
        LocalView = go.AddComponent<VideoSurface>();
        go.transform.Rotate(0.0f, 0.0f, 180.0f);

        go = GameObject.Find("ExitButton");
        if (go != null)
        {
            Button button = go.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(OnLeaveButtonClicked);
            }
        }

        go = GameObject.Find("RemoteView");
        RemoteView = go.AddComponent<VideoSurface>();

        // The target size of the screen or window thumbnail (the width and height are in pixels).
        SIZE t = new SIZE(360, 240);
        // The target size of the icon corresponding to the application program (the width and height are in pixels)
        SIZE s = new SIZE(360, 240);
        // Get a list of shareable screens and windows
        var info = RtcEngine.GetScreenCaptureSources(t, s, true);
        // Get the first source id to share the whole screen.
        ulong dispId = info[0].sourceId;
        // To share a part of the screen, specify the screen width and size using the Rectangle class.
        RtcEngine.StartScreenCaptureByWindowId(System.Convert.ToUInt32(dispId), new Rectangle(),
                default(ScreenCaptureParameters));
        // Publish the screen track and unpublish the local video track.
        updateChannelPublishOptions(true);


        //go = GameObject.Find("RemoteView");
        //RemoteView = go.AddComponent<VideoSurface>();
        //go.transform.Rotate(0.0f, 0.0f, 90.0f);
        //go = GameObject.Find("Leave");
        //go.GetComponent<Button>().onClick.AddListener(Leave);
        //go = GameObject.Find("Join");
        //go.GetComponent<Button>().onClick.AddListener(Join);

        LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA_PRIMARY);
        // Start rendering local video.
        LocalView.SetEnable(true);
        int streamId = 0;
        RtcEngine.CreateDataStream(ref streamId,reliable: true, ordered: true);
        //Join();
    }

    public void Join()
    {
        // Enable the video module.
        RtcEngine.EnableVideo();

        // Set the user role as broadcaster.
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_AUDIENCE);
        // Set the local video view.
        //LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);
        //// Start rendering local video.
        //LocalView.SetEnable(true);
        // Join a channel.
        RtcEngine.JoinChannel(_token, _channelName);
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


    public void LoadEngine(string appId)
    {
        SetupVideoSDKEngine();
        InitEventHandler();
        //SetupUI();
    }

    public void UnloadEngine()
    {
        throw new System.NotImplementedException();
    }

    public void OnSceneLoaded()
    {
        SetupUI();
        //throw new System.NotImplementedException();
    }

    public void EnableVideo(bool enable)
    {
        throw new System.NotImplementedException();
    }

    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly Audience _videoSample;

        internal UserEventHandler(Audience videoSample)
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
            //_videoSample.remoteUid = uid;
        }
        // This callback is triggered when a remote user leaves the channel or drops offline.
        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            _videoSample.RemoteView.SetEnable(false);
        }

    }
}
