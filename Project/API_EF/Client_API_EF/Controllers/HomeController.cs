using Client_API_EF.Dtos;
using Client_API_EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Xml.Linq;

namespace Client_API_EF.Controllers
{
    public class HomeController : Controller
    {

        private readonly IConfiguration configuration;
        public HomeController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        private readonly string BaseUrl = "http://localhost:5000/";
        public int SelectedCate { get; set; }
        [HttpGet]
        [Route("/cart")]
        public IActionResult cart()
        {
            return View();
        }

        [HttpPost]
        [Route("/cart")]
        public IActionResult cart([FromForm]OrderDto orderDto)
        {
            var allProductName = JsonConvert.DeserializeObject<List<string>>(GetData("api/products/allproductname").Result.Content.ReadAsStringAsync().Result);
            if (allProductName == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            if (!string.IsNullOrEmpty(orderDto.action))
            {
                switch (orderDto.action)
                {
                    case "BUY NOW":
                        if (!string.IsNullOrEmpty(orderDto.name) && allProductName.Contains(orderDto.name))
                        {
                            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("cart")))
                            {
                                List<OrderDetailDto> cart = JsonConvert.DeserializeObject<List<OrderDetailDto>>(HttpContext.Session.GetString("cart"));
                                foreach (var item in cart)
                                {
                                    if (item.Product.ProductName == orderDto.name)
                                    {
                                        item.Quantity++;
                                        item.Total = (decimal)item.Product.UnitPrice * item.Quantity;
                                        break;
                                    }
                                    else
                                    {
                                        AddToCart(cart, orderDto.name);
                                        break;
                                    }
                                }
                                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(cart));
                            }
                            else
                            {
                                List<OrderDetailDto> cart = new List<OrderDetailDto>();
                                AddToCart(cart, orderDto.name);
                                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(cart));
                            }
                        }
                        return cart();
                    case "ADD TO CART":
                        if (!string.IsNullOrEmpty(orderDto.name) && allProductName.Contains(orderDto.name))
                        {
                            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("cart")))
                            {
                                List<OrderDetailDto> cart = JsonConvert.DeserializeObject<List<OrderDetailDto>>(HttpContext.Session.GetString("cart"));
                                foreach (var item in cart)
                                {
                                    if (item.Product.ProductName == orderDto.name)
                                    {
                                        item.Quantity++;
                                        item.Total = (decimal)item.Product.UnitPrice * item.Quantity;
                                        break;
                                    }
                                    else
                                    {
                                        AddToCart(cart, orderDto.name);
                                        break;
                                    }
                                }
                                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(cart));
                            }
                            else
                            {
                                List<OrderDetailDto> cart = new List<OrderDetailDto>();
                                AddToCart(cart, orderDto.name);
                                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(cart));
                            }
                        }
                        return Redirect("/" + orderDto.name);
                    case "Remove":
                        if (!string.IsNullOrEmpty(orderDto.name) && allProductName.Contains(orderDto.name))
                        {
                            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("cart")))
                            {
                                List<OrderDetailDto> cart = JsonConvert.DeserializeObject<List<OrderDetailDto>>(HttpContext.Session.GetString("cart"));
                                foreach (var item in cart)
                                {
                                    if (item.Product.ProductName == orderDto.name)
                                    {
                                        cart.Remove(item);
                                        break;
                                    }
                                }
                                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(cart));
                            }
                        }
                        return cart();
                    case "+":
                        if (!string.IsNullOrEmpty(orderDto.name) && allProductName.Contains(orderDto.name))
                        {
                            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("cart")))
                            {
                                List<OrderDetailDto> cart = JsonConvert.DeserializeObject<List<OrderDetailDto>>(HttpContext.Session.GetString("cart"));
                                foreach (var item in cart)
                                {
                                    if (item.Product.ProductName == orderDto.name)
                                    {
                                        item.Quantity++;
                                        item.Total = (decimal)item.Product.UnitPrice * item.Quantity;
                                        break;
                                    }
                                }
                                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(cart));
                            }
                        }
                        return cart();
                    case "-":
                        if (!string.IsNullOrEmpty(orderDto.name) && allProductName.Contains(orderDto.name))
                        {
                            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("cart")))
                            {
                                List<OrderDetailDto> cart = JsonConvert.DeserializeObject<List<OrderDetailDto>>(HttpContext.Session.GetString("cart"));
                                foreach (var item in cart)
                                {
                                    if (item.Product.ProductName == orderDto.name)
                                    {
                                        item.Quantity--;
                                        item.Total = (decimal)item.Product.UnitPrice * item.Quantity;
                                        if (item.Quantity <= 0)
                                            cart.Remove(item);

                                        break;
                                    }
                                }
                                if (cart.Count <= 0)
                                    return index();

                                HttpContext.Session.SetString("cart", JsonConvert.SerializeObject(cart));
                            }
                        }
                        return cart();
                    case "ORDER":
                        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("cart")))
                        {
                            List<OrderDetailDto> cart = JsonConvert.DeserializeObject<List<OrderDetailDto>>(HttpContext.Session.GetString("cart"));
                            if (string.IsNullOrEmpty(HttpContext.Session.GetString("user")))
                                return Redirect("/signin");

                            UserDto user = JsonConvert.DeserializeObject<UserDto>(HttpContext.Session.GetString("user"));
                            List<OrderDetail> orderDetail = new List<OrderDetail>();
                            foreach (var item in cart)
                            {
                                OrderDetail product = new OrderDetail
                                {
                                    ProductId = item.Product.ProductId,
                                    UnitPrice = (decimal)item.Product.UnitPrice,
                                    Quantity = (short)item.Quantity,
                                    Discount = 0
                                };
                                orderDetail.Add(product);
                            }

                            if (((ClaimsIdentity)User.Identity).HasClaim(ClaimTypes.Role, "2"))
                            {
                                Order order = new Order
                                {
                                    CustomerId = user.Account.CustomerId,
                                    OrderDate = DateTime.Now,
                                    RequiredDate = DateTime.Now.AddDays(30),
                                    ShipName = user.Account.Customer.CompanyName,
                                    ShipAddress = user.Account.Customer.Address,
                                    OrderDetails = orderDetail
                                };

                                var Res = PostData("api/orders", JsonConvert.SerializeObject(order));
                                if (!Res.Result.IsSuccessStatusCode)
                                    return StatusCode(StatusCodes.Status500InternalServerError);

                                Order confirmedOrder = JsonConvert.DeserializeObject<Order>(Res.Result.Content.ReadAsStringAsync().Result);

                                SmtpClient smtp = new SmtpClient();
                                smtp.Host = "smtp.gmail.com";
                                smtp.Port = 587;
                                smtp.UseDefaultCredentials = false;
                                smtp.Credentials = new System.Net.NetworkCredential(configuration["Email:Mail"], configuration["Email:Password"]);
                                smtp.EnableSsl = true;
                                //smtp.Send(mail);
                                HttpContext.Session.SetString("cart", "");
                            }
                        }
                        return Redirect("/");
                }
            }
            return View();
        }

        public void AddToCart(List<OrderDetailDto>cart, string name)
        {
            var P = JsonConvert.DeserializeObject<Product>(GetData("api/products/" + name).Result.Content.ReadAsStringAsync().Result);
            decimal Total = (decimal)P.UnitPrice * 1;
            cart.Add(new OrderDetailDto { Product = P, Quantity = 1, Total = Total});
        }

        [Route("/detail/{id}")]
        public IActionResult detail()
        {
            return View();
        }

        [Route("/forgot")]
        public IActionResult forgot()
        {
            return View();
        }

        public IActionResult index()
        {
            var allCategoryName = GetData("api/categories/allcategoryname").Result;
            if (!allCategoryName.IsSuccessStatusCode)
            {
                ErrorViewModel error = new ErrorViewModel
                {
                    errorCode = allCategoryName.StatusCode.ToString(),
                    errorMessage = allCategoryName.ReasonPhrase
                };
                return View("error", error);
            }

            var bestsale = GetData("api/orderdetail/bestsale").Result;
            if (!bestsale.IsSuccessStatusCode)
            {
                ErrorViewModel error = new ErrorViewModel
                {
                    errorCode = bestsale.StatusCode.ToString(),
                    errorMessage = bestsale.ReasonPhrase
                };
                return View("error", error);
            }

            var hot = GetData("api/orderdetail/hot").Result;
            if (!hot.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status500InternalServerError);

            var newP = GetData("api/products/new").Result;
            if (!newP.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status500InternalServerError);
            
                ViewData["allCategory"] = JsonConvert.DeserializeObject<List<string>>(allCategoryName.Content.ReadAsStringAsync().Result);
            ViewData["bestsale"] = JsonConvert.DeserializeObject<List<Product>>(bestsale.Content.ReadAsStringAsync().Result);
            ViewData["hot"] = JsonConvert.DeserializeObject<List<Product>>(hot.Content.ReadAsStringAsync().Result);
            ViewData["newP"] = JsonConvert.DeserializeObject<List<Product>>(newP.Content.ReadAsStringAsync().Result);
            return View();
        }


        [Route("{something}")]
        public IActionResult index(string something, string search)
        {

            var allCategoryName = JsonConvert.DeserializeObject<List<string>>(GetData("api/categories/allcategoryname").Result.Content.ReadAsStringAsync().Result);
            if (allCategoryName == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            var allProductName = JsonConvert.DeserializeObject<List<string>>(GetData("api/products/allproductname").Result.Content.ReadAsStringAsync().Result);
            if (allProductName == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            if (!string.IsNullOrEmpty(something) && allCategoryName.Contains(something.Replace(' ', '/')))
            {
                var P = JsonConvert.DeserializeObject<List<Product>>(GetData("api/products/bycategory/" + something).Result.Content.ReadAsStringAsync().Result);
                if (P == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);

                ViewData["category"] = something;
                ViewData["byCategory"] = P;
            }
            else if (!string.IsNullOrEmpty(something) && allProductName.Contains(something))
            {
                var P = JsonConvert.DeserializeObject<Product>(GetData("api/products/" + something).Result.Content.ReadAsStringAsync().Result);
                if (P == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);

                ViewData["productName"] = something;
                ViewData["productDetail"] = P;

            }
         
            ViewData["allCategory"] = allCategoryName;
            return View();
        }
        [Route("{search}/{categoryid}")]
        public IActionResult index(string search, int categoryid)
        {
            if (!string.IsNullOrEmpty(search))
            {                var P = JsonConvert.DeserializeObject<List<Product>>(GetData("api/products/searchbycate/" + search + "?categoryid=" + categoryid).Result.Content.ReadAsStringAsync().Result);
                if (P == null)
                    return StatusCode(StatusCodes.Status500InternalServerError);

                ViewData["category"] = categoryid;
                ViewData["byCategory"] = P;
            }
            var allCategoryName = JsonConvert.DeserializeObject<List<string>>(GetData("api/categories/allcategoryname").Result.Content.ReadAsStringAsync().Result);
            if (allCategoryName == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            var allProductName = JsonConvert.DeserializeObject<List<string>>(GetData("api/products/allproductname").Result.Content.ReadAsStringAsync().Result);
            if (allProductName == null)
                return StatusCode(StatusCodes.Status500InternalServerError);

            ViewData["allCategory"] = allCategoryName;
            return View();
        }

        public IActionResult privacy()
        {
            return View();
        }

        [HttpGet]
        [Route("/signin")]
        public IActionResult signin()
        {
            if (string.IsNullOrEmpty(HttpContext.Request.Cookies["accessToken"]) && !string.IsNullOrEmpty(HttpContext.Request.Cookies["refreshToken"]))
            {
                UserDto u = new UserDto();
                u.RefreshToken = HttpContext.Request.Cookies["refreshToken"];

                var Res = PostData("auth/refresh-token", JsonConvert.SerializeObject(u));
                if (!Res.Result.IsSuccessStatusCode)
                    return StatusCode(StatusCodes.Status500InternalServerError);

                var user = JsonConvert.DeserializeObject<UserDto>(Res.Result.Content.ReadAsStringAsync().Result);

                HttpContext.Session.SetString("user", Res.Result.Content.ReadAsStringAsync().Result);

                validateToken(user.AccessToken.Replace("\"", ""));

                Response.Cookies.Append("refreshToken", user.RefreshToken, new CookieOptions { Expires = user.TokenExpires, HttpOnly = true, SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict });

                return Redirect("/");
            }

            return View();
        }

        [HttpPost]
        [Route("/signin")]
        public IActionResult signin(SigninDto signin)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var Res = PostData("auth/signin", JsonConvert.SerializeObject(signin));
            if (!Res.Result.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status500InternalServerError);

            var user = JsonConvert.DeserializeObject<UserDto>(Res.Result.Content.ReadAsStringAsync().Result);

            HttpContext.Session.SetString("user", Res.Result.Content.ReadAsStringAsync().Result);

            validateToken(user.AccessToken.Replace("\"", ""));

            Response.Cookies.Append("refreshToken", user.RefreshToken, new CookieOptions { Expires = user.TokenExpires, HttpOnly = true, SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict });

            return RedirectToAction("index");
        }

        [HttpGet]
        [Route("/signup")]
        public IActionResult signup()
        {
            return View();
        }

        [Authorize]
        [HttpGet]
        [Route("/signout")]
        public IActionResult signout()
        {
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");

            return RedirectToAction("index");
        }

        [HttpPost]
        [Route("/signup")]
        public IActionResult signup(SignupDto signup)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var Res = PostData("auth/signup", JsonConvert.SerializeObject(signup));
            if (!Res.Result.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return RedirectToAction("index");
        }

        [Route("/error")]
        public IActionResult error(ErrorViewModel error)
        {
            return View(error);
        }

        private void validateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["JWT:Issuer"],
                    ValidAudience = configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Secret"])),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var expires = jwtToken.ValidTo;
                var role = jwtToken.Claims.First(x => x.Type == ClaimTypes.Role);
                var email = jwtToken.Claims.First(x => x.Type == ClaimTypes.Email);
                List<ClaimsIdentity> identities = new List<ClaimsIdentity>
                {
                    new ClaimsIdentity(new[] { role }),
                    new ClaimsIdentity(new[] { email })
                };

                User.AddIdentities(identities);

                Response.Cookies.Append("accessToken", token, new CookieOptions { Expires = expires, HttpOnly = true, SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict });

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<HttpResponseMessage> PostData(string targerAddress, string content)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(BaseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            if (!string.IsNullOrEmpty(HttpContext.Request.Cookies["accessToken"]))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HttpContext.Request.Cookies["accessToken"]);
            }
            else
            {
                client.DefaultRequestHeaders.Add("refreshToken", HttpContext.Request.Cookies["refreshToken"]);
            }
            var Response = await client.PostAsync(targerAddress, new StringContent(content, Encoding.UTF8, "application/json"));
            return Response;
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