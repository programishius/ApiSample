using Microsoft.AspNetCore.Mvc;
using ApiSample.Services;
using ApiSample.Models;
using ApiSample.Authentication;

namespace ApiSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [ApiKey]
    public class DataController : ControllerBase
    {
        
        private readonly IDataServices _dataService;
        private readonly int _defaultExpirationSeconds;

        public DataController(IDataServices dataService, Microsoft.Extensions.Options.IOptions<Settings> options)
        {
            _dataService = dataService;
            _defaultExpirationSeconds = options.Value.DefaultExpirationInSeconds;
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] Request request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Key))
                return BadRequest("Invalid request data.");

            int expiration = request.ExpireAfterSeconds ?? _defaultExpirationSeconds;
            try
            {
                _dataService.Create(request.Key, request.Values, expiration);
            }
            catch (System.ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            return Ok(new { message = "Entry created/updated successfully." });
        }

        [HttpPost("append")]
        public IActionResult Append([FromBody] Request request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Key))
                return BadRequest("Invalid request data.");

            int expiration = request.ExpireAfterSeconds ?? _defaultExpirationSeconds;
            try
            {
                _dataService.Append(request.Key, request.Values, expiration);
            }
            catch (System.ArgumentOutOfRangeException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            return Ok(new { message = "Entry appended successfully." });
        }


        [HttpDelete]
        public IActionResult Delete(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return BadRequest("Invalid key.");

            if (!_dataService.Delete(key))
                return NotFound(new { message = "Entry not found." });

            return Ok(new { message = "Entry deleted successfully." });
        }

        [HttpGet]
        public IActionResult Get(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return BadRequest("Invalid key.");

            if (_dataService.Get(key, out var values))
            {
                return Ok(values);
            }
            else
            {
                return NotFound(new { message = "Entry not found or expired." });
            }
        }

    }
}
