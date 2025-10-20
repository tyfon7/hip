using System.Collections.Generic;
using System.Threading.Tasks;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;

namespace HideoutInProgress.Server;

[Injectable]
public class HideoutInProgressCallbacks(
    ISptLogger<HideoutInProgressCallbacks> logger,
    ProfileHelper profileHelper,
    InventoryHelper inventoryHelper)
{
    public ValueTask<IEnumerable<AreaProgress>> GetAreaProgresses(MongoId sessionId)
    {
        var pmcData = profileHelper.GetPmcProfile(sessionId);

        List<AreaProgress> results = [];
        foreach (var area in pmcData.Hideout.Areas)
        {
            if (area.ExtensionData.TryGetValue("contributions", out object value))
            {
                var contributions = value as List<Contribution>;
                results.Add(new AreaProgress { Area = area.Type, Contributions = contributions });
            }
        }

        return ValueTask.FromResult<IEnumerable<AreaProgress>>(results);
    }

    public ValueTask<bool> Contribute(ContributionRequestData request, MongoId sessionId)
    {
        var pmcData = profileHelper.GetPmcProfile(sessionId);
        var area = pmcData.Hideout.Areas.Find(a => a.Type == request.Area);

        if (area == null)
        {
            logger.Error($"HideoutInProgress cannot find area of type {request.Area}");
            return ValueTask.FromResult(false);
        }

        // Create mapping of required item with corrisponding item from player inventory (copied)
        Dictionary<Item, HideoutItem> map = [];
        foreach (var requestItem in request.Items)
        {
            var item = pmcData.Inventory.Items.Find(i => i.Id == requestItem.Id);
            if (item == null)
            {
                logger.Error($"HideoutInProgress: Cannot find item {requestItem.Id}");
                continue;
            }

            map.Add(item, requestItem);
        }

        if (map.Count == 0)
        {
            logger.Warning("HideoutInProgress: Contributed 0 items");
            return ValueTask.FromResult(false);
        }

        List<Contribution> contributions;
        if (area.ExtensionData.TryGetValue("contributions", out object value))
        {
            contributions = value as List<Contribution>;
        }
        else
        {
            contributions = [];
            area.ExtensionData.Add("contributions", contributions);
        }

        int totalCount = 0;
        foreach (var (item, requestItem) in map)
        {
            var contribution = contributions.Find(c => c.TemplateId == item.Template);
            if (contribution == null)
            {
                contribution = new Contribution { TemplateId = item.Template, Count = 0 };
                contributions.Add(contribution);
            }

            // Record the contribution
            contribution.Count += (int)requestItem.Count; // why is this a double?
            totalCount += contribution.Count;

            // Remove the item from the inventory
            inventoryHelper.RemoveItem(pmcData, item.Id, sessionId);
        }

        logger.Success($"HideoutInProgress: Contributed {totalCount} items");
        return ValueTask.FromResult(true);
    }
}