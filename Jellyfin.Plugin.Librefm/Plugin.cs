namespace Jellyfin.Plugin.Librefm
{
    using System;
    using System.Collections.Generic;
    using Configuration;
    using MediaBrowser.Common.Configuration;
    using MediaBrowser.Common.Plugins;
    using MediaBrowser.Model.Plugins;
    using MediaBrowser.Model.Serialization;


    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        public PluginConfiguration PluginConfiguration => Configuration;

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public override Guid Id { get; } = new Guid("b78cf106-4cf0-4147-807a-1846f8745f19");

        public override string Name
            => "Libre.fm";

        public override string Description
            => "Scrobble your music collection to Libre.fm";

        public static Plugin Instance { get; private set; }

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "librefm",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
                }
            };
        }
    }
}
