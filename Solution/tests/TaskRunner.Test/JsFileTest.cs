using FluentAssertions;
using System;
using System.Linq;
using TaskRunner.Domain;
using Xunit;

namespace TaskRunner.Test
{
    public class JsFileTest
    {
        private IJsFile _sut;

        public static string RandomString(int length)
        {
            var random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [Theory]
        [InlineData("Functioneval(", true)]
        [InlineData("require('./test.js')", true)]
        [InlineData("require('./test.js') require('./s.js') require('./n.js')", true)]
        [InlineData("require(\"./test.js\")", true)]
        [InlineData("require('')", true)]
        [InlineData("require(\"\")", true)]
        [InlineData("require()", true)]
        [InlineData("var=1; Functioneval(\"text\")", true)]
        [InlineData("var=1; Functioneval(2) Functioneval(param)", true)]
        [InlineData("var=1; require('./httpRequest.js');", true)]
        [InlineData("var=1; require('./test.js')", true)]
        [InlineData("var=1; require('./test.js') require('./s.js') require('./n.js')", true)]
        [InlineData("var=1; require(\"./test.js\")", true)]
        [InlineData("var=1; require('')", true)]
        [InlineData("var=1; require(\"\")", true)]
        [InlineData("var=1; require()", true)]
        [InlineData("eval(1 + 2)", false)]
        [InlineData("var a = -1; eval(1 + 2);", false)]
        [InlineData("require('fs')", false)]
        [InlineData("require(\"fs\")", false)]
        [InlineData("require(\"fs\") require('./s.js') require('./n.js')", false)]
        [InlineData("require('./test.js') require('./s.js') require(\"fs\")", false)]
        [InlineData("var=1; Functioneval(2) eval(1 + 2) Functioneval(param)", false)]
        [InlineData("var=1; Functioneval(2) Functioneval(param) eval(1 + 2)", false)]
        public void Must_ValidateJavascriptCode(string javascriptCode,bool isValidCode)
        {
            // Arrange
            _sut = new JsFile()
            {
                JavascriptCode = javascriptCode,
                JavascriptCodeIdentifier = RandomString(16)
            };

            // Act
            var result = _sut.IsValidJavascriptCode();

            // Assert
            result.Should().Be(isValidCode);
        }
    }
}
