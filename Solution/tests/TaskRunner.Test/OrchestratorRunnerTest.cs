using Client;
using FluentAssertions;
using Moq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace TaskRunner.Test
{
    public class HttpOrchestratorRunnerTest
    {
        private readonly IOrchestratorRunner _sut;

        private readonly Mock<IClientHttp> _client = new Mock<IClientHttp>();

        private readonly Mock<IJsRunner> _jsRunner = new Mock<IJsRunner>();

        public HttpOrchestratorRunnerTest() 
        {
            _sut = new HttpOrchestratorRunner(_client.Object, _jsRunner.Object);
        }

        [Fact]
        public async Task When_RunningHttpOrchestratorRunner_Must_GetCorrectResult()
        {
            var parameters = new HttpOrchestratorRunnerParams()
            {
                JavascriptCode = @"
                    output = input.response.userId;
                ",
                Uri = "https://jsonplaceholder.typicode.com/posts/2"
            };

            var responseContent = "{\"userId\":99,\"id\":2}";

            var clientResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent)
            };

            var expectedResult = 99 as object;

            _client.Setup(c => c.GetAsync(It.IsAny<string>()))
               .Returns(Task.FromResult(clientResponse))
               .Verifiable();

            _jsRunner.Setup(c => c.RunAsync(It.IsAny<JsRunnerParams>()))
               .Returns(Task.FromResult(expectedResult))
               .Verifiable();

            // Act
            var result = _sut.Run(parameters);

            // Assert
            result.Should().Be(99);

            _client.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);

            _jsRunner.Verify(j => j.RunAsync(It.IsAny<JsRunnerParams>()), Times.Once);
        }
    }
}
