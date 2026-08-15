using System;
using System.Reflection;
using EFT.Hideout;
using HarmonyLib;

namespace HideoutInProgress;

public static class HideoutExtensions
{
    private static readonly FieldInfo AreaUpdatedField = AccessTools.Field(typeof(HideoutRepresentation), "_onAreaUpdated");

    public static void FireUpdateArea(this HideoutRepresentation hideout)
    {
        var action = AreaUpdatedField.GetValue(hideout) as Action;
        action?.Invoke();
    }
}