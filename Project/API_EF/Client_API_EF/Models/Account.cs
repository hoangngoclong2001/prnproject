using System;
using System.Collections.Generic;

namespace Client_API_EF.Models
{
    public partial class Account
    {
        public int AccountId { get; set; }
        public string Email { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public string CustomerId { get; set; }
        public int? RefreshTokenId { get; set; }
        public int? EmployeeId { get; set; }
        public int? Role { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Employee Employee { get; set; }
        public virtual RefreshToken RefreshToken { get; set; }
    }
}
