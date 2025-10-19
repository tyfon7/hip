using System.Linq;
using System.Reflection;
using EFT.Hideout;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace HideoutInProgress;

public class UpgradePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(HideoutClass), nameof(HideoutClass.method_21));
    }

    [PatchPrefix]
    public static void Prefix(ref ItemRequirement[] requirements)
    {
        // method_21 doesn't handle 0, so just remove the requirements that are 0
        requirements = requirements.Where(r => r.IntCount > 0).ToArray();
    }
}