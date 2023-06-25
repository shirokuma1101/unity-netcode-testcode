using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility.EnumEx;

public class MenuController : MonoBehaviour
{
    //TODO: Host/Client Init() Username, Password
    //TODO: Client Update() Steam: Search lobby

    [Serializable]
    public struct RootUIItems
    {
        public GameObject topLevelMenuObj;
        public Button buttonCreateLobby;
        public Button buttonJoinLobby;
        public Button buttonQuit;
    }

    [Serializable]
    public struct HostMenuElements
    {
        public GameObject menuObj;
        
        [Header("Common")]
        public Slider sliderMaxMembers;
        public TMP_InputField inputMaxMembers;
        public Button buttonCreate;

        [Header("It depends")]
        public GameObject dropdownConnectionTypeObj;
        public TMP_Dropdown dropdownConnectionType;

        [Header("Direct (switching)")]
        public GameObject directObj;
        public TMP_InputField inputIPAddress;
        public TMP_InputField inputPort;

        [Header("Steam (switching)")]
        public GameObject steamObj;
        public TMP_Dropdown dropdownLobbyType;
    }

    [Serializable]
    public struct ClientMenuElements
    {
        public GameObject menuObj;

        [Header("Common")]
        public Button buttonJoin;

        [Header("It depends")]
        public GameObject dropdownConnectionTypeObj;
        public TMP_Dropdown dropdownConnectionType;

        [Header("Direct (switching)")]
        public GameObject directObj;
        public TMP_InputField inputIPAddress;
        public TMP_InputField inputPort;

        [Header("Steam (switching)")]
        public GameObject steamObj;
        public TMP_InputField inputLobbyID;
    }

    [SerializeField]
    private RootUIItems rootUIItems;
    [SerializeField]
    private HostMenuElements hostMenuElements;
    [SerializeField]
    private ClientMenuElements clientMenuElements;

    private GameObject nowSwitchingUI;


    private void Start()
    {
        rootUIItems.buttonCreateLobby.onClick.AddListener(OnClickCreateLobby);
        rootUIItems.buttonJoinLobby.onClick.AddListener(OnClickJoinLobby);
        rootUIItems.buttonQuit.onClick.AddListener(OnClickQuit);
    }

    private void Update()
    {
        if (hostMenuElements.menuObj.activeSelf)
        {
            UpdateInHostMenu();
        }
        else if (clientMenuElements.menuObj.activeSelf)
        {
            UpdateInClientMenu();
        }
    }

    private void OnClickCreateLobby()
    {
        rootUIItems.topLevelMenuObj.SetActive(false);
        hostMenuElements.menuObj.SetActive(true);
        InitInHostMenu();
    }

    private void OnClickJoinLobby()
    {
        rootUIItems.topLevelMenuObj.SetActive(false);
        clientMenuElements.menuObj.SetActive(true);
        InitInClientMenu();
    }

    private void OnClickQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void InitInHostMenu()
    {
        hostMenuElements.sliderMaxMembers.minValue = LobbyBase.MIN_MEMBERS;
        hostMenuElements.sliderMaxMembers.maxValue = LobbyBase.MAX_MEMBERS;
        hostMenuElements.sliderMaxMembers.value = LobbyBase.MIN_MEMBERS;
        hostMenuElements.inputMaxMembers.text = LobbyBase.MIN_MEMBERS.ToString();

        hostMenuElements.sliderMaxMembers.onValueChanged.AddListener((float value) =>
        {
            hostMenuElements.inputMaxMembers.text = value.ToString();
        });

        hostMenuElements.buttonCreate.onClick.AddListener(OnClickCreateAsync);


        // PlatformのAvailableConnectionTypeが複数ある場合
        List<string> options = new();
        if (MakeCTOptions(ref options))
        {
            hostMenuElements.dropdownConnectionTypeObj.SetActive(true);
            hostMenuElements.dropdownConnectionType.AddOptions(options);
            hostMenuElements.dropdownConnectionType.onValueChanged.AddListener((int value) =>
            {
                ChangeInHostNowSwitchingUI();
            });
        }
        ChangeInHostNowSwitchingUI();
    }

    private void InitInClientMenu()
    {
        clientMenuElements.buttonJoin.onClick.AddListener(OnClickJoinAsync);


        // PlatformのAvailableConnectionTypeが複数ある場合
        List<string> options = new();
        if (MakeCTOptions(ref options))
        {
            clientMenuElements.dropdownConnectionTypeObj.SetActive(true);
            clientMenuElements.dropdownConnectionType.AddOptions(options);
            clientMenuElements.dropdownConnectionType.onValueChanged.AddListener((int value) =>
            {
                ChangeInClientNowSwitchingUI();
            });
        }
        ChangeInClientNowSwitchingUI();
    }

    private void UpdateInHostMenu()
    {
        // 入力フィールドのフォーカスが外れたら、値を制限する
        if (hostMenuElements.inputMaxMembers.isFocused == false)
        {
            int.TryParse(hostMenuElements.inputMaxMembers.text, out int result);
            hostMenuElements.inputMaxMembers.text = LobbyBase.ClampMaxMembers(result).ToString();
        }
    }

    private void UpdateInClientMenu()
    {

    }

    private async void OnClickCreateAsync()
    {
        LobbySystem.Instance.ApplyTransport();
        LobbySystem.Instance.Setup(true);

        LobbySystem.Instance.CurrentLobby.MaxMembers = int.Parse(hostMenuElements.inputMaxMembers.text);
        switch (LobbySystem.Instance.ConnectionType)
        {
            case Bootstrap.AvailableConnectionType.Direct:
                DirectLobby.Instance.IPAddress = hostMenuElements.inputIPAddress.text;
                DirectLobby.Instance.Port = ushort.Parse(hostMenuElements.inputPort.text);
                break;
            case Bootstrap.AvailableConnectionType.Steam:
                SteamLobby.Instance.LobbyType = (Steamworks.ELobbyType)hostMenuElements.dropdownLobbyType.value;
                break;
        }

        static void StartHost(Scene scene, Scene mode)
        {
            LobbySystem.Instance.StartHost();
            SceneManager.activeSceneChanged -= StartHost;
        }
        SceneManager.activeSceneChanged += StartHost;
        await LoadingSceneManager.Instance.LoadSceneAsync(LoadingSceneManager.SceneName.MatchMaking, LoadSceneMode.Single, false);
    }

    private async void OnClickJoinAsync()
    {
        LobbySystem.Instance.ApplyTransport();
        LobbySystem.Instance.Setup();

        switch (LobbySystem.Instance.ConnectionType)
        {
            case Bootstrap.AvailableConnectionType.Direct:
                DirectLobby.Instance.IPAddress = clientMenuElements.inputIPAddress.text;
                DirectLobby.Instance.Port = ushort.Parse(clientMenuElements.inputPort.text);
                break;
            case Bootstrap.AvailableConnectionType.Steam:
                SteamLobby.Instance.LobbyID = ulong.Parse(clientMenuElements.inputLobbyID.text);
                break;
        }

        static void StartClient(Scene scene, Scene mode)
        {
            LobbySystem.Instance.StartClient();
            SceneManager.activeSceneChanged -= StartClient;
        }
        SceneManager.activeSceneChanged += StartClient;
        await LoadingSceneManager.Instance.LoadSceneAsync(LoadingSceneManager.SceneName.MatchMaking, LoadSceneMode.Single, false);
    }

    private void ChangeInHostNowSwitchingUI()
    {
        if (nowSwitchingUI != null)
        {
            nowSwitchingUI.SetActive(false);
        }

        string connectionType = Bootstrap.Instance.Platform.ToString();
        if (hostMenuElements.dropdownConnectionTypeObj.activeSelf)
        {
            connectionType
                = hostMenuElements.dropdownConnectionType
                .gameObject.transform.Find("Label")
                .gameObject.GetComponent<TextMeshProUGUI>().text;
        }

        switch (connectionType)
        {
            case nameof(Bootstrap.AvailableConnectionType.Direct):
                LobbySystem.Instance.ConnectionType = Bootstrap.AvailableConnectionType.Direct;

                nowSwitchingUI = hostMenuElements.directObj;
                nowSwitchingUI.SetActive(true);
                hostMenuElements.inputIPAddress.text = DirectLobby.DEFAULT_IP_ADDRESS;
                hostMenuElements.inputPort.text = DirectLobby.DEFAULT_PORT.ToString();

                break;
            case nameof(Bootstrap.AvailableConnectionType.Steam):
                LobbySystem.Instance.ConnectionType = Bootstrap.AvailableConnectionType.Steam;

                nowSwitchingUI = hostMenuElements.steamObj;
                nowSwitchingUI.SetActive(true);
                hostMenuElements.dropdownLobbyType.ClearOptions();
                {
                    List<string> options = new();
                    // 長いので、k_ELobbyを削除
                    var regex = new Regex(@"^k_ELobby");
                    foreach (var item in Enum.GetValues(typeof(Steamworks.ELobbyType)))
                    {
                        options.Add(regex.Replace(item.ToString(), ""));
                    }
                    hostMenuElements.dropdownLobbyType.AddOptions(options);
                }

                break;
        }
    }

    private void ChangeInClientNowSwitchingUI()
    {
        if (nowSwitchingUI != null)
        {
            nowSwitchingUI.SetActive(false);
        }

        string connectionType = Bootstrap.Instance.Platform.ToString();
        if (clientMenuElements.dropdownConnectionTypeObj.activeSelf)
        {
            connectionType
                = clientMenuElements.dropdownConnectionType
                .gameObject.transform.Find("Label")
                .gameObject.GetComponent<TextMeshProUGUI>().text;
        }

        switch (connectionType)
        {
            case nameof(Bootstrap.AvailableConnectionType.Direct):
                LobbySystem.Instance.ConnectionType = Bootstrap.AvailableConnectionType.Direct;

                nowSwitchingUI = clientMenuElements.directObj;
                nowSwitchingUI.SetActive(true);
                clientMenuElements.inputIPAddress.text = DirectLobby.DEFAULT_IP_ADDRESS;
                clientMenuElements.inputPort.text = DirectLobby.DEFAULT_PORT.ToString();

                break;
            case nameof(Bootstrap.AvailableConnectionType.Steam):
                LobbySystem.Instance.ConnectionType = Bootstrap.AvailableConnectionType.Steam;

                nowSwitchingUI = clientMenuElements.steamObj;
                nowSwitchingUI.SetActive(true);

                break;
        }
    }

    private bool MakeCTOptions(ref List<string> options)
    {
        if (EnumEx.MultipleFlagExists<Bootstrap.AvailableConnectionType, Bootstrap.Platforms>(Bootstrap.Instance.Platform))
        {
            foreach (Bootstrap.AvailableConnectionType type in Enum.GetValues(typeof(Bootstrap.AvailableConnectionType)))
            {
                if (EnumEx.HasFlag(type, Bootstrap.Instance.Platform))
                {
                    options.Add(type.ToString());
                }
            }
            return true;
        }
        return false;
    }
}
