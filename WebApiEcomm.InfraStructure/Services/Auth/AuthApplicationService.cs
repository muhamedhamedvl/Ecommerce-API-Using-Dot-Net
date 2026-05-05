using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Entites.Identity;
using WebApiEcomm.Core.Services.Auth;
using WebApiEcomm.InfraStructure.Options;

namespace WebApiEcomm.InfraStructure.Services.Auth
{
    public class AuthApplicationService : IAuthApplicationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly IEmailQueue _emailQueue;
        private readonly EmailTemplateService _templateService;
        private readonly EmailOptions _emailOptions;
        private readonly ILogger<AuthApplicationService> _logger;

        public AuthApplicationService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IEmailVerificationService emailVerificationService,
            IEmailQueue emailQueue,
            EmailTemplateService templateService,
            IOptions<EmailOptions> emailOptions,
            ILogger<AuthApplicationService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailVerificationService = emailVerificationService;
            _emailQueue = emailQueue;
            _templateService = templateService;
            _emailOptions = emailOptions.Value;
            _logger = logger;
        }

        public async Task RegisterAsync(RegisterRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
        {
            if (await _userManager.FindByEmailAsync(request.Email) is not null)
            {
                throw new AuthException("Email already exists", 409);
            }

            var user = new AppUser
            {
                Email = request.Email,
                UserName = request.UserName,
                DisplayName = request.UserName,
                EmailConfirmed = false,
                City = string.Empty,
                State = string.Empty,
                ZipCode = string.Empty
            };
            var create = await _userManager.CreateAsync(user, request.Password);
            if (!create.Succeeded)
            {
                throw new AuthException(string.Join(", ", create.Errors.Select(x => x.Description)));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, AppRoles.User);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                throw new AuthException(string.Join(", ", roleResult.Errors.Select(x => x.Description)));
            }

            var code = await _emailVerificationService.GenerateCodeAsync(user.Id, cancellationToken);
            var template = _templateService.BuildVerificationTemplate(user.UserName ?? "User", code);

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new AuthException("User email is required for verification.", 400);
            }

            try
            {
                await _emailQueue.QueueAsync(new EmailDto(user.Email, _emailOptions.FromAddress, template.Subject, template.HtmlBody), cancellationToken);
                _logger.LogInformation("Verification email queued for user {UserId} at {Email}", user.Id, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue verification email for user {UserId} at {Email}", user.Id, user.Email);
                throw new AuthException("User was created, but the verification email could not be queued. Please resend verification.", 503);
            }
        }

        public async Task<TokenPairResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);
            if (user is null)
            {
                throw new AuthException("Invalid credentials", 401);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (result.IsLockedOut)
            {
                throw new AuthException("User is locked out", 423);
            }

            if (!result.Succeeded)
            {
                throw new AuthException("Invalid credentials", 401);
            }

            if (!user.EmailConfirmed)
            {
                throw new AuthException("Email is not verified", 403);
            }

            return await _tokenService.CreateTokenPairAsync(user, ipAddress, userAgent, cancellationToken);
        }

        public Task<TokenPairResponse> RefreshAsync(RefreshTokenRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
            => _tokenService.RotateRefreshTokenAsync(request.RefreshToken, ipAddress, userAgent, cancellationToken);

        public async Task VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                throw new AuthException("User not found", 404);
            }

            var isValid = await _emailVerificationService.VerifyCodeAsync(user.Id, request.Code, cancellationToken);
            if (!isValid)
            {
                throw new AuthException("Verification code is invalid or expired", 400);
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        public async Task ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return;
            }

            if (user.EmailConfirmed)
            {
                return;
            }

            var code = await _emailVerificationService.GenerateCodeAsync(user.Id, cancellationToken);
            var template = _templateService.BuildVerificationTemplate(user.UserName ?? "User", code);
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                throw new AuthException("User email is required for verification.", 400);
            }

            try
            {
                await _emailQueue.QueueAsync(new EmailDto(user.Email, _emailOptions.FromAddress, template.Subject, template.HtmlBody), cancellationToken);
                _logger.LogInformation("Verification resend queued for user {UserId} at {Email}", user.Id, user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to queue verification resend for user {UserId} at {Email}", user.Id, user.Email);
                throw new AuthException("Verification email could not be queued. Please try again later.", 503);
            }
        }

        public Task LogoutAsync(string userId, LogoutRequest request, CancellationToken cancellationToken = default)
            => _tokenService.RevokeRefreshTokenAsync(userId, request.RefreshToken, "logout", cancellationToken);

        public async Task<TokenPairResponse> GetCurrentAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                throw new AuthException("User not found", 404);
            }
            return await _tokenService.CreateTokenPairAsync(user, null, null, cancellationToken);
        }
    }
}
