using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;

namespace HideoutInProgress.Server;

[Injectable(TypePriority = OnLoadOrder.PreSptModLoader)]
public class Upgrader : IOnLoad
{
    public Task OnLoad()
    {
        new StartUpgradePatch().Enable();

        return Task.CompletedTask;
    }

    private class StartUpgradePatch : AbstractPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(HideoutController), nameof(HideoutController.StartUpgrade));
        }

        [PatchPrefix]
        public static void Prefix(PmcData pmcData, HideoutUpgradeRequestData request)
        {
            var area = pmcData.Hideout.Areas.Find(a => a.Type == request.AreaType);
            area.ExtensionData.Remove("contributions");
        }
    }
}