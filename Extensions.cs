using System.Linq;
using EFT.InventoryLogic;

namespace HideoutInProgress;

public static class Extensions
{
    public static Item GetRootItemNotEquipment(this Item item)
    {
        return item.GetAllParentItemsAndSelf(true).LastOrDefault(i => i is not InventoryEquipment) ?? item;
    }

    public static Item GetRootItemNotEquipment(this ItemAddress itemAddress)
    {
        if (itemAddress.Container == null || itemAddress.Container.ParentItem == null)
        {
            return null;
        }

        return itemAddress.Container.ParentItem.GetRootItemNotEquipment();
    }
}