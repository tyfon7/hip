using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Comfort.Common;
using EFT.Hideout;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HideoutInProgress;

public class TransferButton : MonoBehaviour, IPointerClickHandler
{
    private static readonly FieldInfo UIField = AccessTools.Field(typeof(UIElement), "UI");

    private static readonly EAreaStatus[] InteractableStatuses =
    [
        EAreaStatus.LockedToConstruct,
        EAreaStatus.ReadyToConstruct,
        EAreaStatus.LockedToUpgrade,
        EAreaStatus.ReadyToUpgrade
    ];

    private AreaData _areaData;
    private ItemRequirement[] _itemRequirements;

    private DefaultUIButton _button;

    public void Init(AreaData areaData, List<Requirement> requirements)
    {
        _areaData = areaData;

        _button = GetComponent<DefaultUIButton>();
        _button.SetHeaderText("HideoutInteractions/TransferItems");

        var ui = (AddViewListClass)UIField.GetValue(_button);

        // Items only, and not money (money is all or nothing)
        _itemRequirements = requirements.OfType<ItemRequirement>().Where(r => r.Item is not MoneyItemClass).ToArray();
        foreach (var itemRequirement in _itemRequirements)
        {
            ui.AddDisposable(itemRequirement.OnFulfillmentChange.Subscribe(UpdateInteractable));
        }

        ui.AddDisposable(areaData.StatusUpdated.Subscribe(UpdateInteractable));

        gameObject.SetActive(true);
        UpdateInteractable();
    }

    private void UpdateInteractable()
    {
        // Apparently the AddDisposable isn't thread safe, check this isn't destroyed
        if (_button == null)
        {
            return;
        }

        if (!InteractableStatuses.Contains(_areaData.Status))
        {
            _button.Interactable = false;
            return;
        }

        foreach (var requirement in _itemRequirements)
        {
            var userCount = requirement.UserItemsCount;
            if (requirement.Item is CompoundItem)
            {
                userCount -= requirement.NotEmptyCompoundItems;
            }

            if (requirement.IntCount > 0 && userCount > 0)
            {
                _button.Interactable = true;
                return;
            }
        }

        _button.Interactable = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_button.Interactable)
        {
            return;
        }

        Contribute().HandleExceptions();
    }

    private async Task Contribute()
    {
        var hideout = Singleton<HideoutClass>.Instance;

        // Get items that satisfy requirements. This doesn't check that it *fully* fulfills requirements
        List<HideoutItem> hideoutItems = hideout.method_21(_itemRequirements);

        // Do the client side delete operations
        var deleteOperations = hideout.method_22(hideoutItems);
        if (!await HipServer.Contribute(_areaData.Template.Type, hideoutItems.ToArray()))
        {
            deleteOperations.RollBack();
            return;
        }

        UpdateRequirements(hideoutItems);

        // Recalc various hideout things
        hideout.method_24();

        Plugin.WishlistExtendedForceRebuild();
    }

    private void UpdateRequirements(IEnumerable<HideoutItem> transferredItems)
    {
        foreach (var transferredItem in transferredItems)
        {
            var requirement = _itemRequirements.Single(r => r.TemplateId == transferredItem.Item.TemplateId);
            requirement.BaseCount -= transferredItem.Count;
            requirement.Retest();
        }
    }
}