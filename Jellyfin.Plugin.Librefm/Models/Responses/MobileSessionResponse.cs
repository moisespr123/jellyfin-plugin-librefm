namespace Jellyfin.Plugin.Librefm.Models.Responses
{
    using System.Text.Json.Serialization;

    public class MobileSessionResponse : BaseResponse
    {
        [JsonPropertyName("session")]
        public MobileSession Session { get; set; }
    }
}
