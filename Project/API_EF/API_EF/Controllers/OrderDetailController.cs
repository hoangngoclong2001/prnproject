using API_EF.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace API_EF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        public PRN231DBContext dBContext;
        public OrderDetailController(PRN231DBContext dBContext)
        {
            this.dBContext = dBContext;
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> Get(int? id)
        {
            if (id == null)
                return BadRequest();

            var OD = await dBContext.OrderDetails.Where(od => od.OrderId == id).Include(p => p.Product).ToListAsync();
            if (OD == null)
                return NotFound();

            return Ok(OD);
        }

        [HttpGet]
        [Route("hot")]
        public async Task<IActionResult> Hot()
        {
            var pendingOrders = await dBContext.Orders.Where(o => o.ShippedDate == null).ToListAsync();
            List<OrderDetail> pendingOrderDetail = new List<OrderDetail>();
            List<Product> hot = new List<Product>();

            foreach (var order in pendingOrders)
            {
                var orderdetail = dBContext.OrderDetails.Where(o => o.OrderId == order.OrderId).GroupBy(p => p.ProductId).Select(od => new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = od.Key,
                    Quantity = (short)od.Sum(q => q.Quantity)
                }).OrderByDescending(q => q.Quantity).ToList();

                foreach(var od in orderdetail)
                {
                    pendingOrderDetail.Add(od);
                }
            }

            foreach(var od in pendingOrderDetail)
            {
                var p = await dBContext.Products.Where(p => p.ProductId == od.ProductId).FirstOrDefaultAsync();
                hot.Add(p);
            }

            return Ok(hot.Take(5));
        }

        [HttpGet]
        [Route("bestsale")]
        public async Task<IActionResult> BestSale()
        {
            List<Product> bestsale = new List<Product>();

            var soldOrderDetail = dBContext.OrderDetails.GroupBy(p => p.ProductId).Select(od => new OrderDetail
            {
                ProductId = od.Key,
                Quantity = (short)od.Sum(q => q.Quantity)
            }).OrderByDescending(q => q.Quantity).Take(5).ToList();
            
            foreach(var od in soldOrderDetail)
            {
                var p = await dBContext.Products.Where(p => p.ProductId == od.ProductId).FirstOrDefaultAsync();
                bestsale.Add(p);
            }

            return Ok(bestsale);
        }
    }
}
