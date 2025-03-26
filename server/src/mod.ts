import type { DependencyContainer } from "tsyringe";

import type { HideoutController } from "@spt/controllers/HideoutController";
import type { InventoryHelper } from "@spt/helpers/InventoryHelper";
import type { ProfileHelper } from "@spt/helpers/ProfileHelper";
import type { IBotHideoutArea } from "@spt/models/eft/common/tables/IBotBase";
import type { IHideoutItem } from "@spt/models/eft/hideout/IHideoutImproveAreaRequestData";
import type { HideoutAreas } from "@spt/models/enums/HideoutAreas";
import type { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod";
import type { ILogger } from "@spt/models/spt/utils/ILogger";
import type { StaticRouterModService } from "@spt/services/mod/staticRouter/StaticRouterModService";

type Contribution = {
    tpl: string;
    count: number;
};

type AreaProgress = {
    areaType: HideoutAreas;
    contributions: Contribution[];
};

interface ContributionRequest {
    areaType: HideoutAreas;
    items: IHideoutItem[];
}

type ExtendedHideoutArea = IBotHideoutArea & {
    contributions: Contribution[];
};

class Hip implements IPreSptLoadMod {
    private logger: ILogger;
    private profileHelper: ProfileHelper;
    private inventoryHelper: InventoryHelper;

    public preSptLoad(container: DependencyContainer): void {
        this.logger = container.resolve<ILogger>("PrimaryLogger");
        this.profileHelper = container.resolve<ProfileHelper>("ProfileHelper");
        this.inventoryHelper = container.resolve<InventoryHelper>("InventoryHelper");

        const staticRouterModService = container.resolve<StaticRouterModService>("StaticRouterModService");

        // Mod HideoutController.startUpgrade to clear existing contributions
        container.afterResolution(
            "HideoutController",
            (_, hideoutController: HideoutController) => {
                const original = hideoutController.startUpgrade;

                hideoutController.startUpgrade = (pmcData, request, sessionId) => {
                    const area = pmcData.Hideout.Areas.find(a => a.type == request.areaType) as ExtendedHideoutArea;
                    area.contributions = [];

                    return original.call(hideoutController, pmcData, request, sessionId);
                };
            },
            { frequency: "Always" }
        );

        staticRouterModService.registerStaticRouter(
            "HipRoutes",
            [
                {
                    url: "/hip/load",
                    action: async (url, info, sessionId, output) => {
                        try {
                            return JSON.stringify(this.getAreaProgress(sessionId));
                        } catch (error) {
                            this.logger.error(`HideoutInProgress failed to load: ${error}`);
                        }
                    }
                },
                {
                    url: "/hip/contribute",
                    action: async (url, info: ContributionRequest, sessionId, output) => {
                        try {
                            return JSON.stringify(this.contributeAreaProgress(info, sessionId));
                        } catch (error) {
                            this.logger.error(`HideoutInProgress failed to contribute: ${error}`);
                        }
                    }
                }
            ],
            "custom-static-hip"
        );
    }

    private getAreaProgress(sessionId: string): AreaProgress[] {
        const pmcData = this.profileHelper.getPmcProfile(sessionId);

        const response: AreaProgress[] = [];
        for (const area of pmcData.Hideout.Areas) {
            const extendedArea = area as ExtendedHideoutArea;
            if (extendedArea.contributions?.length > 0) {
                response.push({
                    areaType: extendedArea.type,
                    contributions: extendedArea.contributions
                });
            }
        }

        return response;
    }

    private contributeAreaProgress(info: ContributionRequest, sessionId: string) {
        const pmcData = this.profileHelper.getPmcProfile(sessionId);
        const area = pmcData.Hideout.Areas.find(a => a.type == info.areaType) as ExtendedHideoutArea;

        if (!area) {
            this.logger.error(`HideoutInProgress: Cannot find hideout area of type ${info.areaType}`);
            return { succeeded: false };
        }

        // Create mapping of required item with corrisponding item from player inventory (copied)
        const items = info.items.map(reqItem => {
            const item = pmcData.Inventory.items.find(invItem => invItem._id === reqItem.id);
            return { inventoryItem: item, requestedItem: reqItem };
        });

        for (const item of items) {
            if (!item.inventoryItem) {
                this.logger.error(`HideoutInProgress: Cannot find inventory item: ${item.requestedItem.id}`);
                continue;
            }

            // Remove the item from inventory
            this.inventoryHelper.removeItem(pmcData, item.inventoryItem._id, sessionId);

            // Remember that this was contributed
            if (!area.contributions) {
                area.contributions = [];
            }

            let contribution = area.contributions.find(c => c.tpl == item.inventoryItem._tpl);
            if (!contribution) {
                contribution = {
                    tpl: item.inventoryItem._tpl,
                    count: 0
                };
                area.contributions.push(contribution);
            }

            contribution.count += item.requestedItem.count;
        }

        return { succeeded: true };
    }
}

export const mod = new Hip();
