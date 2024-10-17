using System.Collections.Generic;
using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerProxies;
using NzbDrone.Core.IndexerProxies.FlareSolverr;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Tests.IndexerProxies.FlareSolverr
{
    [TestFixture]
    public class FlareSolverrSettingsHandlerTests
    {
        private Mock<IHttpClient> _httpClientMock;
        private Mock<Logger> _loggerMock;
        private Mock<FlareSolverrSessionService> _sessionServiceMock;
        private FlareSolverrSettingsHandler _handler;

        [SetUp]
        public void SetUp()
        {
            _httpClientMock = new Mock<IHttpClient>();
            _loggerMock = new Mock<Logger>();
            _sessionServiceMock = new Mock<FlareSolverrSessionService>(_httpClientMock.Object, _loggerMock.Object);
            _handler = new FlareSolverrSettingsHandler(_sessionServiceMock.Object);
        }

        [Test]
        public void
            Handle_ProviderUpdatedEvent_ShouldDeleteSession_WhenSettingsAreFlareSolverrAndSessionShouldBeDeleted()
        {
            // Arrange
            var providerDefinition = new IndexerProxyDefinition
            {
                Settings = new FlareSolverrSettings
                {
                    SessionEnabled = false,
                    Host = "http://localhost", // Ensure Host is set for ListSessions
                    RequestTimeout = 5 // Set a default timeout
                },
                ConfigContract = "FlareSolverrSettings"
            };

            // Mock the ListSessions method
            _sessionServiceMock
                .Setup(s => s.ListSessions(It.IsAny<FlareSolverrSettings>()))
                .Returns(new FlareSoverrSessions
                {
                    Sessions = new List<string>() { FlareSolverrSessionService.DefaultSession }
                });

            var message = new ProviderUpdatedEvent<IIndexerProxy>(providerDefinition);

            // Act
            _handler.Handle(message);

            // Assert
            _sessionServiceMock.Verify(
                s => s.DeleteSession(It.IsAny<FlareSolverrSettings>(), FlareSolverrSessionService.DefaultSession),
                Times.Once);
        }

        [Test]
        public void Handle_ProviderUpdatedEvent_ShouldNotDeleteSession_WhenSessionIsEnabled()
        {
            // Arrange
            var providerDefinition = new IndexerProxyDefinition
            {
                Settings = new FlareSolverrSettings
                {
                    SessionEnabled = true,
                    Host = "http://localhost", // Ensure Host is set for ListSessions
                    RequestTimeout = 5 // Set a default timeout
                },
                ConfigContract = "FlareSolverrSettings"
            };

            // Mock the ListSessions method
            _sessionServiceMock
                .Setup(s => s.ListSessions(It.IsAny<FlareSolverrSettings>()))
                .Returns(new FlareSoverrSessions
                {
                    Sessions = new List<string> { FlareSolverrSessionService.DefaultSession }
                });

            var message = new ProviderUpdatedEvent<IIndexerProxy>(providerDefinition);

            // Act
            _handler.Handle(message);

            // Assert
            _sessionServiceMock.Verify(s => s.DeleteSession(It.IsAny<FlareSolverrSettings>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public void Handle_ProviderDeletedEvent_ShouldDeleteSession_WhenSettingsAreFlareSolverrAndSessionIsEnabled()
        {
            // Arrange
            var providerDefinition = new IndexerProxyDefinition
            {
                Settings = new FlareSolverrSettings
                {
                    SessionEnabled = true,
                    Host = "http://localhost", // Ensure Host is set for ListSessions
                    RequestTimeout = 5 // Set a default timeout
                },
                ConfigContract = "FlareSolverrSettings"
            };

            // Mock the ListSessions method
            _sessionServiceMock
                .Setup(s => s.ListSessions(It.IsAny<FlareSolverrSettings>()))
                .Returns(new FlareSoverrSessions
                {
                    Sessions = new List<string> { FlareSolverrSessionService.DefaultSession }
                });

            var message = new ProviderDeletedEvent<IIndexerProxy>(1, providerDefinition);

            // Act
            _handler.Handle(message);

            // Assert
            _sessionServiceMock.Verify(
                s => s.DeleteSession(It.IsAny<FlareSolverrSettings>(), FlareSolverrSessionService.DefaultSession),
                Times.Once);
        }

        [Test]
        public void Handle_ProviderDeletedEvent_ShouldNotDeleteSession_WhenSessionIsDisabled()
        {
            // Arrange
            var providerDefinition = new IndexerProxyDefinition
            {
                Settings = new FlareSolverrSettings
                {
                    SessionEnabled = false,
                    Host = "http://localhost", // Ensure Host is set for ListSessions
                    RequestTimeout = 5 // Set a default timeout
                },
                ConfigContract = "FlareSolverrSettings"
            };

            // Mock the ListSessions method
            _sessionServiceMock
                .Setup(s => s.ListSessions(It.IsAny<FlareSolverrSettings>()))
                .Returns(new FlareSoverrSessions
                {
                    Sessions = new List<string> { FlareSolverrSessionService.DefaultSession }
                });

            var message = new ProviderDeletedEvent<IIndexerProxy>(1, providerDefinition);

            // Act
            _handler.Handle(message);

            // Assert
            _sessionServiceMock.Verify(s => s.DeleteSession(It.IsAny<FlareSolverrSettings>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public void Handle_ProviderUpdatedEvent_ShouldNotDeleteSession_WhenNotFlareSolverrSettings()
        {
            var providerDefinition = new IndexerProxyDefinition
            {
                Settings = new FlareSolverrSettings
                {
                    SessionEnabled = false,
                    Host = "http://localhost",
                    RequestTimeout = 5
                },
                ConfigContract = "OtherSettings"
            };

            _sessionServiceMock
                .Setup(s => s.ListSessions(It.IsAny<FlareSolverrSettings>()))
                .Returns(new FlareSoverrSessions
                {
                    Sessions = new List<string> { FlareSolverrSessionService.DefaultSession }
                });

            var message = new ProviderUpdatedEvent<IIndexerProxy>(providerDefinition);

            _handler.Handle(message);

            _sessionServiceMock.Verify(s => s.DeleteSession(It.IsAny<FlareSolverrSettings>(), It.IsAny<string>()),
                Times.Never);
        }

        [Test]
        public void Handle_ProviderDeletedEvent_ShouldNotDeleteSession_WhenNotFlareSolverrSettings()
        {
            var providerDefinition = new IndexerProxyDefinition
            {
                Settings = new FlareSolverrSettings
                {
                    SessionEnabled = true,
                    Host = "http://localhost",
                    RequestTimeout = 5
                },
                ConfigContract = "OtherSettings"
            };

            _sessionServiceMock
                .Setup(s => s.ListSessions(It.IsAny<FlareSolverrSettings>()))
                .Returns(new FlareSoverrSessions
                {
                    Sessions = new List<string> { FlareSolverrSessionService.DefaultSession }
                });

            var message = new ProviderDeletedEvent<IIndexerProxy>(1, providerDefinition);

            _handler.Handle(message);

            _sessionServiceMock.Verify(s => s.DeleteSession(It.IsAny<FlareSolverrSettings>(), It.IsAny<string>()),
                Times.Never);
        }
    }
}
