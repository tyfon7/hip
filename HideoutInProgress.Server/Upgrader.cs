using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using Microsoft.Extensions.DependencyInjection;
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
            var profileDataHelper = ServiceLocator.ServiceProvider.GetService<ProfileDataHelper>();
            var profileData = profileDataHelper.GetProfileData(pmcData.Id.Value);

            if (profileData.AreaProgresses.Remove(request.AreaType.Value))
            {
                profileDataHelper.SaveProfileData(pmcData.Id.Value);
            }
        }
    }
}