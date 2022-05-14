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
        private readonly ILogger<JsController> _logger;

        private readonly IJsService _jsService;

        public JsController(
                ILogger<JsController> logger,
                IJsService jsService
            )
        {
            _logger = logger;
            _jsService = jsService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(JsFileRequest jsFileRequest)
        {
            var jsFileIdentifier = _jsService.Set(jsFileRequest);

            return Ok(jsFileIdentifier);
        }

        [HttpGet("{javaScriptIdentifier}/Run")]
        public async Task<IActionResult> Run([FromQuery] string javaScriptIdentifier)
        {

            return Ok();
        }

        [HttpGet("{javaScriptIdentifier}/Get")]
        public async Task<IActionResult> Get(string javaScriptIdentifier)
        {
            var jsFile = _jsService.Get(javaScriptIdentifier);

            return Ok(jsFile);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var jsFileList = _jsService.GetAll();

            return Ok(jsFileList);
        }
    }
}