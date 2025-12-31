using WebApiEcomm.Core.Entites.Dtos;
using WebApiEcomm.Core.Entites.Identity;

namespace WebApiEcomm.Core.Interfaces.Auth
{
    public interface IAuth
    {
        Task<AppUser> RegisterAsync(RegisterDto registerDto);
        Task<AppUser> LoginAsync(LoginDto loginDto);
        Task<bool> CheckEmailExistsAsync(string email);
        Task<string> ForgotPasswordAsync(string email);
        Task ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task ChangePasswordAsync(string email, ChangePasswordDto changePasswordDto);
        Task ConfirmEmailAsync(string email, string token);
        Task ResendConfirmationEmailAsync(string email);
    }
}
