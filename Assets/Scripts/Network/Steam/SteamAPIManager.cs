using NGOManager.Utility.Singleton;
using Steamworks;
using System.Text;
using UnityEngine;

[DisallowMultipleComponent]
public class SteamAPIManager : SingletonPersistent<SteamAPIManager>
{
    public bool Initialized { get; private set; }

    private SteamAPIWarningMessageHook_t steamAPIWarningMessageHook;


    protected override void Awake()
    {
        base.Awake();

        if (!Packsize.Test())
        {
            Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
        }
        if (!DllCheck.Test())
        {
            Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
        }

        try
        {
            //if (SteamAPI.RestartAppIfNecessary((AppId_t)480))
            if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid))
            {
                Debug.Log("[Steamworks.NET] Shutting down because RestartAppIfNecessary returned true. Steam will restart the application.");

                Application.Quit();
                return;
            }
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);

            Application.Quit();
            return;
        }

        Initialized = SteamAPI.Init();
        if (Initialized == false)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);

            Application.Quit();
            return;
        }

        if (steamAPIWarningMessageHook == null)
        {
            steamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
            SteamClient.SetWarningMessageHook(steamAPIWarningMessageHook);
        }
    }

    private void Update()
    {
        if (Initialized == false) return;

        SteamAPI.RunCallbacks();
    }

    private void OnDestroy()
    {
        if (Initialized == false) return;

        SteamAPI.Shutdown();
    }

    private void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
    {
        Debug.LogWarning(pchDebugText, this);
    }
}
