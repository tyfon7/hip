using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Utils;

namespace HideoutInProgress.Server;

public record ContributionRequestData : IRequestData
{
    [JsonPropertyName("area")]
    public HideoutAreas Area { get; set; }

    [JsonPropertyName("items")]
    public HideoutItem[] Items { get; set; }
}