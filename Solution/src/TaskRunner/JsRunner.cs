using Jering.Javascript.NodeJS;
using TaskRunner.Domain;

namespace TaskRunner
{
    public class JsRunnerParams : JsFile
    {
        public object?[]? Args { get; set; }
    }   

    public class JsRunner : IJsRunner
    {
        private readonly int _timeOut = 10;

        public JsRunner(int timeOut)
        {
            _timeOut = timeOut;
        }

        public async Task<object?> RunAsync(JsRunnerParams parameters)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(_timeOut));

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

                var canceledExceptionMessage = $" It took more than {_timeOut} seconds";

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