using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Text.Json;
using TaskRunner;
using System.Threading;
using System;

namespace jsTaskRunner.Test
{
    public class JsRunnerTest
    {
        private readonly IJsRunner _sut;

        public JsRunnerTest()
        {
            _sut = new JsRunner();
        }

        [Fact]
        public async Task When_RunningTaskRunner_Must_ReturnResultCorrectly()
        {
            // Arrange
            var expectedResult = JsonSerializer.SerializeToElement(
                new
                {
                    a = 7,
                    b = new[] { 2, 8 }
                }
                );

            var cancellationTokenSource = new CancellationTokenSource();

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = @"
                    var c = input.x.field.innerField;
                    output.a = c + input.y[0] + input.y[1];
                    output.b = input.y.map((e,i) => c * e );
                ",
                JavascriptCodeIdentifier = "CorrectJavascript",
                Args = new object[] {
                        new {
                            x = new { field = new { innerField = 2 } },
                            y = new[] { 1, 4 }
                        }
                    },
                CancellationToken = cancellationTokenSource.Token
            };

            // Act
            var result = await _sut.RunAsync(jsRunnerParams);

            // Assert
            result.Should().NotBeNull();
            result.ToString().Should().BeEquivalentTo(expectedResult.ToString());

            cancellationTokenSource.Dispose();
        }

        [Fact]
        public async Task When_RunningTaskRunner_With_ErrorInJavascript_Must_ThrowError()
        {
            // Arrange   
            var expectedExceptionMessage = "a is not defined";

            var cancellationTokenSource = new CancellationTokenSource();

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = @"
                    a.push();
                ",
                JavascriptCodeIdentifier = "ErrorInJavascript",
                Args = new object[] { 1 },
                CancellationToken = cancellationTokenSource.Token
            };

            // Act
            var action = async () => await _sut.RunAsync(jsRunnerParams);

            // Assert
            action.Should()
                .ThrowAsync<JsRunnerException>()
                .WithMessage(expectedExceptionMessage)
                .Wait();

            cancellationTokenSource.Dispose();
        }

        [Fact]
        public async Task When_CancelRunningTaskRunner_Must_ThrowError()
        {
            // Arrange  
            var expectedExceptionMessage = "A task was canceled.";

            var cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.Cancel();

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = @"
                    output = 10;
                ",
                JavascriptCodeIdentifier = "TaskCanceledJavascript",
                Args = new object[] { },
                CancellationToken = cancellationTokenSource.Token
            };

            // Act
            var action = async () => await _sut.RunAsync(jsRunnerParams);

            // Assert
            action.Should()
                .ThrowAsync<Exception>()
                .WithMessage(expectedExceptionMessage)
                .Wait();

            cancellationTokenSource.Dispose();
        }

        [Fact]
        public async Task When_RunningTaskRunnerTakesTooLong_Must_Cancel()
        {
            // Arrange  
            var expectedExceptionMessage = "The operation was canceled.";

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = @"
                    function sleep(milliseconds) {
                      const date = Date.now();
                      let currentDate = null;
                      do {
                        currentDate = Date.now();
                      } while (currentDate - date < milliseconds);
                    }
                    sleep(4000);
                    output = 10;
                ",
                JavascriptCodeIdentifier = "OperationCanceledJavascript",
                Args = new object[] { },
                CancellationToken = cancellationTokenSource.Token
            };

            // Act
            var action = async () => await _sut.RunAsync(jsRunnerParams);

            // Assert
            action.Should()
                .ThrowAsync<Exception>()
                .WithMessage(expectedExceptionMessage)
                .Wait();

            cancellationTokenSource.Dispose();
        }

        [Fact]
        public async Task When_RunningTaskRunner_Must_BeAbleToRunJavaScriptFile()
        {
            // Arrange
            var expectedResult = JsonSerializer.SerializeToElement(
                    new
                    {
                        result = "chocolate"
                    }
                );

            var cancellationTokenSource = new CancellationTokenSource();

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = @"
                    var flavours = require('./test.js');
                    output.result = flavours[0]
                ",
                Args = new object[] {},
                CancellationToken = cancellationTokenSource.Token
            };

            // Act
            var result = await _sut.RunAsync(jsRunnerParams);

            // Assert
            result.Should().NotBeNull();
            result.ToString().Should().Be(expectedResult.ToString());

            cancellationTokenSource.Dispose();
        }
    }
}