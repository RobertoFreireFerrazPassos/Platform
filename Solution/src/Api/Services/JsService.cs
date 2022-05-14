using Api.DataContracts.Requests;
using Api.EF;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Web;
using TaskRunner;
using TaskRunner.Domain;

namespace Api.Services
{
    public interface IJsService
    {
        Task<JsFile> Get(string javaScriptIdentifier);
        Task<object> Run(string javaScriptIdentifier);
        string Set(JsFileRequest jsFileRequest);
        Task<IEnumerable<JsFile>> GetAll();
    }

    public class JsService : IJsService
    {
        private readonly MyContext _context;

        public JsService(MyContext context)
        {
            _context = context;
        }

        public async Task<JsFile> Get(string javaScriptIdentifier) 
        {
            return await _context.JsFile.FirstOrDefaultAsync(j => j.JavascriptCodeIdentifier == HttpUtility.UrlDecode(javaScriptIdentifier));
        }

        public async Task<IEnumerable<JsFile>> GetAll()
        {
            var jsList = await _context.JsFile.ToListAsync();

            return jsList;
        }

        public async Task<object> Run(string javaScriptIdentifier)
        {
            var jsFile = Get(javaScriptIdentifier);

            var stopwatch = new Stopwatch();

            var jsRunner = new JsRunner(10);

            var jsRunnerParams = new JsRunnerParams
            {
                JavascriptCode = EncapsulteJavascriptCodeInModule(jsFile.Result.JavascriptCode),
                JavascriptCodeIdentifier = jsFile.Result.JavascriptCodeIdentifier,
                Args = new object[] { }
            };

            try
            {
                stopwatch.Start();

                var result = jsRunner.RunAsync(jsRunnerParams).Result;

                stopwatch.Stop();

                return new 
                {
                    Time = $"Elapsed Time is {stopwatch.ElapsedMilliseconds} ms",
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new Exception(ex.Message);
            }     
        }

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

        public string Set(JsFileRequest jsFileRequest)
        {
            var js = new JsFile();

            js.JavascriptCode = jsFileRequest.JavascriptCode;

            js.SetJavascriptCodeIdentifier();

            _context.JsFile.Add(js);

            _context.SaveChanges();

            return js.JavascriptCodeIdentifier;
        }
    }
}
