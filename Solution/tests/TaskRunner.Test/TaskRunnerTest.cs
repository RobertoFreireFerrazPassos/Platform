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
        [Fact]
        public async Task When_RunningTaskRunner_Must_ReturnResultCorrectly()
        {
            // Arrange            
            var javascriptCode = @"
                var c = input.x.field.innerField;
                output.a = c + input.y[0] + input.y[1];
                output.b = input.y.map((e,i) => c * e );
                ";

            var args = new object[] {
                        new {
                            x = new { field = new { innerField = 2 } },
                            y = new[] { 1, 4 }
                        }
                    };

            var expectedResult = JsonSerializer.SerializeToElement(
                new {
                        a = 7,
                        b = new[] { 2, 8 }
                    }
                );

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var result = await JsRunner.RunAsync(javascriptCode,args, cancellationTokenSource.Token);

            // Assert
            result.Should().NotBeNull();
            result.ToString().Should().BeEquivalentTo(expectedResult.ToString());

            cancellationTokenSource.Dispose();
        }

        [Fact]
        public async Task When_RunningTaskRunner_With_ErrorInJavascript_Must_ThrowError()
        {
            // Arrange            
            var javascriptCode = @"
                a.push();
                ";

            var expectedExceptionMessage = "a is not defined";

            var args = new object[] { 1 };

            var cancellationTokenSource = new CancellationTokenSource();

            // Act
            var action = async () => await JsRunner.RunAsync(javascriptCode, args, cancellationTokenSource.Token);

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
            var javascriptCode = @"
                output = 10;
                ";

            var args = new object[] { };

            var expectedExceptionMessage = "A task was canceled.";

            var cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.Cancel();

            // Act
            var action = async () => await JsRunner.RunAsync(javascriptCode, args, cancellationTokenSource.Token);

            // Assert
            action.Should()
                .ThrowAsync<Exception>()
                .WithMessage(expectedExceptionMessage)
                .Wait();

            cancellationTokenSource.Dispose();
        }
    }
}