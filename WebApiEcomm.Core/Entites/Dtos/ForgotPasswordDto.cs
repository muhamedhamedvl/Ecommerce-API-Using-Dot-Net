using System.ComponentModel.DataAnnotations;

namespace WebApiEcomm.Core.Entites.Dtos
{
    public record ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; init; }
    }
}
