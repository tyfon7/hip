using System.Collections.Generic;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Enums.Hideout;

namespace HideoutInProgress.Server;

public record AreaProgress
{
    [JsonPropertyName("area")]
    public HideoutAreas Area { get; set; }

    [JsonPropertyName("contributions")]
    public IEnumerable<Contribution> Contributions { get; set; }
}