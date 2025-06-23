using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(NetworkRunner))]
public class FusionBootstrap : MonoBehaviour
{
    private async void Start()
    {
        var runner = GetComponent<NetworkRunner>();
        // 이미 실행 중이면 두 번째 StartGame은 스킵
        if (runner.IsRunning)
        {
            Debug.Log("Runner already running, skipping StartGame");
            return;
        }

        runner.ProvideInput = true;               // 내 입력 전송

        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(
            SceneManager.GetActiveScene().buildIndex));

        runner.AddCallbacks(FindAnyObjectByType<PlayerSpawner>());

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Host,          // 로컬 Host 모드
            SessionName = "LocalRoom",
            Scene = sceneInfo,
            SceneManager = GetComponent<NetworkSceneManagerDefault>()
        });

        Debug.Log($"Runner started? {result.Ok}");
    }
}
