using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.UI;

namespace HideoutInProgress;

[BepInPlugin("com.tyfon.hideoutinprogress", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin Instance;

    public new ManualLogSource Logger => base.Logger;

    public void Awake()
    {
        Instance = this;

        // Settings.Init(Config);

        new LoadPatch().Enable();
        new ButtonPatch().Enable();
        new CountPatch().Enable();
        new UpgradePatch().Enable();

        WishlistPatches.Enable();
    }

    public static bool InRaid()
    {
        var instance = Singleton<AbstractGame>.Instance;
        return instance != null && instance.InRaid;
    }

    public static void WishlistExtendedForceRebuild()
    {
        if (Chainloader.PluginInfos.ContainsKey("com.zgfuedkx.wishlistextended"))
        {
            Singleton<HideoutClass>.Instance.FireUpdateArea(); // Will invalidate their cache

            // Due to how I patch RelatedRequirements.GetEnumerator and how he patches IsInWishlist/GetWishlist, I need to force 
            // WishlistExtended to not just clear its cache but to rebuild it. I need their cache rebuilt from a call to GetWishlist,
            // since that will trigger my logic removing requirements with 0 left
            var _ = ItemUiContext.Instance.WishlistManager.GetWishlist();
        }
    }
}
