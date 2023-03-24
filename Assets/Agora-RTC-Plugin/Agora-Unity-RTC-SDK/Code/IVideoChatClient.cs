/// <summary>
///   Interface definition that use Agora RTC SDK as a clinet.	
/// </summary>
public interface IVideoChatClient
{
    void Join();
    void Leave();
    void LoadEngine(string appId);
    void UnloadEngine();
    void OnSceneLoaded();
    void EnableVideo(bool enable);
}
