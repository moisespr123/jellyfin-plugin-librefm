namespace Jellyfin.Plugin.Librefm.Models.Requests
{
    using System.Collections.Generic;

    public class MobileSessionRequest : BaseRequest
    {
        public string AuthToken { get; set; }
        public string Username { get; set; }

        public override Dictionary<string, string> ToDictionary() 
        {
            var data = new Dictionary<string, string>(base.ToDictionary()) 
            {
                { "username", Username },
                { "authToken", AuthToken },
            };

            return data;
        }
    }
}
