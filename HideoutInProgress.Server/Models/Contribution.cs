using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Common;

namespace HideoutInProgress.Server;

public record Contribution
{
    [JsonPropertyName("tpl")]
    public MongoId TemplateId { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}