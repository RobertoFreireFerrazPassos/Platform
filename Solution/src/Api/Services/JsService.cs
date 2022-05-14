using Api.DataContracts.Requests;
using Api.EF;
using Microsoft.EntityFrameworkCore;
using System.Web;
using TaskRunner.Domain;

namespace Api.Services
{
    public interface IJsService
    {
        Task<JsFile> Get(string javaScriptIdentifier);
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
