using Client;
using FluentAssertions;
using Moq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace TaskRunner.Test
{
    public class HttpOrchestratorRunnerTest
    {
        private readonly IOrchestratorRunner _sut;

        private readonly Mock<IClientHttp> _client = new Mock<IClientHttp>();

        private readonly IJsRunner _jsRunner = new JsRunner();

        public HttpOrchestratorRunnerTest() 
        {
            _sut = new HttpOrchestratorRunner(_client.Object, _jsRunner);
        }

        [Fact]
        public async Task When_RunningHttpOrchestratorRunner_Must_GetCorrectResult()
        {
            var parameters = new HttpGetOrchestratorRunnerParams()
            {
                JavascriptCode = @"
                    output = input.response.userId;
                ",
                JavascriptCodeIdentifier = "HttpGetOrchestratorRunnerExample",
                Uri = "https://jsonplaceholder.typicode.com/posts/2"
            };

            var responseContent = JsonSerializer.Deserialize<object>("{\"userId\":99,\"id\":2}");

            var expectedResult = 99;

            _client.Setup(c => c.GetAsync(It.IsAny<string>()))
               .Returns(Task.FromResult(responseContent))
               .Verifiable();

            // Act
            var result = _sut.Get(parameters);

            // Assert
            result.ToString().Should().Be(expectedResult.ToString());

            _client.Verify(c => c.GetAsync(It.IsAny<string>()), Times.Once);
        }
    }
}
