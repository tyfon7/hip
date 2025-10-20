using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EFT;
using Newtonsoft.Json;
using SPT.Common.Http;

namespace HideoutInProgress;

using LoadPayload = Dictionary<EAreaType, Dictionary<string, int>>;

public static class HipServer
{
    public static async Task<LoadPayload> Load()
    {
        try
        {
            string jsonPayload = await RequestHandler.GetJsonAsync("/hip/load");
            return JsonConvert.DeserializeObject<LoadPayload>(jsonPayload);
        }
        catch (Exception ex)
        {
            Plugin.Instance.Logger.LogError("Failed to load: " + ex.ToString());
            NotificationManagerClass.DisplayWarningNotification("Hideout In Progress failed to load - check the server");
            return [];
        }
    }

    public static async Task<bool> Contribute(EAreaType areaType, HideoutItem[] items)
    {
        try
        {
            var request = new ContributionRequest()
            {
                area = areaType,
                items = items
            };

            await RequestHandler.PutJsonAsync("/hip/contribute", JsonConvert.SerializeObject(request));
            return true;
        }
        catch (Exception ex)
        {
            Plugin.Instance.Logger.LogError("Failed to contribute: " + ex.ToString());
            NotificationManagerClass.DisplayWarningNotification("Hideout contribution failed - check the server");
            return false;
        }
    }

    public struct ContributionRequest
    {
        public EAreaType area;
        public HideoutItem[] items;
    }
}