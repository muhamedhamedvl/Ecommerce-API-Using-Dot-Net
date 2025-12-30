using System.ComponentModel.DataAnnotations;

namespace WebApiEcomm.Core.Entites.Dtos
{
    public record LoginDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; init; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; init; }
    }
}
