namespace Jellyfin.Plugin.Librefm.Api
{
    using MediaBrowser.Controller.Entities.Audio;
    using Models;
    using Models.Requests;
    using Models.Responses;
    using Resources;
    using System;
    using Microsoft.Extensions.Caching.Memory;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Utils;
    using Microsoft.Extensions.Logging;

    public class LibrefmApiClient : BaseLibrefmApiClient
    {
        private readonly ILogger _logger;

        private static readonly TimeSpan DuplicateScrobbleTTL = TimeSpan.FromSeconds(15);
        private readonly MemoryCache _scrobbleCache = new(new MemoryCacheOptions());
        private readonly object _scrobbleLock = new();

        public LibrefmApiClient(IHttpClientFactory httpClientFactory, ILogger logger) : base(httpClientFactory, logger)
        {
            _logger = logger;
        }



        public async Task<MobileSessionResponse> RequestSession(string username, string password)
        {
            //Build request object
            var request = new MobileSessionRequest
            {
                Username = username,
                Password = password,

                ApiKey = Strings.Keys.LibrefmApiKey,
                Method = Strings.Methods.GetMobileSession,
                Secure = true
            };

            var response = await Post<MobileSessionRequest, MobileSessionResponse>(request);

            if (ShouldRetryWithLegacyAuthToken(response))
            {
                _logger.LogInformation("Retrying mobile session auth with legacy authToken flow for host={Host}", Plugin.Instance?.PluginConfiguration?.LibrefmApiHost);

                request.Password = null;
                request.AuthToken = BuildLegacyAuthToken(username, password);
                response = await Post<MobileSessionRequest, MobileSessionResponse>(request);
            }


            return response;
        }

        private static string BuildLegacyAuthToken(string username, string password)
        {
            var passwordHash = Helpers.CreateMd5Hash(password).ToLowerInvariant();
            return Helpers.CreateMd5Hash(username + passwordHash).ToLowerInvariant();
        }

        private static bool ShouldRetryWithLegacyAuthToken(MobileSessionResponse response)
        {
            if (response == null || !response.IsError())
            {
                return false;
            }

            var configuredHost = Plugin.Instance?.PluginConfiguration?.LibrefmApiHost;
            var isLibreHost = !string.IsNullOrWhiteSpace(configuredHost) && configuredHost.Contains("libre.fm", StringComparison.OrdinalIgnoreCase);
            if (!isLibreHost)
            {
                return false;
            }

            if (response.ErrorCode == 6)
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(response.Message)
                && response.Message.Contains("missing a required parameter", StringComparison.OrdinalIgnoreCase);
        }

        public async Task Scrobble(Audio item, LibrefmUser user)
        {
            if (CheckAndUpdateScrobbleCache(user.Username, item.Id.ToString()))
            {
                return;
            }

            // API docs -> https://www.libre.fm/api
            var request = new ScrobbleRequest
            {
                Track = item.Name,
                Artist = item.Artists.First(),
                Timestamp = Helpers.CurrentTimestamp(),

                ApiKey = Strings.Keys.LibrefmApiKey,
                Method = Strings.Methods.Scrobble,
                SessionKey = user.SessionKey,
                Secure = true
            };

            if (!string.IsNullOrWhiteSpace(item.Album))
            {
                request.Album = item.Album;
            }
            if (item.ProviderIds.ContainsKey("MusicBrainzTrack"))
            {
                request.MbId = item.ProviderIds["MusicBrainzTrack"];
            }
            var albumArtist = item.AlbumArtists.First();
            if (!string.IsNullOrWhiteSpace(albumArtist) && albumArtist != request.Artist)
            {
                request.AlbumArtist = albumArtist;
            }

            try
            {
                _logger.LogInformation("Submitting scrobble: user={User}, artist={Artist}, track={Track}, album={Album}, timestamp={Timestamp}", user.Username, request.Artist, request.Track, request.Album, request.Timestamp);

                // Send the request
                var response = await Post<ScrobbleRequest, ScrobbleResponse>(request);
                if (response != null && !response.IsError())
                {
                    _logger.LogInformation("Scrobble succeeded: user={User}, artist={Artist}, track={Track}, album={Album}", user.Username, request.Artist, request.Track, request.Album);
                    return;
                }

                if (response == null)
                {
                    _logger.LogError("Scrobble failed with null response: user={User}, artist={Artist}, track={Track}, album={Album}", user.Username, request.Artist, request.Track, request.Album);
                    return;
                }

                _logger.LogError("Scrobble failed: user={User}, artist={Artist}, track={Track}, album={Album}, errorCode={ErrorCode}, message={Message}", user.Username, request.Artist, request.Track, request.Album, response.ErrorCode, response.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Scrobble exception: ex={0}, user={1}, name={2}, track={3}, artist={4}, album={5}, albumArtist={6}, mbid={7}", ex, user.Username, item.Name, request.Track, request.Artist, request.Album, request.AlbumArtist, request.MbId);
            }
        }

        public async Task NowPlaying(Audio item, LibrefmUser user)
        {
            var request = new NowPlayingRequest
            {
                Track = item.Name,
                Artist = item.Artists.First(),

                ApiKey = Strings.Keys.LibrefmApiKey,
                Method = Strings.Methods.NowPlaying,
                SessionKey = user.SessionKey,
                Secure = true
            };


            if (!string.IsNullOrWhiteSpace(item.Album))
            {
                request.Album = item.Album;
            }
            if (item.ProviderIds.ContainsKey("MusicBrainzTrack"))
            {
                request.MbId = item.ProviderIds["MusicBrainzTrack"];
            }
            var albumArtist = item.AlbumArtists.First();
            if (!string.IsNullOrWhiteSpace(albumArtist) && albumArtist != request.Artist)
            {
                request.AlbumArtist = albumArtist;
            }

            // Add duration
            if (item.RunTimeTicks != null)
                request.Duration = Convert.ToInt32(TimeSpan.FromTicks((long)item.RunTimeTicks).TotalSeconds);

            try
            {
                var response = await Post<NowPlayingRequest, ScrobbleResponse>(request);
                if (response != null && !response.IsError())
                {
                    _logger.LogInformation("{0} is now playing artist={1}, track={2}, album={3}", user.Username, request.Artist, request.Track, request.Album);
                    return;
                }

                _logger.LogError("Failed to send now playing for track: {0}", item.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to send now playing for track: ex={0}, name={1}, track={2}, artist={3}, album={4}, albumArtist={5}, mbid={6}", ex, item.Name, request.Track, request.Artist, request.Album, request.AlbumArtist, request.MbId);
            }
        }


        /// <summary>
        /// Checks for duplicate scrobble and updates cache if not duplicate.
        /// Even though MemoryCache is thread-safe, we use the _scrobbleLock to ensure thread safety for the whole check-and-set operation.
        /// The method also updates the cache with the new scrobble if it's not a duplicate.
        /// Returns true if duplicate, false otherwise.
        /// </summary>
        private bool CheckAndUpdateScrobbleCache(string username, string trackId)
        {
            var cacheKey = $"{username}:{trackId}";
            lock (_scrobbleLock)
            {
                if (_scrobbleCache.TryGetValue(cacheKey, out _))
                {
                    _logger.LogInformation("Duplicate scrobble detected for user={0}, trackId={1} within {2} seconds. Skipping.", username, trackId, DuplicateScrobbleTTL.TotalSeconds);
                    return true;
                }
                _scrobbleCache.Set(cacheKey, true, DuplicateScrobbleTTL);
                return false;
            }
        }
    }
}
