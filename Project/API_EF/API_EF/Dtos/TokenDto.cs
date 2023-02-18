using API_EF.Models;

namespace API_EF.Dtos
{
    public class TokenDto
    {
        public string AccessToken { get; set; }
        public RefreshToken RefreshToken { get; set; }
    }
}
