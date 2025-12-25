using System;
using System.Reflection;
using HarmonyLib;

namespace HideoutInProgress;

public static class HideoutExtensions
{
    private static readonly FieldInfo AreaUpdatedField = AccessTools.Field(typeof(HideoutClass), "action_2");

    public static void FireUpdateArea(this HideoutClass hideout)
    {
        var action = AreaUpdatedField.GetValue(hideout) as Action;
        action?.Invoke();
    }
}