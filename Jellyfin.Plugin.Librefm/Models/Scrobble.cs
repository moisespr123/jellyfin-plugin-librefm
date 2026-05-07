namespace Jellyfin.Plugin.Librefm.Models
{
    using System.Text.Json.Serialization;

    public class Scrobbles
    {
        [JsonPropertyName("@attr")]
        public ScrobbleAttributes Attributes { get; set; }
    }

    public class ScrobbleAttributes
    {
        // https://www.libre.fm/api
        // accepted : Number of accepted scrobbles
        [JsonPropertyName("accepted")]
        public int Accepted { get; set; }

        // https://www.libre.fm/api
        // ignored : Number of ignored scrobbles (see ignoredMessage for details)
        [JsonPropertyName("ignored")]
        public int Ignored { get; set; }
    }
}
