namespace Jellyfin.Plugin.Librefm.Configuration
{
    using Models;
    using MediaBrowser.Model.Plugins;
    using Resources;

    /// <summary>
    /// Class PluginConfiguration
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        public LibrefmUser[] LibrefmUsers { get; set; }
        public string LibrefmApiHost { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginConfiguration" /> class.
        /// </summary>
        public PluginConfiguration()
        {
            LibrefmUsers = new LibrefmUser[] { };
            LibrefmApiHost = Strings.Endpoints.LibrefmApi;
        }
    }
}
