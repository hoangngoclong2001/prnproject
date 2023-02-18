using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Client_API_EF.Models
{
    public class ErrorViewModel
    {
        public string errorCode { get; set; } = string.Empty;
        public string errorMessage { get; set; } = string.Empty;
    }
}