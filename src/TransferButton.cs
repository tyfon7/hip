using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.Hideout;
using EFT.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HideoutInProgress;

public class TransferButton : MonoBehaviour, IPointerClickHandler
{
    private static readonly FieldInfo UIField = AccessTools.Field(typeof(UIElement), "UI");

    private EAreaType areaType;
    private IEnumerable<ItemRequirement> itemRequirements;

    private DefaultUIButton button;
    private AddViewListClass UI;

    public void Init(EAreaType areaType, List<Requirement> requirements)
    {
        this.areaType = areaType;

        button = GetComponent<DefaultUIButton>();
        button.SetHeaderText("HideoutInteractions/TransferItems");

        UI = (AddViewListClass)UIField.GetValue(button);

        // Items only, and not money (money is all or nothing)
        itemRequirements = requirements.OfType<ItemRequirement>().Where(r => r.Item is not MoneyItemClass);
        foreach (var itemRequirement in itemRequirements)
        {
            UI.AddDisposable(itemRequirement.OnFulfillmentChange.Subscribe(UpdateInteractable));
        }

        gameObject.SetActive(true);
        UpdateInteractable();
    }

    private void UpdateInteractable()
    {
        // Apparently the AddDisposable isn't thread safe, check this isn't destroyed
        if (button != null)
        {
            button.Interactable = itemRequirements.Any(r => r.IntCount > 0 && r.UserItemsCount > 0);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.Interactable)
        {
            return;
        }

        Contribute().HandleExceptions();
    }

    private async Task Contribute()
    {
        var hideout = Singleton<HideoutClass>.Instance;

        // Get items that satisfy requirements. This doesn't check that it *fully* fulfills requirements
        List<HideoutItem> hideoutItems = hideout.method_16(itemRequirements.ToArray());

        // Do the client side delete operations
        var deleteOperations = hideout.method_17(hideoutItems);
        if (!await HipServer.Contribute(areaType, hideoutItems.ToArray()))
        {
            deleteOperations.RollBack();
            return;
        }

        UpdateRequirements(hideoutItems);

        // Recalc various hideout things
        hideout.method_19();
    }

    private void UpdateRequirements(IEnumerable<HideoutItem> transferredItems)
    {
        foreach (var transferredItem in transferredItems)
        {
            var requirement = itemRequirements.Single(r => r.TemplateId == transferredItem.Item.TemplateId);
            requirement.BaseCount -= transferredItem.Count;
            requirement.Retest();
        }
    }
}