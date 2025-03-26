using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EFT;
using EFT.Hideout;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace HideoutInProgress;

public class LoadPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(HideoutClass), nameof(HideoutClass.method_9));
    }

    [PatchPostfix]
    public static async void Postfix(Dictionary<EAreaType, AreaData> ___dictionary_0)
    {
        AreaProgress[] progress = await HipServer.Load();

        foreach (var areaProgress in progress)
        {
            if (!___dictionary_0.TryGetValue(areaProgress.areaType, out AreaData areaData))
            {
                continue;
            }

            var itemRequirements = areaData.NextStage.Requirements.OfType<ItemRequirement>().Where(r => r.Item is not MoneyItemClass);
            foreach (var contribution in areaProgress.contributions ?? [])
            {
                var requirement = itemRequirements.FirstOrDefault(r => r.Item.TemplateId == contribution.tpl);
                if (requirement != null)
                {
                    requirement.BaseCount -= contribution.count;
                }
            }
        }
    }
}