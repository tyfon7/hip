import type { DependencyContainer } from "tsyringe";

import type { ProfileHelper } from "@spt/helpers/ProfileHelper";
import type { IPostSptLoadMod } from "@spt/models/external/IPostSptLoadMod";
import type { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod";
import { LogTextColor } from "@spt/models/spt/logging/LogTextColor";
import type { ILogger } from "@spt/models/spt/utils/ILogger";
import type { ICloner } from "@spt/utils/cloners/ICloner";

class Hip implements IPreSptLoadMod, IPostSptLoadMod {
    private logger: ILogger;
    private profileHelper: ProfileHelper;

    public preSptLoad(container: DependencyContainer): void {
        this.logger = container.resolve<ILogger>("PrimaryLogger");
    }

    public postSptLoad(container: DependencyContainer): void {
        this.profileHelper = container.resolve<ProfileHelper>("ProfileHelper");
    }
}

export const mod = new Hip();
