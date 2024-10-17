using System.Net;
using System.Net.Http;
using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerProxies.FlareSolverr;

namespace NzbDrone.Core.Test.IndexerProxyTests
{
    [TestFixture]
    public class FlareSolverrSessionServiceTests
    {
        private Mock<IHttpClient> _httpClientMock;
        private Mock<Logger> _loggerMock;
        private FlareSolverrSessionService _sessionService;

        public FlareSolverrSessionServiceTests()
        {
            _httpClientMock = new Mock<IHttpClient>();
            _loggerMock = new Mock<Logger>();
            _sessionService = new FlareSolverrSessionService(_httpClientMock.Object, _loggerMock.Object);
        }

        [SetUp]
        public void SetUp()
        {
            // Reinitialize mocks for each test
            _httpClientMock = new Mock<IHttpClient>();
            _loggerMock = new Mock<Logger>();
            _sessionService = new FlareSolverrSessionService(_httpClientMock.Object, _loggerMock.Object);
        }

        [Test]
        public void DeleteSession_Should_Delete_When_Session_Exists()
        {
            var settings = new FlareSolverrSettings { Host = "http://example.com", RequestTimeout = 5 };
            var sessionListResponse = new HttpResponse(new HttpRequest("http://example.com", HttpAccept.Json), new HttpHeader(), new CookieCollection(), "{\"Sessions\": [\"DEFAULT_SESSION\"]}");
            _httpClientMock.Setup(client => client.Execute(It.IsAny<HttpRequest>()))
                .Returns(sessionListResponse);

            _sessionService.DeleteSession(settings, FlareSolverrSessionService.DefaultSession);

            _httpClientMock.Verify<HttpResponse>(client => client.Execute(It.Is<HttpRequest>(r =>
                r.GetContent().Contains("\"sessions.destroy\"") &&
                r.GetContent().Contains(FlareSolverrSessionService.DefaultSession))), Times.Once);
        }

        [Test]
        public void DeleteSession_Should_Not_Delete_When_Session_Does_Not_Exist()
        {
            var settings = new FlareSolverrSettings { Host = "http://example.com", RequestTimeout = 5 };
            var sessionListResponse = new HttpResponse(
                request: new HttpRequest("http://example.com/v1"),
                headers: new HttpHeader(),
                cookies: new CookieCollection(),
                content: "{\"Sessions\": [\"OTHER_SESSION\"]}",
                statusCode: HttpStatusCode.OK);

            _httpClientMock.Setup(client => client.Execute(It.IsAny<HttpRequest>()))
                .Returns(sessionListResponse);

            _sessionService.DeleteSession(settings, FlareSolverrSessionService.DefaultSession);

            _httpClientMock.Verify(client => client.Execute(It.Is<HttpRequest>(r =>
                r.GetContent().Contains("\"sessions.destroy\""))), Times.Never);
        }

        [Test]
        public void ListSessions_Should_Return_Parsed_Sessions()
        {
            var settings = new FlareSolverrSettings { Host = "http://example.com", RequestTimeout = 5 };
            var httpRequest = new HttpRequest("http://example.com/v1")
            {
                Method = HttpMethod.Post
            };
            var expectedResponse = new HttpResponse(
                request: httpRequest,
                headers: new HttpHeader(),
                cookies: new CookieCollection(),
                content: "{\"Sessions\": [\"session1\", \"session2\"]}",
                statusCode: HttpStatusCode.OK);

            _httpClientMock.Setup(client => client.Execute(It.IsAny<HttpRequest>())).Returns(expectedResponse);

            var result = _sessionService.ListSessions(settings);

            Assert.NotNull(result);
            Assert.AreEqual(2, result.Sessions.Count);
            Assert.Contains("session1", result.Sessions);
            Assert.Contains("session2", result.Sessions);
        }

        [Test]
        public void ListSessions_Should_Return_Empty_When_No_Sessions()
        {
            var settings = new FlareSolverrSettings { Host = "http://example.com", RequestTimeout = 5 };
            var httpRequest = new HttpRequest("http://example.com/v1")
            {
                Method = HttpMethod.Post
            };
            var emptyResponse = new HttpResponse(
                request: httpRequest,
                headers: new HttpHeader(),
                cookies: new CookieCollection(),
                content: "{\"Sessions\": []}",
                statusCode: HttpStatusCode.OK);

            _httpClientMock.Setup(client => client.Execute(It.IsAny<HttpRequest>())).Returns(emptyResponse);

            var result = _sessionService.ListSessions(settings);

            Assert.NotNull(result);
            Assert.IsEmpty(result.Sessions);
        }
    }
}
