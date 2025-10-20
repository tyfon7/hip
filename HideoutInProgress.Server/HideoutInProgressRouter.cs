using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Utils;

namespace HideoutInProgress.Server;

[Injectable]
public class HideoutInProgressRouter(JsonUtil jsonUtil, HideoutInProgressCallbacks callbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<ContributionRequestData>(
                "/hip/contribute",
                async (url, info, sessionId, output) => jsonUtil.Serialize(await callbacks.Contribute(info, sessionId))
            ),
            new RouteAction(
                "/hip/load",
                async (url, info, sessionId, output) => jsonUtil.Serialize(await callbacks.GetAreaProgresses(sessionId))
            )
        ]
    )
{ }
