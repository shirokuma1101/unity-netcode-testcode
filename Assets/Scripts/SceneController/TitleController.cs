using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleController : MonoBehaviour
{
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            LoadingSceneManager.Instance.LoadSceneAsync(LoadingSceneManager.SceneName.Menu, LoadSceneMode.Single, false).Forget();
        }
    }
}
