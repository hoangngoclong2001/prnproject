using System.ComponentModel.DataAnnotations;

namespace API_EF.Dtos
{
    public class SigninDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
