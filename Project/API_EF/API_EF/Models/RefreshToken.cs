using System;
using System.Collections.Generic;

namespace API_EF.Models
{
    public partial class RefreshToken
    {
        public int AccountId { get; set; }
        public string TokenId { get; set; }
        public string Token { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }

        public virtual Account Account { get; set; }
    }
}
