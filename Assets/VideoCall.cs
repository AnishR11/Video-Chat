using Agora.Rtc;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class VideoCall : MonoBehaviour
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
    // Start is called before the first frame update
    void Start()
    {
        SetupVideoSDKEngine();
        SetupUI();
        //Changes
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

    private void SetupUI()
    {
        GameObject go = GameObject.Find("LocalView");
        LocalView = go.AddComponent<VideoSurface>();
        go.transform.Rotate(0.0f, 0.0f, 180.0f);
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
            //timeStamp = (ulong)(Time.realtimeSinceStartup * 1000)
        });
    }

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
}
