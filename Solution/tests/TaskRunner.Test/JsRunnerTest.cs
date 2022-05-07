using Xunit;
using FluentAssertions;
using System.Threading.Tasks;
using System.Text.Json;
using TaskRunner;

namespace jsTaskRunner.Test
{
    public class JsRunnerTest
    {
        private readonly IJsRunner _sut;

        private readonly int _timeOut = 3;

        private string EncapsulteJavascriptCodeInModule(string javascriptCode)
        {
            return @"
                module.exports = (callback, input) => {
                    var output = {};
                    "
                    + javascriptCode +
                    @"
                    callback(null, output);
                }";
        }

        public JsRunnerTest()
        {
            _sut = new JsRunner(_timeOut);
        }

        [Fact]
        public async Task Must_ReturnResultCorrectly()
        {
            // Arrange
            var expectedResult = JsonSerializer.SerializeToElement(
                new
                    {
                        a = 7,
                        b = new[] { 2, 8 }
                    }
                );

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = EncapsulteJavascriptCodeInModule(@"
                    var c = input.x.field.innerField;
                    output.a = c + input.y[0] + input.y[1];
                    output.b = input.y.map((e,i) => c * e );
                "),
                JavascriptCodeIdentifier = "CorrectJavascript",
                Args = new object[] {
                        new {
                            x = new { field = new { innerField = 2 } },
                            y = new[] { 1, 4 }
                        }
                    }
            };

            // Act
            var result = await _sut.RunAsync(jsRunnerParams);

            // Assert
            result.Should().NotBeNull();

            result.ToString().Should().BeEquivalentTo(expectedResult.ToString());
        }

        [Fact]
        public async Task Given_ErrorInJavascript_Must_ThrowError()
        {
            // Arrange   
            var expectedExceptionMessage = "a is not defined";

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = EncapsulteJavascriptCodeInModule(@"
                    a.push();
                "),
                JavascriptCodeIdentifier = "ErrorInJavascript",
                Args = new object[] { 1 }
            };

            // Act
            var action = async () => await _sut.RunAsync(jsRunnerParams);

            // Assert
            action.Should()
                .ThrowAsync<JsRunnerException>()
                .WithMessage(expectedExceptionMessage)
                .Wait();
        }

        [Fact]
        public async Task When_TakesTooLong_Must_Cancel()
        {
            // Arrange
            var expectedExceptionMessage = $"The operation was canceled. It took more than {_timeOut} seconds";

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = EncapsulteJavascriptCodeInModule(@"
                    function sleep(milliseconds) {
                      const date = Date.now();
                      let currentDate = null;
                      do {
                        currentDate = Date.now();
                      } while (currentDate - date < milliseconds);
                    }
                    sleep(4000);
                    output = 10;
                "),
                JavascriptCodeIdentifier = "OperationCanceledJavascript",
                Args = new object[] { }
            };

            // Act
            var action = async () => await _sut.RunAsync(jsRunnerParams);

            // Assert
            action.Should()
                .ThrowAsync<JsRunnerException>()
                .WithMessage(expectedExceptionMessage)
                .Wait();
        }

        [Fact]
        public async Task Must_BeAbleToRunJavaScriptFile()
        {
            // Arrange
            var expectedResult = JsonSerializer.SerializeToElement(
                    new
                    {
                        result = "chocolate"
                    }
                );

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = EncapsulteJavascriptCodeInModule(@"
                    var flavours = require('./test.js');
                    output.result = flavours[0]
                "),
                Args = new object[] {},
                JavascriptCodeIdentifier = "JavascriptFileExample"
            };

            // Act
            var result = await _sut.RunAsync(jsRunnerParams);

            // Assert
            result.Should().NotBeNull();

            result.ToString().Should().Be(expectedResult.ToString());
        }

        [Fact]
        public async Task Must_BeAbleToMakeHTTPCallsAsync()
        {
            // Arrange
            var expectedResult = "{\"userId\":1,\"id\":2,\"title\":\"qui est esse\",\"body\":\"est rerum tempore vitae\\nsequi sint nihil reprehenderit dolor beatae ea dolores neque\\nfugiat blanditiis voluptate porro vel nihil molestiae ut reiciendis\\nqui aperiam non debitis possimus qui neque nisi nulla\"}";

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = @"
                    module.exports = async () => {
                        var http = require('http');

                        var params = {
                            host: 'jsonplaceholder.typicode.com',
                            port: 80,
                            method: 'GET',
                            path: '/posts/2'
                        };

                        return new Promise(function(resolve, reject) {
                                var req = http.request(params, function(res) {
                                    // reject on bad status
                                    if (res.statusCode < 200 || res.statusCode >= 300) {
                                        return reject(new Error('statusCode=' + res.statusCode));
                                    }
                                    // cumulate data
                                    var body = [];
                                    res.on('data', function(chunk) {
                                        body.push(chunk);
                                    });
                                    // resolve on end
                                    res.on('end', function() {
                                        try {
                                            body = JSON.parse(Buffer.concat(body).toString());
                                        } catch(e) {
                                            reject(e);
                                        }
                                        resolve(body);
                                    });
                                });
                                // reject on request error
                                req.on('error', function(err) {
                                    reject(err);
                                });
                            // IMPORTANT
                            req.end();
                            });
                        }   
                ",
                JavascriptCodeIdentifier = "HttpCallAsyncJavascriptExample",
                Args = new object[] { }
            };

            // Act
            var result = await _sut.RunAsync(jsRunnerParams);

            // Assert
            result.Should().NotBeNull();

            result.ToString().Should().Be(expectedResult);
        }

        [Fact]
        public async Task Must_BeAbleToMakeHTTPCallsAsync_From_HttpRequestLibrary()
        {
            // Arrange
            var expectedResult = "{\"userId\":1,\"id\":2,\"title\":\"qui est esse\",\"body\":\"est rerum tempore vitae\\nsequi sint nihil reprehenderit dolor beatae ea dolores neque\\nfugiat blanditiis voluptate porro vel nihil molestiae ut reiciendis\\nqui aperiam non debitis possimus qui neque nisi nulla\"}";

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = @"
                    module.exports = async () => {
                        var httpRequest = require('./httpRequest.js');

                        var params = {
                            host: 'jsonplaceholder.typicode.com',
                            port: 80,
                            method: 'GET',
                            path: '/posts/2'
                        };

                        return httpRequest(params);
                    }
                ",
                JavascriptCodeIdentifier = "HttpCallAsyncJavascriptFromHttpRequestLibraryExample",
                Args = new object[] { }
            };

            // Act
            var result = await _sut.RunAsync(jsRunnerParams);

            // Assert
            result.Should().NotBeNull();

            result.ToString().Should().Be(expectedResult);
        }

        [Fact]
        public async Task Must_BeAbleToMakeHTTPCalls_From_HttpRequestLibrary()
        {
            // Arrange
            var expectedResult = JsonSerializer.SerializeToElement(
                    new
                    {
                        result = "qui est esse"
                    }
                );

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = @"
                    module.exports = async () => {
                        var httpRequest = require('./httpRequest.js');

                        var params = {
                            host: 'jsonplaceholder.typicode.com',
                            port: 80,
                            method: 'GET',
                            path: '/posts/2'
                        };

                         var result = await httpRequest(params);

                         return { result : result.title };
                    }
                ",
                JavascriptCodeIdentifier = "HttpCallJavascriptFromHttpRequestLibraryExample",
                Args = new object[] { }
            };

            // Act
            var result = await _sut.RunAsync(jsRunnerParams);

            // Assert
            result.Should().NotBeNull();

            result.ToString().Should().Be(expectedResult.ToString());
        }

        [Fact]
        public async Task Must_BeAbleToMakePostHTTPCalls_From_HttpRequestLibrary()
        {
            // Arrange
            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = @"
                    module.exports = async () => {
                        var httpRequest = require('./httpRequest.js');

                        var params = {
                            host: 'jsonplaceholder.typicode.com',
                            port: 80,
                            method: 'POST',
                            body: {
                                title: 'foo',
                                body: 'bar',
                                userId: 1,
                              },
                            path: '/posts',
                            headers: { 'Content-type': 'application/json; charset=UTF-8' },
                        };

                         return await httpRequest(params);
                    }
                ",
                JavascriptCodeIdentifier = "HttpPostCallJavascriptFromHttpRequestLibraryExample",
                Args = new object[] { }
            };

            // Act
            var result = await _sut.RunAsync(jsRunnerParams);

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Must_BeAbleToRunSyncInAsyncFunction()
        {
            // Arrange
            var expectedResult = JsonSerializer.SerializeToElement(
                    new
                    {
                        result = 12
                    }
                );

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = @"
                    module.exports = async (input) => {
                         var output = {} 
                         var result = input.number * 4;
                         return output = {
                            result : result
                        };
                    }
                ",
                JavascriptCodeIdentifier = "SyncInAsyncExample",
                Args = new object[] {
                        new {
                            number = 3
                        }
                    }
            };

            // Act
            var result = await _sut.RunAsync(jsRunnerParams);

            // Assert
            result.Should().NotBeNull();

            result.ToString().Should().Be(expectedResult.ToString());
        }
    }
}