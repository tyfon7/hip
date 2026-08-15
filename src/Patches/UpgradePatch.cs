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
        return AccessTools.Method(typeof(HideoutRepresentation), nameof(HideoutRepresentation.GetItemReferences));
    }

    [PatchPrefix]
    public static void Prefix(ref ItemRequirement[] requirements)
    {
        // GetItemReferences doesn't handle 0, so just remove the requirements that are 0
        requirements = requirements.Where(r => r.IntCount > 0).ToArray();
    }
}