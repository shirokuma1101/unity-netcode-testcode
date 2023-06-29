using NGOManager.Utility.Singleton;
using RapidGUI;

public class DebugGUIManager : SingletonPersistent<DebugGUIManager>
{
    public WindowLaunchers Launchers { get; set; }


    private void Start()
    {
        Launchers = new WindowLaunchers
        {
            name = "Debug GUI Manager"
        };
    }

    private void OnGUI()
    {
        Launchers.DoGUI();
    }
}
