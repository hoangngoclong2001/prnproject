using System;
using System.Collections.Generic;

namespace API_EF.Models
{
    public partial class Picture
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? PictureFileName { get; set; }
        public bool? PictureType { get; set; }

        public virtual Product? Product { get; set; }
    }
}
