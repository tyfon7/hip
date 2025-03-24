using System.Collections.Generic;
using System.ComponentModel;
using BepInEx.Configuration;

namespace HideoutInProgress;

internal class Settings
{
    private const string GeneralSection = "General";

    public static void Init(ConfigFile config)
    {
        var configEntries = new List<ConfigEntryBase>();

        RecalcOrder(configEntries);
    }

    private static void RecalcOrder(List<ConfigEntryBase> configEntries)
    {
        // Set the Order field for all settings, to avoid unnecessary changes when adding new settings
        int settingOrder = configEntries.Count;
        foreach (var entry in configEntries)
        {
            if (entry.Description.Tags[0] is ConfigurationManagerAttributes attributes)
            {
                attributes.Order = settingOrder;
            }

            settingOrder--;
        }
    }
}