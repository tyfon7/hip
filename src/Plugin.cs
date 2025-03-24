using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;

namespace HideoutInProgress;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance;

    public new ManualLogSource Logger => base.Logger;

    public void Awake()
    {
        Instance = this;

        Settings.Init(Config);
    }

    public static bool InRaid()
    {
        var instance = Singleton<AbstractGame>.Instance;
        return instance != null && instance.InRaid;
    }
}
