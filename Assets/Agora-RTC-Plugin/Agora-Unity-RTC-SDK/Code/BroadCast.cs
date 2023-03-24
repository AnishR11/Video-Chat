using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Agora.Rtc;
using UnityEngine.SceneManagement;
using Unity.Collections;
using UnityEngine.XR.ARFoundation;
using Vuforia;

#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif



public class BroadCast : MonoBehaviour, IVideoChatClient
{

    // Fill in your app ID.
    private string _appID = "a2dc077ec06a4fd8bbd83c4e9f4dc8ee";
    // Fill in your channel name.
    private string _channelName = "ABC";
    // Fill in the temporary token you obtained from Agora Console.
    private string _token = "007eJxTYJgUcE6Rt/fWUcma4rv156YL8HLOz/qfeyjqtH5A5BILtisKDIlGKckG5uapyQZmiSZpKRZJSSkWxskmqZZpJinJFqmpkfdlUxoCGRkcE6cyMTJAIIjPzODo5MzAAAAZYx5v";
    // A variable to save the remote user uid.
    private uint remoteUid;
    internal VideoSurface LocalView;
    internal VideoSurface RemoteView;
    internal IRtcEngine RtcEngine;
    //ARCameraManager cameraManager;
    public static int ShareCameraMode = 1;
    int i = 0; // monotonic timestamp counter
    Texture2D mVideoTexture;
    public static VIDEO_PIXEL_FORMAT PixelFormat = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_BGRA;

    MonoBehaviour monoProxy;
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

        //changes
        mVideoTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        RtcEngine.EnableVideo();

        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        // Set the local video view.
        LocalView.SetForUser(0, "", VIDEO_SOURCE_TYPE.VIDEO_SOURCE_CAMERA);

        LocalView.SetEnable(true);
        // Join a channel.
        RtcEngine.JoinChannel(_token, _channelName);



        VideoDimensions videoDimensions = new VideoDimensions()
        {

        };
        RtcEngine.SetVideoEncoderConfiguration(new VideoEncoderConfiguration(ref videoDimensions,
            15,
            (int)BITRATE.STANDARD_BITRATE,
            ORIENTATION_MODE.ORIENTATION_MODE_FIXED_PORTRAIT

        ));

        RtcEngine.EnableLocalVideo(true);
        RtcEngine.SetExternalVideoSource(true, true, EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME, new SenderOptions());
    }

    // Update is called once per frame
    void Update()
    {
        Vuforia.Image image = VuforiaBehaviour.Instance.CameraDevice.GetCameraImage(Vuforia.PixelFormat.RGB888);
        if (image != null)
        {
            mVideoTexture.LoadRawTextureData(image.Pixels);
            mVideoTexture.Apply();
        }
        // Update the Agora video stream with the new texture
        RtcEngine.PushVideoFrame(new ExternalVideoFrame()
        {
            type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA,
            format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_I420,
            buffer = mVideoTexture.GetRawTextureData(),
            stride = Screen.width,
            height = Screen.height,
            cropLeft = 0,
            cropTop = 0,
            cropRight = 0,
            cropBottom = 0,
            rotation = 0,
        });
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
        GameObject go = GameObject.Find("Broadcast");
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

        go = GameObject.Find("ARCamera");
        if (go != null)
        {
            monoProxy = go.GetComponent<MonoBehaviour>();
        }

        //go = GameObject.Find("sphere");
        //if (go != null)
        //{
        //    var sphere = go;
        //    // hide this before AR Camera start capturing
        //    sphere.SetActive(false);
        //    monoProxy.StartCoroutine(DelayAction(.5f,
        //        () =>
        //        {
        //            sphere.SetActive(true);
        //        }));
        //}


        go = GameObject.Find("RemoteView");
        RemoteView = go.AddComponent<VideoSurface>();
        //Join();
    }

    IEnumerator DelayAction(float delay, System.Action doAction)
    {
        yield return new WaitForSeconds(delay);
        doAction();
    }

    public void Join()
    {
        RtcEngine.SetCameraCapturerConfiguration(new CameraCapturerConfiguration() { cameraDirection = CAMERA_DIRECTION.CAMERA_REAR });
        // Enable the video module.
        RtcEngine.EnableVideo();


        //AR CAMERA
        //CameraCapturerConfiguration config = new CameraCapturerConfiguration();
        ////config.preference = CAPTURER_OUTPUT_PREFERENCE.CAPTURER_OUTPUT_PREFERENCE_AUTO;
        //config.cameraDirection = CAMERA_DIRECTION.CAMERA_REAR;
        //RtcEngine.SetCameraCapturerConfiguration(config);
        //int s = RtcEngine.SetVideoEncoderConfiguration(new VideoEncoderConfiguration
        //{
        //    dimensions = new VideoDimensions { width = 360, height = 640 },
        //    frameRate = (int)FRAME_RATE.FRAME_RATE_FPS_24,
        //    bitrate = 800,
        //    orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_FIXED_PORTRAIT
        //});
        //int a = RtcEngine.SetExternalVideoSource(true, false,EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME,new SenderOptions() { });
        //RtcEngine.EnableLocalAudio(false);
        //RtcEngine.MuteLocalAudioStream(true);
        // Set the user role as broadcaster.
        //RtcEngine.SetExternalVideoSource(true,true,EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME,new SenderOptions());
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        RtcEngine.JoinChannel(_token, _channelName);
    }

    
    private void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        // There are two ways doing the capture. 
        if (ShareCameraMode == 0)
        {
            // See function header for what this function is
            // CaptureARBuffer();
        }
        else
        {
            //ShareRenderTexture();
        }
    }

   

    IEnumerator PushFrame(byte[] bytes, int width, int height, System.Action onFinish)
    {
        if (bytes == null || bytes.Length == 0)
        {
            Debug.LogError("Zero bytes found!!!!");
            yield break;
        }

        //if the engine is present
        if (RtcEngine != null)
        {
            //Create a new external video frame
            ExternalVideoFrame externalVideoFrame = new ExternalVideoFrame();
            //Set the buffer type of the video frame
            externalVideoFrame.type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA;
            // Set the video pixel format
            externalVideoFrame.format = PixelFormat; // VIDEO_PIXEL_BGRA for now
            //apply raw data you are pulling from the rectangle you created earlier to the video frame
            externalVideoFrame.buffer = bytes;
            //Set the width of the video frame (in pixels)
            externalVideoFrame.stride = width;
            //Set the height of the video frame
            externalVideoFrame.height = height;
            //Remove pixels from the sides of the frame
            externalVideoFrame.cropLeft = 10;
            externalVideoFrame.cropTop = 10;
            externalVideoFrame.cropRight = 10;
            externalVideoFrame.cropBottom = 10;
            //Rotate the video frame (0, 90, 180, or 270)
            externalVideoFrame.rotation = 180;
            // increment i with the video timestamp
            externalVideoFrame.timestamp = i++;
            //Push the external video frame with the frame we just created
            // int a = 
            var ret = RtcEngine.PushVideoFrame(externalVideoFrame);
            // Debug.Log(" pushVideoFrame(" + i + ") size:" + bytes.Length + " => " + a);

        }
        yield return null;
        onFinish();
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
        private readonly BroadCast _videoSample;

        internal UserEventHandler(BroadCast videoSample)
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
}
