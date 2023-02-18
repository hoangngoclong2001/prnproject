using System.ComponentModel.DataAnnotations;

namespace Client_API_EF.Dtos
{
    public class SigninDto
    {
        [EmailAddress(ErrorMessage = "Invalid Email format")]
        [Required(ErrorMessage = "This field is required")]
        [MaxLength(50, ErrorMessage = "This field must not exceed 50 characters")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "This field is required")]
        public string Password { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
