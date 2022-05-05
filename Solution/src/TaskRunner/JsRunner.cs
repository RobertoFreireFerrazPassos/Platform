using Jering.Javascript.NodeJS;

namespace TaskRunner
{
    public class JsRunner
    {
        public static async Task<object?> RunAsync(string javascriptCode, object?[]? args, CancellationToken cancellationToken)
        {
            string javascriptModule = @"
                module.exports = (callback, input) => {
                    var output = {};
                    "
                    + javascriptCode +
                    @"
                    callback(null, output);
                }";

            try
            {
                return await StaticNodeJSService.InvokeFromStringAsync<object>(javascriptModule, args: args, cancellationToken : cancellationToken);
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