﻿namespace Client_API_EF.Dtos
{
    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
    }
}
