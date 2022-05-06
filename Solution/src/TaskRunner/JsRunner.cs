using Jering.Javascript.NodeJS;

namespace TaskRunner
{
    public class JsRunnerParams
    {
        public string JavascriptCode { get; set; }

        public object?[]? Args { get; set; }

        public CancellationToken CancellationToken { get; set; }
    }   

    public class JsRunner : IJsRunner
    {
        public async Task<object?> RunAsync(JsRunnerParams parameters)
        {
            string javascriptModule = @"
                module.exports = (callback, input) => {
                    var output = {};
                    "
                    + parameters.JavascriptCode +
                    @"
                    callback(null, output);
                }";

            try
            {
                return await StaticNodeJSService.InvokeFromStringAsync<object>(javascriptModule, args: parameters.Args, cancellationToken : parameters.CancellationToken);
            }
            catch (Exception ex)
            {
                var exceptionMessage = RemoveStringAfter(ex.Message, "ReferenceError:").TrimEnd();

                throw new JsRunnerException(exceptionMessage);
            }

            string RemoveStringAfter(string text, string delimiter) {
                int index = text.LastIndexOf(delimiter);

                if (index >= 0)
                {
                    text = text.Substring(0, index);
                }

                return text;
            }
        }
    }
}