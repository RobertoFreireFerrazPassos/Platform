using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Text.Json;
using TaskRunner;

namespace jsTaskRunner.Test
{
    public class JsRunnerTest
    {
        [Fact]
        public async Task When_RunningTaskRunner_Must_ReturnResultCorrectly()
        {
            // Arrange            
            string javascriptCode = @"
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

            // Act
            var result = await JsRunner.RunAsync(javascriptCode,args);

            // Assert
            result.Should().NotBeNull();
            result.ToString().Should().BeEquivalentTo(expectedResult.ToString());
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

            // Act
            var action = async () => await JsRunner.RunAsync(javascriptCode, args);

            // Assert
            action.Should()
                .ThrowAsync<JsRunnerException>()
                .WithMessage(expectedExceptionMessage)
                .Wait(); 
        }
    }
}