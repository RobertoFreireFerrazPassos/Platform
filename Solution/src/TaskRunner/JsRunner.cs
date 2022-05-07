using Jering.Javascript.NodeJS;
using System.Threading;

namespace TaskRunner
{
    public class JsRunnerParams
    {
        public string JavascriptCode { get; set; }

        public string JavascriptCodeIdentifier { get; set; }

        public object?[]? Args { get; set; }
    }   

    public class JsRunner : IJsRunner
    {
        public async Task<object?> RunAsync(JsRunnerParams parameters)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(10));

            try
            {
                return await StaticNodeJSService.InvokeFromStringAsync<object>(
                        parameters.JavascriptCode,
                        parameters.JavascriptCodeIdentifier,
                        args: parameters.Args,
                        cancellationToken: cancellationTokenSource.Token
                    );
            }
            catch (Exception ex)
            {
                var exceptionMessage = RemoveStringAfter(ex.Message, "ReferenceError:").TrimEnd();

                throw new JsRunnerException(exceptionMessage);
            }
            finally
            {
                cancellationTokenSource.Dispose();
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