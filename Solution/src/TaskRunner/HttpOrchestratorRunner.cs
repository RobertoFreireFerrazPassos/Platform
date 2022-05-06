using Client;

namespace TaskRunner
{
    public class HttpOrchestratorRunnerParams
    {
        public string Uri { get; set; }

        public string JavascriptCode { get; set; }
    }

    public class HttpOrchestratorRunner : IOrchestratorRunner
    {
        private readonly IClientHttp _clientHttp;

        private readonly IJsRunner _jsRunner;

        public HttpOrchestratorRunner(IClientHttp clientHttp, IJsRunner jsRunner)
        {
            _clientHttp = clientHttp;

            _jsRunner = jsRunner;
        }

        public object Run(HttpOrchestratorRunnerParams parameters)
        {
            var response = _clientHttp.GetAsync(parameters.Uri).Result;

            var cancellationTokenSource = new CancellationTokenSource();

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = parameters.JavascriptCode,
                Args = new object[] {
                        new {
                            response = response
                        }
                    },

                CancellationToken = cancellationTokenSource.Token
            };

            return _jsRunner.RunAsync(jsRunnerParams).Result;
        }
    }
}
