using Microsoft.AspNetCore.Identity;
using Org.BouncyCastle.Crypto.Macs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Entites.Identity;
using WebApiEcomm.Core.Interfaces.Auth;
using WebApiEcomm.Core.Services;

namespace WebApiEcomm.InfraStructure.Repositores
{
    public class AuthRepository : IAuth
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly SignInManager<AppUser> _signInManager;

        public AuthRepository(UserManager<AppUser> userManager, IEmailService emailService, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _emailService = emailService;
            _signInManager = signInManager;
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
            };
            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(result.Errors.FirstOrDefault()?.Description ?? "User creation failed");
            }
            //Send email confirmation
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await SendEmail(user.Email, code, "active", "ActiveEmail", "Active your email,click on button to active");
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

            // Example: subject based on component
            string subject = $"Verification for {component}";

            // Format the content (replace placeholders in the template)
            string formattedContent = string.Format(content, email, code, component, message);

            var emailDto = new EmailDto(
                to: email,
                from: "mh1191128@gmail.com",
                subject: subject,
                content: formattedContent
            );

            await _emailService.SendEmail(emailDto);
        }
        public async Task<AppUser> LoginAsync(LoginDto loginDto)
        {
            if (loginDto == null)
            {
                throw new ArgumentNullException(nameof(loginDto));
            }
            var user = await _userManager.FindByEmailAsync(loginDto.Email );
            if (user == null)
            {
                throw new ArgumentException("Invalid username or password");
            }
            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, false, false);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Login failed");
            }
            return user;
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }
    }
}
