namespace Jellyfin.Plugin.Librefm.Models.Responses
{
    using System.Text.Json.Serialization;

    public class ScrobbleResponse : BaseResponse
    {
        [JsonPropertyName("scrobbles")]
        public Scrobbles Scrobbles { get; set; }
    }
}
