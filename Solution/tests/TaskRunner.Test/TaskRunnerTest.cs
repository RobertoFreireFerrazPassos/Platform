using Xunit;
using Jering.Javascript.NodeJS;
using FluentAssertions;
using System.Threading.Tasks;
using System.Text.Json;
using System;

namespace jsTaskRunner.Test
{
    public static class TaskRunner
    {
        public static async Task<object?> RunAsync(string javascriptCode, object?[]? args)
        {
            string javascriptModule = @"
                module.exports = (callback, input) => {
                    var output = {};
                    "
                    + javascriptCode + 
                    @"
                    callback(null, output);
                }";

            return await StaticNodeJSService.InvokeFromStringAsync<object>(javascriptModule, args: args);
        }
    }

    public class TaskRunnerTest
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
            var result = await TaskRunner.RunAsync(javascriptCode,args);

            // Assert
            result.Should().NotBeNull();
            result.ToString().Should().BeEquivalentTo(expectedResult.ToString());
        }

        [Fact]
        public async Task When_RunningTaskRunner_With_ErrorInJavascript_Must_ThrowError()
        {
            // Arrange            
            string javascriptCode = @"
                a.push();
                "; // a is not defined

            var args = new object[] { 1 };

            // Act
            var action = async () => await TaskRunner.RunAsync(javascriptCode, args);

            // Assert
            action.Should()
                .ThrowAsync<Exception>()
                .Wait(); 
        }
    }
}