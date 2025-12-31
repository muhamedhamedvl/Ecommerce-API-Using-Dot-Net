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

        /// <summary>
        /// Request password reset email
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var message = await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
                return Ok(new { message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseApi(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseApi(500, $"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Reset password using reset token
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                await _authService.ResetPasswordAsync(resetPasswordDto);
                return Ok(new { message = "Password has been reset successfully" });
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
        /// Change password for authenticated user
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            try
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new ResponseApi(401, "User not authenticated"));
                }

                await _authService.ChangePasswordAsync(email, changePasswordDto);
                return Ok(new { message = "Password has been changed successfully" });
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
        /// Confirm user email
        /// </summary>
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailDto confirmEmailDto)
        {
            try
            {
                await _authService.ConfirmEmailAsync(confirmEmailDto.Email, confirmEmailDto.Token);
                return Ok(new { message = "Email has been confirmed successfully" });
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
        /// Resend email confirmation
        /// </summary>
        [HttpGet("resend-confirmation-email")]
        [Authorize]
        public async Task<IActionResult> ResendConfirmationEmail()
        {
            try
            {
                var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized(new ResponseApi(401, "User not authenticated"));
                }

                await _authService.ResendConfirmationEmailAsync(email);
                return Ok(new { message = "Confirmation email has been sent" });
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
    }
}
