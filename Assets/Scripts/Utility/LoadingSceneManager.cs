using Cysharp.Threading.Tasks;
using System;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using Utility.Singleton;

public class LoadingSceneManager : SingletonPersistent<LoadingSceneManager>
{
    public enum SceneName
    {
        Bootstrap,
        Title,
        Menu,
        MatchMaking,
        Game,
        Result,
    }

    /// <summary>
    /// 
    /// </summary>
    public SceneName ActiveSceneInLobby { get; private set; } = SceneName.Bootstrap;


    private async void Start()
    {
        var cancelToken = this.GetCancellationTokenOnDestroy();

        // Wait until NetworkManager is instantiated
        await UniTask.WaitUntil(() => NetworkManager.Singleton != null, cancellationToken: cancelToken);
        // Wait until NetworkManager is Host or Client connected
        await UniTask.WaitUntil(() => NetworkManager.Singleton.SceneManager != null, cancellationToken: cancelToken);

        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }

    public async UniTask LoadSceneAsync(SceneName sceneName, LoadSceneMode mode, bool isNetwork)
    {
        var cancelToken = this.GetCancellationTokenOnDestroy();

        if (isNetwork && NetworkManager.Singleton.IsHost)
        {
            await UniTask.WaitUntil(() =>
            {
                NetworkManager.Singleton.SceneManager.LoadScene(sceneName.ToString(), mode);
                return true;
            },
            cancellationToken: cancelToken);
        }
        else
        {
            await UniTask.WaitUntil(() =>
            {
                SceneManager.LoadScene(sceneName.ToString(), mode);
                return true;
            },
            cancellationToken: cancelToken);
        }

        ActiveSceneInLobby = sceneName;
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (!NetworkManager.Singleton.IsHost) return;
    }
}
