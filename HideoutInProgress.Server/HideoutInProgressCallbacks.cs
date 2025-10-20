using System.Collections.Generic;
using System.Threading.Tasks;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;

namespace HideoutInProgress.Server;

using GetProgressPayload = Dictionary<HideoutAreas, Dictionary<string, int>>;

[Injectable]
public class HideoutInProgressCallbacks(
    ISptLogger<HideoutInProgressCallbacks> logger,
    ProfileHelper profileHelper,
    ProfileDataHelper profileDataHelper,
    InventoryHelper inventoryHelper,
    EventOutputHolder eventOutputHolder)
{
    public ValueTask<GetProgressPayload> GetProgress(MongoId sessionId)
    {
        var pmcData = profileHelper.GetPmcProfile(sessionId);
        var profileData = profileDataHelper.GetProfileData(pmcData.Id.Value);

        GetProgressPayload results = [];
        foreach (var area in pmcData.Hideout.Areas)
        {
            if (profileData.AreaProgresses.TryGetValue(area.Type, out var progress))
            {
                results[area.Type] = progress;
            }
        }

        return ValueTask.FromResult(results);
    }

    public ValueTask<bool> Contribute(ContributionRequestData request, MongoId sessionId)
    {
        var pmcData = profileHelper.GetPmcProfile(sessionId);

        var profileData = profileDataHelper.GetProfileData(pmcData.Id.Value);
        if (!profileData.AreaProgresses.TryGetValue(request.Area, out var progress))
        {
            profileData.AreaProgresses[request.Area] = progress = [];
        }

        int totalCount = 0;
        List<Item> inventoryItems = [];
        foreach (var hideoutItem in request.Items)
        {
            var count = (int)hideoutItem.Count;

            var item = pmcData.Inventory.Items.Find(i => i.Id == hideoutItem.Id);
            if (item == null)
            {
                logger.Error($"HideoutInProgress: Cannot find item {hideoutItem.Id}");
                continue;
            }

            if (!progress.TryGetValue(item.Template, out int existingCount))
            {
                existingCount = 0;
            }

            // Record the contribution
            progress[item.Template] = existingCount + count;
            totalCount += count;

            // Remove the item from the inventory
            var dummyOutput = eventOutputHolder.GetOutput(sessionId);
            inventoryHelper.RemoveItemByCount(pmcData, item.Id, (int)hideoutItem.Count, sessionId, dummyOutput);
        }

        profileDataHelper.SaveProfileData(pmcData.Id.Value);
        logger.Success($"HideoutInProgress: Contributed {totalCount} items");

        return ValueTask.FromResult(true);
    }
}