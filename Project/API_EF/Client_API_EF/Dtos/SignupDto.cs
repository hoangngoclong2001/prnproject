using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace Client_API_EF.Dtos
{
    public class SignupDto
    {
        [EmailAddress(ErrorMessage = "Invalid Email format")]
        [Required(ErrorMessage = "This field is required")]
        [MaxLength(50, ErrorMessage = "This field must not over 50 characters")]
        public string Email { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public string Password { get; set; }
        [Required(ErrorMessage = "This field is required")]
        [Compare(nameof(Password), ErrorMessage = "Password is not match")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "This field is required")]
        [MaxLength(5, ErrorMessage = "This field must not over 5 characters")]
        public string CompanyId { get; set; }
        [Required(ErrorMessage = "This field is required")]
        [MaxLength(40, ErrorMessage = "This field must not over 40 characters")]
        public string CompanyName { get; set; }
        [Required(ErrorMessage = "This field is required")]
        [MaxLength(30, ErrorMessage = "This field must not over 30 characters")]
        public string ContactName { get; set; }
        [Required(ErrorMessage = "This field is required")]
        [MaxLength(30, ErrorMessage = "This field must not over 30 characters")]
        public string ContactTitle { get; set; }
        [Required(ErrorMessage = "This field is required")]
        [MaxLength(60, ErrorMessage = "This field must not over 60 characters")]
        public string Address { get; set; }
    }
}
