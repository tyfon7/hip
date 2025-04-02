using System;
using System.IO;
using System.Threading.Tasks;
using EFT;
using Newtonsoft.Json;
using SPT.Common.Http;

namespace HideoutInProgress;

public static class HipServer
{
    public static async Task<AreaProgress[]> Load()
    {
        try
        {
            string jsonPayload = await RequestHandler.GetJsonAsync("/hip/load");
            return JsonConvert.DeserializeObject<AreaProgress[]>(jsonPayload);
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
                areaType = areaType,
                items = items
            };

            await RequestHandler.PutJsonAsync("/hip/contribute", Serialize(request));
            return true;
        }
        catch (Exception ex)
        {
            Plugin.Instance.Logger.LogError("Failed to contribute: " + ex.ToString());
            NotificationManagerClass.DisplayWarningNotification("Hideout contribution failed - check the server");
            return false;
        }
    }

    private static string Serialize<T>(T input)
    {
        // This is necessary to sidestep default serialization settings that have been set, which serialize enums as strings
        JsonSerializer serializer = new();
        using StringWriter sw = new();
        using JsonWriter jw = new JsonTextWriter(sw);

        serializer.Serialize(jw, input);
        return sw.ToString();
    }

    public struct ContributionRequest
    {
        public EAreaType areaType;
        public HideoutItem[] items;
    }
}

public struct Contribution
{
    public string tpl;
    public int count;
}

public struct AreaProgress
{
    public EAreaType areaType;
    public Contribution[] contributions;
}
