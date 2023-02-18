using Client_API_EF.Dtos;
using Client_API_EF.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Client_API_EF.Controllers
{
    [Authorize(Roles = "1")]
    public class AdminController : Controller
    {
        private readonly string BaseUrl = "http://localhost:5000/";
        
        [Route("/admin/customers/detail/{id}")]
        public IActionResult customerdetail(string id)
        {
            var Res = GetData("api/customers/" + id).Result;
            if (!Res.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status500InternalServerError);

            Customer customer = JsonConvert.DeserializeObject<Customer>(Res.Content.ReadAsStringAsync().Result);
            return View(customer);
        }

        [Route("/admin/customers")]
        public IActionResult customers(string search, [FromQuery] PaginationParams @params)
        {
            if (@params.ItemsPerPage == 0)
                @params.ItemsPerPage = 10;

            List<Customer> customers = new List<Customer>();
            PaginationMetadata pagination = new PaginationMetadata();

            if (!string.IsNullOrEmpty(search))
            {
                ViewData["search"] = search;
                var Res = GetData("api/customers/search/" + search + "?page=" + @params.Page.ToString() + "&itemsperpage=" + @params.ItemsPerPage.ToString()).Result;
                customers = JsonConvert.DeserializeObject<List<Customer>>(Res.Content.ReadAsStringAsync().Result);
                pagination = JsonConvert.DeserializeObject<PaginationMetadata>(Res.Headers.GetValues("X-Pagination").FirstOrDefault());
            }
            else
            {
                var Res = GetData("api/customers?page=" + @params.Page.ToString() + "&itemsperpage=" + @params.ItemsPerPage.ToString()).Result;
                customers = JsonConvert.DeserializeObject<List<Customer>>(Res.Content.ReadAsStringAsync().Result);
                pagination = JsonConvert.DeserializeObject<PaginationMetadata>(Res.Headers.GetValues("X-Pagination").FirstOrDefault());
            }

            ViewData["pagination"] = pagination;
            return View(customers);
        }

        [Route("/admin/dashboard")]
        public IActionResult dashboard()
        {
            return View();
        }

        [Route("/admin/orders/detail/{id}")]
        public IActionResult orderdetail(string id)
        {
            var ResDetail = GetData("api/orderdetail/" + id).Result;
            if (!ResDetail.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status500InternalServerError);

            var ResOrder = GetData("api/orders/" + id).Result;
            if (!ResOrder.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status500InternalServerError);

            List<OrderDetail> orderdetails = JsonConvert.DeserializeObject<List<OrderDetail>>(ResDetail.Content.ReadAsStringAsync().Result);

            ViewData["order"] = JsonConvert.DeserializeObject<Order>(ResOrder.Content.ReadAsStringAsync().Result);
            return View(orderdetails);
        }

        [Route("/admin/orders")]
        public IActionResult orders(string from, string to, [FromQuery] PaginationParams @params)
        {
            if (@params.ItemsPerPage == 0)
                @params.ItemsPerPage = 10;

            DateTime?[] minmax = new DateTime?[2];
            List<Order> orders = new List<Order>();
            PaginationMetadata pagination = new PaginationMetadata();

            if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
            {
                ViewData["from"] = from;
                ViewData["to"] = to;

                string filter = from + "_" + to;
                var Res = GetData("api/orders/filter/" + filter + "?page=" + @params.Page.ToString() + "&itemsperpage=" + @params.ItemsPerPage.ToString()).Result;
                orders = JsonConvert.DeserializeObject<List<Order>>(Res.Content.ReadAsStringAsync().Result);
                pagination = JsonConvert.DeserializeObject<PaginationMetadata>(Res.Headers.GetValues("X-Pagination").FirstOrDefault());
            }
            else
            {
                var Res = GetData("api/orders?page=" + @params.Page.ToString() + "&itemsperpage=" + @params.ItemsPerPage.ToString()).Result;
                orders = JsonConvert.DeserializeObject<List<Order>>(Res.Content.ReadAsStringAsync().Result);
                pagination = JsonConvert.DeserializeObject<PaginationMetadata>(Res.Headers.GetValues("X-Pagination").FirstOrDefault());
            }
            minmax = JsonConvert.DeserializeObject<DateTime?[]>(GetData("api/orders/minmax").Result.Content.ReadAsStringAsync().Result);

            ViewData["minmax"] = minmax;
            ViewData["pagination"] = pagination;
            return View(orders);
        }

        [HttpGet]
        [Route("/admin/products/create")]
        public IActionResult productcreate()
        {
            List<SelectListItem> categories = JsonConvert.DeserializeObject<List<SelectListItem>>(GetData("api/categories/selectlist").Result.Content.ReadAsStringAsync().Result);
            ViewData["categories"] = categories;
            return View();
        }

        [HttpPost]
        [Route("/admin/products/create")]
        public IActionResult productcreate(Product product, [FromForm] IFormFile picture)
        {
            List<SelectListItem> categories = JsonConvert.DeserializeObject<List<SelectListItem>>(GetData("api/categories/selectlist").Result.Content.ReadAsStringAsync().Result);
            ViewData["categories"] = categories;

            Category category = JsonConvert.DeserializeObject<Category>(GetData("api/categories/" + product.CategoryId).Result.Content.ReadAsStringAsync().Result);
            product.Category = category;

            if (!ModelState.IsValid)
            {
                foreach (var error in ViewData.ModelState.Values.SelectMany(modelState => modelState.Errors))
                {
                    string err = error.ErrorMessage;
                }
                return View(product);
            }

            if (picture != null && picture.Length > 0)
            {
                var fileName = Path.GetFileName(picture.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                picture.CopyTo(stream);

                //product.PictureFileName = picture.FileName;
            }
            else
            {
                //product.PictureFileName = "1.jpg";
            }

            var Res = PostData("api/products/create", JsonConvert.SerializeObject(product));

            if (!Res.Result.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status500InternalServerError);

            return RedirectToAction("products");
        }
        [HttpGet]
        [Route("/admin/products/detail/{id}")]
        public IActionResult productdetail(string id)
        {
            var Res = GetData("api/products/" + id).Result;
            if (!Res.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status500InternalServerError);

            Product product = JsonConvert.DeserializeObject<Product>(Res.Content.ReadAsStringAsync().Result);
            return View(product);
        }
        [HttpGet]
        [Route("/admin/products/delete/{id}")]
        public IActionResult productdelete(string id)
        {
            //var product = JsonConvert.DeserializeObject<Product>(GetData("api/products/" + id).Result.Content.ReadAsStringAsync().Result);
            //var Res = PostData("api/products/delete", JsonConvert.SerializeObject(product)).Result;
            //if (!Res.IsSuccessStatusCode)
            //    return StatusCode(StatusCodes.Status500InternalServerError);

            //return RedirectToAction("products");
            var Res = GetData("api/products/delete/" + id).Result;

            if (!Res.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status500InternalServerError);
            ViewData["deleteProduct"] = "Can't delete because this product are in " + Res + " Order";
            return Redirect("/admin/products");
        }

        [HttpGet]
        [Route("/admin/products/edit/{id}")]
        public IActionResult productedit(string id)
        {
            var Res = GetData("api/products/" + id).Result;
            Product product = JsonConvert.DeserializeObject<Product>(Res.Content.ReadAsStringAsync().Result);

            var categories = JsonConvert.DeserializeObject<List<SelectListItem>>(GetData("api/categories/selectlist").Result.Content.ReadAsStringAsync().Result);

            ViewBag.CategoryId = new SelectList(categories, "Value", "Text", product.Category.CategoryId);
            return View(product);
        }

        [HttpPost]
        [Route("/admin/products/edit/{id}")]
        public IActionResult productedit(Product product, [FromForm] IFormFile picture)
        {
            Category category = JsonConvert.DeserializeObject<Category>(GetData("api/categories/" + product.CategoryId).Result.Content.ReadAsStringAsync().Result);
            Product oldP = JsonConvert.DeserializeObject<Product>(GetData("api/products/" + product.ProductId).Result.Content.ReadAsStringAsync().Result);
            product.Category = category;

            ModelState.Remove("picture");

            if (!ModelState.IsValid)
                return BadRequest();

            if (picture != null && picture.Length > 0)
            {
                var fileName = Path.GetFileName(picture.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                picture.CopyTo(stream);

                //product.PictureFileName = picture.FileName;
            }
            //else if (oldP.PictureFileName != null)
            //{
            //    product.PictureFileName = oldP.PictureFileName;
            //}
            //else
            //{
            //    product.PictureFileName = "1.jpg";
            //}

            var Res = PostData("api/products/edit", JsonConvert.SerializeObject(product));

            if (!Res.Result.IsSuccessStatusCode)
                return StatusCode(StatusCodes.Status500InternalServerError);

            Product P = JsonConvert.DeserializeObject<Product>(Res.Result.Content.ReadAsStringAsync().Result);

            return Redirect("/admin/products/edit/" + P.ProductId);
        }

        [Route("/admin/products")]
        public IActionResult products(string filter, string search, [FromQuery] PaginationParams @params)
        {
            if (@params.ItemsPerPage == 0)
                @params.ItemsPerPage = 10;

            List<Product> products = new List<Product>();
            List<SelectListItem> categories = new List<SelectListItem>();
            PaginationMetadata pagination = new PaginationMetadata();
            if (!string.IsNullOrEmpty(filter))
            {
                ViewData["filter"] = filter;
                if (filter.Contains('/'))
                    filter = filter.Replace('/', '-');

                var Res = GetData("api/products/filter/" + filter + "?page=" + @params.Page.ToString() + "&itemsperpage=" + @params.ItemsPerPage.ToString()).Result;
                if (!Res.IsSuccessStatusCode)
                {
                    ErrorViewModel error = new ErrorViewModel
                    {
                        errorCode = Res.StatusCode.ToString(),
                        errorMessage = Res.ReasonPhrase
                    };
                    return View("error", error);
                }

                products = JsonConvert.DeserializeObject<List<Product>>(Res.Content.ReadAsStringAsync().Result);
                pagination = JsonConvert.DeserializeObject<PaginationMetadata>(Res.Headers.GetValues("X-Pagination").FirstOrDefault());
                categories.Add(new SelectListItem
                {
                    Text = "All",
                    Value = ""
                });
            }
            else if (!string.IsNullOrEmpty(search))
            {
                ViewData["search"] = search;
                var Res = GetData("api/products/search/" + search + "?page=" + @params.Page.ToString() + "&itemsperpage=" + @params.ItemsPerPage.ToString()).Result;
                if (!Res.IsSuccessStatusCode)
                {
                    ErrorViewModel error = new ErrorViewModel
                    {
                        errorCode = Res.StatusCode.ToString(),
                        errorMessage = Res.ReasonPhrase
                    };
                    return View("error", error);
                }

                products = JsonConvert.DeserializeObject<List<Product>>(Res.Content.ReadAsStringAsync().Result);
                pagination = JsonConvert.DeserializeObject<PaginationMetadata>(Res.Headers.GetValues("X-Pagination").FirstOrDefault());
                categories.Add(new SelectListItem
                {
                    Text = "All",
                    Value = "",
                    Selected = true
                });
            }
            else
            {
                var Res = GetData("api/products?page=" + @params.Page.ToString() + "&itemsperpage=" + @params.ItemsPerPage.ToString()).Result;
                if (!Res.IsSuccessStatusCode)
                {
                    ErrorViewModel error = new ErrorViewModel
                    {
                        errorCode = Res.StatusCode.ToString(),
                        errorMessage = Res.ReasonPhrase
                    };
                    return View("error", error);
                }

                products = JsonConvert.DeserializeObject<List<Product>>(Res.Content.ReadAsStringAsync().Result);
                pagination = JsonConvert.DeserializeObject<PaginationMetadata>(Res.Headers.GetValues("X-Pagination").FirstOrDefault());
                categories.Add(new SelectListItem
                {
                    Text = "All",
                    Value = "",
                    Selected = true
                });
            }

            var data = JsonConvert.DeserializeObject<List<SelectListItem>>(GetData("api/categories/selectlist").Result.Content.ReadAsStringAsync().Result);
            foreach (var item in data)
            {
                item.Value = item.Text;
                if (!string.IsNullOrEmpty(filter) && item.Value == filter)
                    item.Selected = true;

                categories.Add(item);
            }

            ViewData["pagination"] = pagination;
            ViewData["categories"] = categories;
            return View(products);
        }

        [HttpPost("/admin/products/import")]
        public IActionResult ImportProduct([FromForm] IFormFile excel)
        {
            if (excel != null && excel.Length > 0)
            {
                List<Product> products = new List<Product>();

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                var fileName = Path.GetFileName(excel.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/excel", fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                excel.CopyTo(stream);

                stream.Position = 0;
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            ProductName = reader.GetValue(0).ToString(),
                            CategoryId = Int32.Parse(reader.GetValue(1).ToString()),
                            QuantityPerUnit = reader.GetValue(2).ToString(),
                            UnitPrice = Decimal.Parse(reader.GetValue(3).ToString()),
                            UnitsInStock = short.Parse(reader.GetValue(4).ToString()),
                            UnitsOnOrder = short.Parse(reader.GetValue(5).ToString()),
                        });
                    }
                }

                var Res = PostData("api/products/import", JsonConvert.SerializeObject(products));

                if (!Res.Result.IsSuccessStatusCode)
                    return StatusCode(StatusCodes.Status500InternalServerError);

            }
            return Ok();
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
