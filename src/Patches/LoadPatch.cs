using System.Linq;
using System.Reflection;
using EFT.Hideout;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace HideoutInProgress;

public class LoadPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(HideoutRepresentation), nameof(HideoutRepresentation.method_14));
    }

    [PatchPostfix]
    public static async void Postfix(HideoutRepresentation __instance)
    {
        var data = await HipServer.Load();

        foreach (var (areaType, contributions) in data)
        {
            if (!__instance._areaDataIndex.TryGetValue(areaType, out AreaData areaData))
            {
                continue;
            }

            var itemRequirements = areaData.NextStage.Requirements.OfType<ItemRequirement>().Where(r => r.Item is not Money);
            foreach (var (templateId, count) in contributions ?? [])
            {
                var requirement = itemRequirements.FirstOrDefault(r => r.Item.TemplateId == templateId);
                if (requirement != null)
                {
                    requirement.BaseCount -= count;
                    requirement.Retest();
                }
            }

            areaData.DecideStatus(areaData.CurrentLevel);
        }

        Plugin.WishlistExtendedForceRebuild();
    }
}