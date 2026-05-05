using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApiEcomm.API.Controllers
{
    [Route("errors/{statusCode}")]
    [ApiController]
    public class ErrorController : ControllerBase
    {
        [HttpGet]
        public IActionResult HandleError(int statusCode)
        {
            switch (statusCode)
            {
                case StatusCodes.Status404NotFound:
                    return NotFound(new { Message = "Resource not found." });
                case StatusCodes.Status500InternalServerError:
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An unexpected error occurred." });
                case StatusCodes.Status400BadRequest:
                    return BadRequest(new { Message = "Bad request." });
                default:
                    return StatusCode(statusCode, new { Message = "An error occurred." });
            }
        }
    }
}
