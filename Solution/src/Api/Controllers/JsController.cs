using Api.DataContracts.Requests;
using Api.EF;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskRunner.Domain;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JsController : ControllerBase
    {
        private readonly IJsService _jsService;

        public JsController(IJsService jsService)
        {
            _jsService = jsService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(JsFileRequest jsFileRequest)
        {
            var jsFileIdentifier = _jsService.Set(jsFileRequest);

            return Ok(jsFileIdentifier);
        }

        [HttpGet("{javaScriptIdentifier}/Run")]
        public async Task<IActionResult> Run(string javaScriptIdentifier)
        {
            return Ok(_jsService.Run(javaScriptIdentifier).Result);
        }

        [HttpGet("{javaScriptIdentifier}/Get")]
        public async Task<IActionResult> Get(string javaScriptIdentifier)
        {
            return Ok(_jsService.Get(javaScriptIdentifier).Result);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(_jsService.GetAll().Result);
        }
    }
}