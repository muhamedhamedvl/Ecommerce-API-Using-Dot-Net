using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Entites.Identity;
using WebApiEcomm.Core.Interfaces.Auth;
using WebApiEcomm.Core.Services;
using WebApiEcomm.InfraStructure.Configuration;

namespace WebApiEcomm.InfraStructure.Repositores
{
    public class AuthRepository : IAuth
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly EmailSmtpMergedSettings _smtp;

        public AuthRepository(
            UserManager<AppUser> userManager,
            IEmailService emailService,
            EmailSmtpMergedSettings smtp)
        {
            _userManager = userManager;
            _emailService = emailService;
            _smtp = smtp;
        }
        public async Task<AppUser> RegisterAsync(RegisterDto registerDto)
        {
            if (registerDto == null)
            {
                throw new ArgumentNullException(nameof(registerDto));
            }
            if (await _userManager.FindByNameAsync(registerDto.UserName) != null)
            {
                throw new ArgumentException("Username already exists");
            }
            if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
            {
                throw new ArgumentException("Email already exists");
            }
            AppUser user = new AppUser
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                DisplayName = registerDto.UserName,
                // Keep identity required profile fields non-null to satisfy DB constraints.
                City = string.Empty,
                State = string.Empty,
                ZipCode = string.Empty
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.Errors.FirstOrDefault()?.Description ?? "User creation failed");
            }
            //Send email confirmation
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await SendEmail(
                user.Email,
                code,
                "Email verification",
                $"<p>Please confirm your account for <strong>{WebUtility.HtmlEncode(user.Email)}</strong>.</p>",
                "Use the verification code below to confirm your email.");
            return user;
        }
        public async Task SendEmail(string email, string code, string component, string content, string message)
        {
            if (string.IsNullOrEmpty(email)
                || string.IsNullOrEmpty(code)
                || string.IsNullOrEmpty(component)
                || string.IsNullOrEmpty(content)
                || string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Email, code, component, content, and message must not be empty");
            }

            string subject = string.IsNullOrWhiteSpace(component) ? "Verification" : $"Verification — {component}";

            var body = ResolveEmailBody(email, code, component, content, message);

            var emailDto = new EmailDto(
                to: email,
                from: string.IsNullOrWhiteSpace(_smtp.FromAddress) ? email : _smtp.FromAddress,
                subject: subject,
                content: body
            );

            await _emailService.SendEmail(emailDto);
        }

        private static string ResolveEmailBody(string email, string code, string component, string content, string message)
        {
            if (!string.IsNullOrWhiteSpace(content) && content.Contains('<', StringComparison.Ordinal))
                return content;

            var safeMessage = WebUtility.HtmlEncode(message);
            var safeCode = WebUtility.HtmlEncode(code);
            var safeEmail = WebUtility.HtmlEncode(email);
            var safeComponent = WebUtility.HtmlEncode(component);

            return $"""
                <p>{safeMessage}</p>
                <p><strong>Code:</strong> {safeCode}</p>
                <p><strong>For:</strong> {safeEmail}</p>
                <p><strong>Type:</strong> {safeComponent}</p>
                """;
        }
        public async Task<AppUser> LoginAsync(LoginDto loginDto)
        {
            if (loginDto == null)
            {
                throw new ArgumentNullException(nameof(loginDto));
            }
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                throw new ArgumentException("Invalid username or password");
            }

            // JWT API: validate password with UserManager only. Do not use SignInManager.PasswordSignInAsync — that requires cookie auth (Identity.Application).
            if (!await _userManager.CheckPasswordAsync(user, loginDto.Password).ConfigureAwait(false))
            {
                throw new InvalidOperationException("Login failed");
            }

            return user;
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }

        public async Task<string> ForgotPasswordAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email), "Email is required");
            }

            var user = await _userManager.FindByEmailAsync(email);
            
            // For security reasons, don't reveal if user exists or not
            // But still generate and send token if user exists
            if (user == null)
            {
                // Return success message without revealing user doesn't exist
                return "If an account with that email exists, a password reset link has been sent.";
            }

            // Generate password reset token
            string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Send password reset email
            string emailContent = $@"
                <h2>Password Reset Request</h2>
                <p>Hello {user.DisplayName ?? user.UserName},</p>
                <p>You requested to reset your password. Please use the following token to reset your password:</p>
                <p><strong>Token:</strong> {resetToken}</p>
                <p>This token will expire in 24 hours.</p>
                <p>If you did not request this reset, please ignore this email.</p>
            ";

            await SendEmail(
                user.Email, 
                resetToken, 
                "Password Reset", 
                emailContent, 
                "Reset your password using the token provided above");

            return "If an account with that email exists, a password reset link has been sent.";
        }

        public async Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            if (resetPasswordDto == null)
            {
                throw new ArgumentNullException(nameof(resetPasswordDto));
            }

            var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                throw new ArgumentException("Invalid password reset request");
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Token, resetPasswordDto.NewPassword);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Password reset failed: {errors}");
            }
        }

        public async Task ChangePasswordAsync(string email, ChangePasswordDto changePasswordDto)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email), "Email is required");
            }

            if (changePasswordDto == null)
            {
                throw new ArgumentNullException(nameof(changePasswordDto));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(
                user, 
                changePasswordDto.CurrentPassword, 
                changePasswordDto.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Password change failed: {errors}");
            }
        }

        public async Task ConfirmEmailAsync(string email, string token)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email), "Email is required");
            }

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token), "Token is required");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Email confirmation failed: {errors}");
            }
        }

        public async Task ResendConfirmationEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email), "Email is required");
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            if (user.EmailConfirmed)
            {
                throw new InvalidOperationException("Email is already confirmed");
            }

            // Generate new confirmation token
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            await SendEmail(
                user.Email, 
                code, 
                "Email Confirmation", 
                "Please confirm your email", 
                "Confirm your email by using the token provided");
        }
    }
}
