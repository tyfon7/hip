using System.Reflection;
using EFT.Hideout;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace HideoutInProgress;

public class CountPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(ItemRequirementPanel.CG_Show), nameof(ItemRequirementPanel.CG_Show.method_0));
    }

    [PatchPostfix]
    public static void PatchPostfix(ItemRequirementPanel.CG_Show __instance)
    {
        if (!__instance.ignoreFulfillment && __instance.itemRequirement.IntCount > 0)
        {
            return;
        }

        __instance.itemRequirementPanel_0._itemIconViewFactory.SetCounterText(" ");
    }
}