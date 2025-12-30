using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApiEcomm.API.Helper;
using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Entites.Identity;
using WebApiEcomm.Core.Interfaces.Auth;
using WebApiEcomm.Core.Services;

namespace WebApiEcomm.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuth _authService;
        private readonly IGenrateToken _tokenService;
        private readonly UserManager<AppUser> _userManager;

        public AccountController(IAuth authService, IGenrateToken tokenService, UserManager<AppUser> userManager)
        {
            _authService = authService;
            _tokenService = tokenService;
            _userManager = userManager;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            try
            {
                // Check if email exists
                if (await _authService.CheckEmailExistsAsync(registerDto.Email))
                {
                    return BadRequest(new ResponseApi(400, "Email already exists"));
                }

                var user = await _authService.RegisterAsync(registerDto);

                // Assign default User role
                await _userManager.AddToRoleAsync(user, AppRoles.User);

                var token = await _tokenService.CreateTokenAsync(user);
                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new UserDto
                {
                    Email = user.Email,
                    DisplayName = user.DisplayName ?? user.UserName,
                    Token = token,
                    Roles = roles
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseApi(400, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ResponseApi(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseApi(500, $"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            try
            {
                var user = await _authService.LoginAsync(loginDto);
                var token = await _tokenService.CreateTokenAsync(user);
                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new UserDto
                {
                    Email = user.Email,
                    DisplayName = user.DisplayName ?? user.UserName,
                    Token = token,
                    Roles = roles
                });
            }
            catch (ArgumentException ex)
            {
                return Unauthorized(new ResponseApi(401, ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new ResponseApi(401, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseApi(500, $"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Get current authenticated user
        /// </summary>
        [HttpGet("current-user")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new ResponseApi(401, "User not authenticated"));
                }

                var user = await _userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    return NotFound(new ResponseApi(404, "User not found"));
                }

                var token = await _tokenService.CreateTokenAsync(user);
                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new UserDto
                {
                    Email = user.Email,
                    DisplayName = user.DisplayName ?? user.UserName,
                    Token = token,
                    Roles = roles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseApi(500, $"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Check if email exists
        /// </summary>
        [HttpGet("check-email")]
        public async Task<ActionResult<bool>> CheckEmailExists([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new ResponseApi(400, "Email is required"));
            }

            return await _authService.CheckEmailExistsAsync(email);
        }
    }
}
