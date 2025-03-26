using System;
using System.Linq;
using System.Reflection;
using EFT.Hideout;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace HideoutInProgress;

public class CountPatch : ModulePatch
{
    private static FieldInfo HideoutItemViewFactoryField;
    protected override MethodBase GetTargetMethod()
    {
        HideoutItemViewFactoryField = AccessTools.Field(typeof(ItemRequirementPanel), "_itemIconViewFactory");

        Type type = typeof(ItemRequirementPanel).GetNestedTypes().First();
        return AccessTools.Method(type, "method_0");
    }

    [PatchPostfix]
    public static void PatchPostfix(ItemRequirement ___itemRequirement, ItemRequirementPanel ___itemRequirementPanel_0)
    {
        if (___itemRequirement.IntCount > 0)
        {
            return;
        }

        var viewFactory = (HideoutItemViewFactory)HideoutItemViewFactoryField.GetValue(___itemRequirementPanel_0);
        viewFactory.SetCounterText(" ");
    }
}