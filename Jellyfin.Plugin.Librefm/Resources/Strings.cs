namespace Jellyfin.Plugin.Librefm.Resources
{
    public static class Strings
    {
        public static class Endpoints
        {
            public static string LibrefmApi  = "libre.fm";
        }

        public static class Methods
        {
            // Libre.FM API specs located at https://www.libre.fm/api
            public static string Scrobble         = "track.scrobble";
            public static string NowPlaying       = "track.updateNowPlaying";
            public static string GetMobileSession = "auth.getMobileSession";
        }

        public static class Keys
        {
            public static string LibrefmApiKey     = "4a45113da515f00f2417b38b0cc47413";
            public static string LibrefmApiSecret = "51eb12dbf7d00a1eba3f7d17304fe0c8";
        }
    }
}
