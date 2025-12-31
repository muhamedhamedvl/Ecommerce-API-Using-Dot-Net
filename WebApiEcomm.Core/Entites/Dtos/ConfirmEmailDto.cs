using System.ComponentModel.DataAnnotations;

namespace WebApiEcomm.Core.Entites.Dtos
{
    public record ConfirmEmailDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; init; }

        [Required(ErrorMessage = "Token is required")]
        public string Token { get; init; }
    }
}
