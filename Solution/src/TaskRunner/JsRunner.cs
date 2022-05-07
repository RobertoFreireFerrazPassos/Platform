using Jering.Javascript.NodeJS;

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

            var timeOut = 10;

            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(timeOut));

            try
            {
                return await StaticNodeJSService.InvokeFromStringAsync<object>(
                        parameters.JavascriptCode,
                        parameters.JavascriptCodeIdentifier,
                        args: parameters.Args,
                        cancellationToken: cancellationTokenSource.Token
                    );
            }
            catch (TaskCanceledException ex)
            {
                var exceptionMessage = RemoveStringAfter(ex.Message, "ReferenceError:").TrimEnd();

                var canceledExceptionMessage = $" It tooks more than {timeOut} seconds";

                throw new JsRunnerException(exceptionMessage + canceledExceptionMessage);
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