## jellyfin-plugin-librefm

Enables audio scrobbling to Libre.fm.

This repository is a fork of the Last.fm plugin maintained by [danielfariati](https://github.com/danielfariati/jellyfin-plugin-lastfm).

While his plugin already supports scrobbling to Last.fm, it has a limitation that it works with Last.fm or Libre.fm, but not both at the same time. Therefore, this separate plugin, when installed, will scrobble to Libre.fm.

You can have both the Last.fm and Libre.fm plugins enabled to allow scrobbling to both services at the same time. Just keep the endpoint set at the default. (that is, let the Last.fm plugin only scrobble to last.fm and let the Libre.fm plugin only scrobble to libre.fm). Additionally, there is also the [ListenBrainz plugin](https://github.com/lyarenei/jellyfin-plugin-listenbrainz) if you're interested to scrobble there too.

This plugin only scrobbles to either libre.fm or last.fm. Metadata / album art functions have been cleaned up. Continue using the Last.fm plugin if you rely in this functionality.

## 🔧 Installation and Configuration

Install the plugin via the Jellyfin plugin repository. Navigate to the **Plugins** section of the admin dashboard and add the following repository to receive stable builds of this plugin:

- **Repo name:** Libre.fm Stable  
- **Repo URL:** https://raw.githubusercontent.com/moisespr123/jellyfin-plugin-librefm/refs/heads/master/manifest.json

Restart the Jellyfin server after installation.

## 👤 Per-user Settings

The plugin is configured **per Jellyfin user**.

Select the Jellyfin user from the dropdown at the top of the configuration screen.

When configuring a user, you must provide your **Libre.fm username and password once**. The password is **not stored**.

It is used only to authenticate with Libre.fm and obtain a **session key**, which is then saved and used for all future scrobbling and API requests.

If a user changes their Libre.fm password, you may need to reconfigure the plugin for that user.

- **Enable Scrobbling for this user?**  
  Enables or disables Libre.fm scrobbling for the selected Jellyfin user.

- **Use alternative mode and scrobble on `UserDataSaved` events instead of `PlaybackStopped`?**

  By default, the plugin scrobbles tracks when Jellyfin emits the `PlaybackStopped` event. This event is reported by the client, and its timing and accuracy depend on the client implementation. Some clients may emit this event with delayed or synthetic timing, or may not emit it consistently (particularly mobile clients), which can lead to missing or inconsistent scrobbles.
  
  When **Alternative Mode** is enabled, the plugin scrobbles tracks on `UserDataSaved` events instead. These events are triggered when Jellyfin persists playback progress or marks an item as played, making scrobbling dependent on server-side playback state rather than client-reported stop events.

  **Enable Alternative Mode if:**
  - You experience missing or inconsistent scrobbles;
  - You primarily use mobile clients, or clients with unreliable stop reporting;

  **Disable it if:**
  - Your clients reliably report `PlaybackStopped` events;
  - You prefer scrobbling to be triggered by the client-reported stop event rather than by Jellyfin saving user playback data;

- **Advanced options**
  - **API host:**

    Allows you to specify a custom API host for Libre.fm-compatible services / APIs.
    For example:
    - **Libre.fm (Default):** `libre.fm`
    - **Last.fm:** `ws.audioscrobbler.com`

    If you change this value, you have to re-enter the password and save the configuration for the change to take effect, as the session key is tied to the API host.

    Please note that while the plugin may work with Libre.fm-compatible services, it is primarily designed and tested against the official Libre.fm API. Compatibility with other services may vary based on how closely they adhere to the Libre.fm API specifications.

## 🛠 Troubleshooting

- Missing scrobbles? Try enabling **Alternative Mode** (more details in the [Per-user Settings](#-per-user-settings) section)
- If authentication appears broken, re-enter your Libre.fm credentials and save to generate a new session key
- If using a custom API host (for example Libre.fm), confirm the host is correct and then re-authenticate to refresh the session key for that host
- Check Jellyfin server logs for plugin-related messages
