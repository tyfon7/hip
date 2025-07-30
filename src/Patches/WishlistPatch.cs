using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.Hideout;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace HideoutInProgress;

// Specifically when called from WishlistManager (GClass1836), remove items with required count of 0 from the enumeration. This way they won't
// be tagged as hideout wishlist
public static class WishlistPatches
{
    private static bool InPatch = false;

    public static void Enable()
    {
        new IsInWishlistPatch().Enable();
        new GetWishlistPatch().Enable();
        new RequirementsPatch().Enable();
    }

    public class IsInWishlistPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WishlistManager), nameof(WishlistManager.IsInWishlist));
        }

        [PatchPrefix]
        public static void Prefix()
        {
            InPatch = true;
        }

        [PatchPostfix]
        public static void Postfix()
        {
            InPatch = false;
        }
    }

    public class GetWishlistPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WishlistManager), nameof(WishlistManager.GetWishlist));
        }

        [PatchPrefix]
        public static void Prefix()
        {
            InPatch = true;
        }

        [PatchPostfix]
        public static void Postfix()
        {
            InPatch = false;
        }
    }

    public class RequirementsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(RelatedRequirements), nameof(RelatedRequirements.GetEnumerator));
        }

        [PatchPrefix]
        public static bool Prefix(RelatedRequirements __instance, ref IEnumerator<Requirement> __result)
        {
            if (!InPatch)
            {
                return true;
            }

            // Normally this function just returns the entire Data. I remove item requirements with count = 0
            var requirements = __instance.Data.Where(r => r is not ItemRequirement itemRequirement || itemRequirement.BaseCount > 0);

            __result = requirements.GetEnumerator();
            return false;
        }
    }
}