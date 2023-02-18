using API_EF.Dtos;
using API_EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Data;

namespace API_EF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        public PRN231DBContext dBContext;
        public OrdersController(PRN231DBContext dBContext)
        {
            this.dBContext = dBContext;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> New(Order order)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            try
            {
                await dBContext.Orders.AddAsync(order);
                await dBContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }

            Order o = await dBContext.Orders.Where(o => o.OrderId == order.OrderId).Include(c => c.Customer).Include(a => a.Customer.Accounts).Include(od => od.OrderDetails).FirstOrDefaultAsync();

            foreach(var item in o.OrderDetails)
            {
                Product p = await dBContext.Products.Where(p => p.ProductId == item.ProductId).FirstOrDefaultAsync();
                item.Product = p;
            }

            return Ok(o);
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PaginationParams @params)
        {
            var O = dBContext.Orders.Include(o => o.Customer).Include(o => o.Employee);
            if (O == null)
                return NotFound();

            var paginationMetadata = new PaginationMetadata(O.Count(), @params.Page, @params.ItemsPerPage);
            Response.Headers.Add("Access-Control-Expose-Headers", "X-Pagination");
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));
            var items = await O.Skip((@params.Page - 1) * @params.ItemsPerPage)
                               .Take(@params.ItemsPerPage)
                               .ToListAsync();

            if (items == null)
                return NotFound();
            else
                return Ok(items);
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        [Route("filter/{filter}")]
        public async Task<IActionResult> Filter(string filter, [FromQuery] PaginationParams @params)
        {
            if (string.IsNullOrEmpty(filter))
                return BadRequest();

            DateTime?[] filters = new DateTime?[2];
            if (filter.Contains('_'))
            {
                filters[0] = DateTime.Parse(filter.Split('_')[0]);
                filters[1] = DateTime.Parse(filter.Split('_')[1]);
            }

            var O = dBContext.Orders.Where(p => p.OrderDate >= filters[0] && p.OrderDate <= filters[1]).Include(o => o.Customer).Include(o => o.Employee);
            if (O == null)
                return NotFound();

            var paginationMetadata = new PaginationMetadata(O.Count(), @params.Page, @params.ItemsPerPage);
            Response.Headers.Add("Access-Control-Expose-Headers", "X-Pagination");
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));
            var items = await O.Skip((@params.Page - 1) * @params.ItemsPerPage)
                               .Take(@params.ItemsPerPage)
                               .ToListAsync();

            if (items == null)
                return NotFound();
            else
            {
                return Ok(items);
            }
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        [Route("byemployee/{name}")]
        public async Task<IActionResult> ByEmployee(string name, [FromQuery] PaginationParams @params)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest();

            var O = dBContext.Orders.Where(p => p.Employee.LastName == name).Include(o => o.Customer).Include(o => o.Employee);
            if (O == null)
                return NotFound();

            var paginationMetadata = new PaginationMetadata(O.Count(), @params.Page, @params.ItemsPerPage);
            Response.Headers.Add("Access-Control-Expose-Headers", "X-Pagination");
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));
            var items = await O.Skip((@params.Page - 1) * @params.ItemsPerPage)
                               .Take(@params.ItemsPerPage)
                               .ToListAsync();

            if (items == null)
                return NotFound();
            else
            {
                return Ok(items);
            }
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        [Route("cancelorder/{id:int}")]
        public async Task<IActionResult> CancelOrder(int? id)
        {
            if (id == null)
                return BadRequest();

            var O = await dBContext.Orders.Where(p => p.OrderId == id).Include(o => o.Customer).Include(o => o.Employee).FirstOrDefaultAsync();
            if (O == null)
                return NotFound();

            O.RequiredDate = null;
            dBContext.Update(O);
            await dBContext.SaveChangesAsync();

            return Ok();
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> Get(int? id)
        {
            if (id == null)
                return BadRequest();

            var OD = await dBContext.Orders.Where(od => od.OrderId == id).Include(o => o.Customer).Include(o => o.Employee).FirstOrDefaultAsync();
            if (OD == null)
                return NotFound();

            return Ok(OD);
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        [Route("minmax")]
        public async Task<IActionResult> Get()
        {
            DateTime?[] minmax = new DateTime?[2];

            var MinDate = dBContext.Orders.Select(o => o.OrderDate).Min();
            var MaxDate = dBContext.Orders.Select(o => o.OrderDate).Max();

            minmax[0] = MinDate;
            minmax[1] = MaxDate;

            return Ok(minmax);
        }
    }
}
