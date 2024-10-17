using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.IndexerProxies.FlareSolverr
{
    public class FlareSolverrSessionService
    {
        public const string DefaultSession = "DEFAULT_SESSION";

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public FlareSolverrSessionService(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public virtual void DeleteSession(FlareSolverrSettings settings, string sessionName)
        {
            var flareSoverrSessions = ListSessions(settings);
            if (flareSoverrSessions.Sessions.Contains(DefaultSession))
            {
                var request = CreateDeleteSessionRequest(settings, sessionName);
                try
                {
                    _httpClient.Execute(request);
                }
                catch (Exception ex)
                {
                    _logger.Warn("Failed to delete session: {0}", ex);
                }
            }
        }

        public virtual FlareSoverrSessions ListSessions(FlareSolverrSettings settings)
        {
            var request = CreateListSessionRequest(settings);
            var response = _httpClient.Execute(request);
            return ParseSessionList(response.Content);
        }

        private HttpRequest CreateListSessionRequest(FlareSolverrSettings settings)
        {
            var apiUrl = string.Format("{0}/v1", settings.Host.TrimEnd('/'));
            var newRequest = new HttpRequest(apiUrl, HttpAccept.Json)
            {
                Headers = { ContentType = "application/json" },
                Method = HttpMethod.Post,
                LogResponseContent = true,
                RequestTimeout = TimeSpan.FromSeconds(settings.RequestTimeout + 5)
            };
            var req = new FlareSolverrRequest { Cmd = "sessions.list" };
            newRequest.SetContent(req.ToJson());
            newRequest.ContentSummary = req.ToJson(Formatting.None);
            return newRequest;
        }

        private HttpRequest CreateDeleteSessionRequest(FlareSolverrSettings settings, string sessionName)
        {
            var apiUrl = string.Format("{0}/v1", settings.Host.TrimEnd('/'));
            var req = new FlareSolverrDestroyRequest
            {
                Cmd = "sessions.destroy",
                Session = sessionName
            };
            var newRequest = new HttpRequest(apiUrl, HttpAccept.Json)
            {
                Headers = { ContentType = "application/json" },
                Method = HttpMethod.Post,
                LogResponseContent = true,
                RequestTimeout = TimeSpan.FromSeconds(settings.RequestTimeout + 5)
            };
            newRequest.SetContent(req.ToJson());
            newRequest.ContentSummary = req.ToJson(Formatting.None);
            return newRequest;
        }

        private FlareSoverrSessions ParseSessionList(string responseContent)
        {
                return JsonConvert.DeserializeObject<FlareSoverrSessions>(responseContent)
                       ?? new FlareSoverrSessions { Sessions = new List<string>() };
        }

        private class FlareSolverrRequest
        {
            public string Cmd { get; set; }
        }

        private class FlareSolverrDestroyRequest : FlareSolverrRequest
        {
            public string Session { get; set; }
        }
    }
}
