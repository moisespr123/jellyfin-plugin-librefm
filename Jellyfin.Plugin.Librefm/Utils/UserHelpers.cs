namespace Jellyfin.Plugin.Librefm.Utils
{
    using Jellyfin.Database.Implementations.Entities;
    using Models;
    using System;
    using System.Linq;

    public static class UserHelpers
    {
        public static LibrefmUser GetUser(User user)
        {
            if (user == null)
                return null;

            if (Plugin.Instance.PluginConfiguration.LibrefmUsers == null)
                return null;

            return GetUser(user.Id);
        }

        public static LibrefmUser GetUser(Guid userId)
        {
            return Plugin.Instance.PluginConfiguration.LibrefmUsers.FirstOrDefault(u => u.MediaBrowserUserId.Equals(userId));
        }

        public static LibrefmUser GetUser(string userGuid)
        {
            Guid g;
            if (Guid.TryParse(userGuid, out g))
                return GetUser(g);

            return null;
        }
    }
}
