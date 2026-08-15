using System.Reflection;
using HarmonyLib;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;

namespace HideoutInProgress.Server;

[Injectable]
public class StartUpgradePatch : AbstractPatch
{
    private static ProfileDataHelper ProfileDataHelper;

    public StartUpgradePatch(ProfileDataHelper profileDataHelper)
    {
        ProfileDataHelper = profileDataHelper;
    }

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(HideoutController), nameof(HideoutController.StartUpgrade));
    }

    [PatchPrefix]
    public static void Prefix(PmcData pmcData, HideoutUpgradeRequestData request)
    {
        var profileData = ProfileDataHelper.GetProfileData(pmcData.Id.Value);

        if (profileData.AreaProgresses.Remove(request.AreaType.Value))
        {
            ProfileDataHelper.SaveProfileData(pmcData.Id.Value);
        }
    }
}