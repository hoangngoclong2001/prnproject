using API_EF.Models;
using System;
using System.Collections.Generic;

namespace API_EF.Dtos
{
    public partial class CustomerDto
    {
        public CustomerDto()
        {
            Orders = new HashSet<Order>();
        }

        public string CustomerId { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string? ContactName { get; set; }
        public string? ContactTitle { get; set; }
        public string? Address { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
