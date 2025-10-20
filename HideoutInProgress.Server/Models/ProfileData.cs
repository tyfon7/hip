using System.Collections.Generic;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums.Hideout;

namespace HideoutInProgress.Server;

public record ProfileData
{
    [JsonPropertyName("areaProgresses")]
    public Dictionary<HideoutAreas, Dictionary<string, int>> AreaProgresses { get; set; } = [];
}
