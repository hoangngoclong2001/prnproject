using Client_API_EF.Models;

namespace Client_API_EF.Dtos
{
    public class UserDto
    {
        public Account Account { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime TokenCreated { get; set; }
        public DateTime TokenExpires { get; set; }
    }
}
