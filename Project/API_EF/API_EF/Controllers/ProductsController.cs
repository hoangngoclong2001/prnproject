using API_EF.Dtos;
using API_EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using System.Xml.Serialization;

namespace API_EF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : Controller
    {
        public PRN231DBContext dBContext;
        public ProductsController(PRN231DBContext dBContext)
        {
            this.dBContext = dBContext;
        }
        public int count = 0;
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PaginationParams @params)
        {
            var P = dBContext.Products.Include(p => p.Category);
            if (P == null)
                return NotFound();

            var paginationMetadata = new PaginationMetadata(P.Count(), @params.Page, @params.ItemsPerPage);
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));
            var items = await P.Skip((@params.Page - 1) * @params.ItemsPerPage)
                               .Take(@params.ItemsPerPage)
                               .ToListAsync();

            if (items == null)
                return NotFound();
            else
                return Ok(items);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> Get(int? id)
        {
            if (id == null)
                return BadRequest();

            var P = await dBContext.Products.Where(p => p.ProductId == id).Include(p => p.Category).FirstOrDefaultAsync();
            if (P == null)
                return NotFound();
            else
                return Ok(P);
        }

        [HttpGet]
        [Route("{name}")]
        public async Task<IActionResult> Get(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest();

            var P = await dBContext.Products.Where(p => p.ProductName == name).Include(p => p.Category).FirstOrDefaultAsync();
            if (P == null)
                return NotFound();
            else
                return Ok(P);
        }

        [HttpGet]
        [Route("new")]
        public async Task<IActionResult> New()
        {
            var newP = await dBContext.Products.OrderByDescending(p => p.ProductId).Take(5).ToListAsync();
            if (newP == null)
                return NotFound();

            return Ok(newP);
        }

        [HttpGet]
        [Route("filter/{filter}")]
        public async Task<IActionResult> Filter(string filter, [FromQuery] PaginationParams @params)
        {
            if (string.IsNullOrEmpty(filter))
                return BadRequest();

            if (filter.Contains('-'))
                filter = filter.Replace('-', '/');

            var P = dBContext.Products.Where(p => p.Category.CategoryName == filter).Include(p => p.Category);
            if (P == null)
                return NotFound();

            var paginationMetadata = new PaginationMetadata(P.Count(), @params.Page, @params.ItemsPerPage);
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));
            var items = await P.Skip((@params.Page - 1) * @params.ItemsPerPage)
                               .Take(@params.ItemsPerPage)
                               .ToListAsync();

            if (items == null)
                return NotFound();
            else
                return Ok(items);
        }

        [HttpGet]
        [Route("search/{search}")]
        public async Task<IActionResult> Search(string search, [FromQuery] PaginationParams @params)
        {
            if (string.IsNullOrEmpty(search))
                return BadRequest();

            var P = dBContext.Products.Where(p => p.ProductName.Contains(search)).Include(p => p.Category);
            if (P == null)
                return NotFound();

            var paginationMetadata = new PaginationMetadata(P.Count(), @params.Page, @params.ItemsPerPage);
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));
            var items = await P.Skip((@params.Page - 1) * @params.ItemsPerPage)
                               .Take(@params.ItemsPerPage)
                               .ToListAsync();

            if (items == null)
                return NotFound();
            else
                return Ok(items);
        }

        [HttpGet]
        [Route("searchbycate/{search}/{category}")]
        public async Task<IActionResult> SearchCate(string search,string category)
        {   
            if (string.IsNullOrEmpty(search))
                return BadRequest();
            var cate = dBContext.Categories.Where(c => c.CategoryName == category).FirstOrDefault();
            var searchpros = dBContext.Products.Where(p => p.CategoryId == cate.CategoryId);
            var pros = dBContext.Products.Where(p => p.CategoryId == cate.CategoryId).ToListAsync();
            if (search != null && search != "")
            {
                searchpros = dBContext.Products.Where(p => p.ProductName.Contains(search) && p.CategoryId == cate.CategoryId).Include(c => c.Category);
                if(searchpros != null){ 
                pros = searchpros.ToListAsync();
                }
            }

            if (pros == null)
                return NotFound();
            else
                return Ok(pros);
        }

        [HttpPost]
        [Route("import")]
        public async Task<IActionResult> Import(List<Product> products)
        {
            if (products.Count <= 0 || products == null)
                return BadRequest();

            try
            {
                await dBContext.Products.AddRangeAsync(products);
                await dBContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict();
            }

            return Ok();
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateProduct(Product P)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                P.Category = null;
                await dBContext.AddAsync<Product>(P);
                await dBContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.StackTrace, title: ex.Message);
            }
            return Ok(P);
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        [Route("edit")]
        public async Task<IActionResult> EditProduct(Product P)
        {
            if (P == null)
                return BadRequest();

            var existed = await dBContext.Products.Where(p => p.ProductId == P.ProductId).Include(p => p.Category).FirstOrDefaultAsync();
            if (existed == null)
                return NotFound();

            existed.ProductName = P.ProductName;
            existed.UnitPrice = P.UnitPrice;
            existed.QuantityPerUnit = P.QuantityPerUnit;
            existed.UnitsInStock = P.UnitsInStock;
            existed.CategoryId = P.CategoryId;
            existed.ReorderLevel = P.ReorderLevel;
            existed.UnitsOnOrder = P.UnitsOnOrder;
            existed.Discontinued = P.Discontinued;

            try
            {
                dBContext.Update<Product>(existed);
                dBContext.SaveChanges();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.StackTrace, title: ex.Message);
            }

            return Ok(existed);
        }


        [Authorize(Roles = "1")]
        [HttpGet]
        [Route("delete/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (id == null)
                return BadRequest();
            var product = await dBContext.Products.Where(p => p.ProductId == id).FirstOrDefaultAsync();
            if (product != null)
            {
                var pendingOrders = dBContext.Orders.ToList();
                List<OrderDetail> pendingOrderDetail = new List<OrderDetail>();

                foreach (var order in pendingOrders)
                {
                    var orderdetail = dBContext.OrderDetails.Where(o => o.OrderId == order.OrderId && o.ProductId == id).ToList();

                    foreach (var od in orderdetail)
                    {
                        pendingOrderDetail.Add(od);
                    }
                }

                foreach (var od in pendingOrderDetail)
                {
                    count++;
                }
                if (count == 0)
                {
                    product.Discontinued = false;
                }
            }
            try
            {
                dBContext.Update(product);
                await dBContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.StackTrace, title: ex.Message);
            }

            return Ok(count);
        }

        [HttpGet]
        [Route("bycategory/{category}")]
        public async Task<IActionResult> GetProductByCategory(string category)
        {
            if (string.IsNullOrEmpty(category))
                return BadRequest();

            if (category.Contains(' '))
                category = category.Replace(' ', '/');

            var c = await dBContext.Categories.Where(c => c.CategoryName == category).FirstOrDefaultAsync();
            var P = await dBContext.Products.Where(p => p.CategoryId == c.CategoryId).ToListAsync();

            if (P == null)
                return NotFound();

            return Ok(P);
        }

        [HttpGet]
        [Route("allProductName")]
        public async Task<IActionResult> GetAllProductName()
        {
            List<string> allProductName = await dBContext.Products.Select(c => c.ProductName).ToListAsync();
            if (allProductName == null)
                return NotFound();

            return Ok(allProductName);
        }
    }
}
