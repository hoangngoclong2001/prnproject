using Client_API_EF.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Client_API_EF.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IConfiguration configuration;
        public ProfileController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        private readonly string BaseUrl = "http://localhost:5000/";
        public IActionResult index()
        {
            //var claims = ClaimsPrincipal.Current.Identities.First().Claims.ToList();
            //var email = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var identity = (ClaimsIdentity)User.Identity;
            var claims = identity.Claims.ToList();
            var email = claims?.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            //var account = GetData("api/profile/email/" + email).Result;
            ViewData["account"] = JsonConvert.DeserializeObject<List<Account>>(GetData("api/profile/email/" + email).Result.Content.ReadAsStringAsync().Result);
            var account1 = ViewData["account"] as Account;
            var customer = GetData("api/profile/customer/" + account1.CustomerId + 0).Result;
            ViewData["customer"] = JsonConvert.DeserializeObject<List<Customer>>(customer.Content.ReadAsStringAsync().Result);
            return View();
        }

        [Route("/profile/orders")]
        public IActionResult orders()
        {
            return View();
        }
        public async Task<HttpResponseMessage> GetData(string targetAddress)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrEmpty(HttpContext.Request.Cookies["accessToken"]))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Request.Cookies["accessToken"]);
            }
            var Response = await client.GetAsync(targetAddress);
            return Response;
        }
    }
}
