using System.Reflection;
using EFT.Hideout;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace HideoutInProgress;

public class ButtonPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.DeclaredMethod(typeof(RequirementsPanel), nameof(RequirementsPanel.ShowContents));
    }

    [PatchPostfix]
    public static void Postfix(RequirementsPanel __instance)
    {
        var areaScreen = __instance.GetComponentInParent<AreaScreenSubstrate>();
        var backButton = areaScreen.transform.Find("Content/NextLevel/BottomPanel/CurrentLevelButton");

        var clone = UnityEngine.Object.Instantiate(backButton, __instance._itemContainer.parent, false);
        clone.name = "HipTransferButton";
        clone.SetSiblingIndex(__instance._itemContainer.GetSiblingIndex() + 1);

        var transferButton = clone.gameObject.AddComponent<TransferButton>();
        transferButton.Init(__instance.AreaData, __instance.Info);
    }
}