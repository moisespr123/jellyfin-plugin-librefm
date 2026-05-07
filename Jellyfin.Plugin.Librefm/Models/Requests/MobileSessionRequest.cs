namespace Jellyfin.Plugin.Librefm.Models.Requests
{
    using System.Collections.Generic;

    public class MobileSessionRequest : BaseRequest
    {
        public string Password { get; set; }
        public string AuthToken { get; set; }
        public string Username { get; set; }

        public override Dictionary<string, string> ToDictionary() 
        {
            var data = new Dictionary<string, string>(base.ToDictionary()) 
            {
                { "username", Username },
            };

            if (!string.IsNullOrWhiteSpace(AuthToken))
            {
                data.Add("authToken", AuthToken);
            }
            else
            {
                data.Add("password", Password);
            }

            return data;
        }
    }
}
