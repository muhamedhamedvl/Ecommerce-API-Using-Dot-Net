using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApiEcomm.Core.Interfaces.IUnitOfWork;

namespace WebApiEcomm.API.Controllers
{
    /// <summary>
    /// ⚠️ This controller is for testing and simulating different types of errors.
    /// It is used for debugging purposes only (Not for production use).
    /// - /not-found → returns 404 Not Found
    /// - /server-error → returns 500 Internal Server Error
    /// - /bad-request → returns 400 Bad Request
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    public class bugcontroller : BaseController
    {
        public bugcontroller(IUnitOfWork work, IMapper mapper) : base(work, mapper)
        {
        }

        /// <summary>
        /// Simulates a 404 Not Found error.
        /// </summary>
        [HttpGet("not-found")]
        public async Task<IActionResult> GetNotFound()
        {
            var category = await _work.CategoryRepository.GetByIdAsync(100);
            if (category == null) return NotFound();
            return Ok(category);
        }

        /// <summary>
        /// Simulates a 500 Internal Server Error.
        /// </summary>
        [HttpGet("server-error")]
        public async Task<IActionResult> GetServerError()
        {
            var category = await _work.CategoryRepository.GetByIdAsync(100);
            category.Name = "";
            return Ok(category);
        }

        /// <summary>
        /// Simulates a 400 Bad Request error with parameter.
        /// </summary>
        [HttpGet("bad-request/{id}")]
        public async Task<IActionResult> GetBadRequest(int id)
        {
            return Ok();
        }

        /// <summary>
        /// Simulates a 400 Bad Request error without parameter.
        /// </summary>
        [HttpGet("bad-request")]
        public async Task<IActionResult> GetBadRequest()
        {
            return BadRequest();
        }
    }
}
