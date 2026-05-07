namespace Jellyfin.Plugin.Librefm.Models
{
    using System;

    public class LibrefmUser
    {
        public string Username { get; set; }

        //We wont store the password, but instead store the session key since its a lifetime key
        public string SessionKey { get; set; }

        public Guid MediaBrowserUserId { get; set; }

        public LibreFmUserOptions Options { get; set; }
    }

    public class LibreFmUserOptions
    {
        public bool Scrobble        { get; set; }
        public bool AlternativeMode { get; set; }
    }
}
