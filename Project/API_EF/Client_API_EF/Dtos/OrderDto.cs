using Client_API_EF.Models;

namespace Client_API_EF.Dtos
{
    public class OrderDto
    {
        public string name { get; set; }
        public string action { get; set; }
        public Customer Customer { get; set; }
    }
}
