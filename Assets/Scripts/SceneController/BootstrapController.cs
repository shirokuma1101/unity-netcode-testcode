using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BootstrapController : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown dropdownPlatform;
    [SerializeField]
    private Button buttonContinue;

    private void Start()
    {
        List<string> options = new();

        foreach (var item in Enum.GetValues(typeof(Bootstrap.Platforms)))
        {
            options.Add(item.ToString());
        }

        dropdownPlatform.AddOptions(options);
        buttonContinue.onClick.AddListener(GoToTitle);
    }

    public void GoToTitle()
    {
        Bootstrap.Instance.InitializeNetworkAPI(dropdownPlatform.value);
        LoadingSceneManager.Instance.LoadSceneAsync(LoadingSceneManager.SceneName.Title, LoadSceneMode.Single, false).Forget();
    }
}
