using Client_API_EF.Models;

namespace Client_API_EF.Dtos
{
    public class OrderDetailDto
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }
}
