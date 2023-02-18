namespace API_EF.Dtos
{
    public class OrderDto
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string? CustomerName { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Price { get; set; }
        public short Quantity { get; set; }
    }
}
