# Hideout In Progress

Contribute resources to hideout upgrades as you get them.

This mod for SPT adds a "Transfer Items" button to hideout construction/upgrade windows. Clicking this button will contribute any items in your stash towards the upgrade, and the UI will update to reflect the remaining resources required.

_I announced this mod and 6 hours later BSG announced their plans to implement this feature. Coincidence? You decide. Until they actually implement it, use this._

Some things to keep in mind:

-   Cash is not contributed early, and will be payed in full when you finally start the upgrade
-   Items contributed cannot be retrieved

### Technical details

Contributions are stored in the user profile, in `characters.pmc.Hideout.Areas[area].contributions`. If you uninstall this mod, they will remain in your profile but will not be used by anything, and the items will not only be lost, but will not contribute towards the construction.
