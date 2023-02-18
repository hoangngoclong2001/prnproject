using API_EF.Dtos;
using API_EF.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace API_EF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : Controller
    {
        public PRN231DBContext dBContext;
        public CustomersController(PRN231DBContext _dBContext)
        {
            dBContext = _dBContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PaginationParams @params)
        {
            var P = dBContext.Customers;
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
            {
                return Ok(items);
            }
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var C = await dBContext.Customers.Where(p => p.CustomerId == id).FirstOrDefaultAsync();
            if (C == null)
                return NotFound();
            else
            {
                return Ok(C);
            }
        }

        [HttpGet]
        [Route("search/{search}")]
        public async Task<IActionResult> Search(string search, [FromQuery] PaginationParams @params)
        {
            if (string.IsNullOrEmpty(search))
                return BadRequest();

            var P = dBContext.Customers.Where(p => p.ContactName.Contains(search));
            if (P == null)
                return NotFound();

            var paginationMetadata = new PaginationMetadata(P.Count(), @params.Page, @params.ItemsPerPage);
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationMetadata));
            var items = await P.Skip((@params.Page - 1) * @params.ItemsPerPage)
                               .Take(@params.ItemsPerPage)
                               .ToListAsync();

            if (items == null)
                return NotFound();
            {
                return Ok(items);
            }
        }

        [Authorize(Roles = "1")]
        [HttpPost("create")]
        public async Task<IActionResult> Post(Customer C)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            else
            {
                try
                {
                    await dBContext.AddAsync<Customer>(C);
                    await dBContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return Problem(detail: ex.StackTrace, title: ex.Message);
                }
                return Json(C);
            }
        }

        [Authorize(Roles = "1")]
        [HttpDelete("remove/{customerId}")]
        public async Task<IActionResult> Delete(string? customerId)
        {
            if (customerId == null)
                return NotFound();
            else
            {
                try
                {
                    var C = await dBContext.Customers.Where(c => c.CustomerId == customerId).Include(c => c.Orders).FirstOrDefaultAsync();
                    if (C == null)
                        return NotFound();

                    if (C.Orders.Count != 0)
                        return NoContent();

                    dBContext.Remove<Customer>(C);
                    await dBContext.SaveChangesAsync();
                    return Json(C);
                }
                catch (Exception ex)
                {
                    return Problem(detail: ex.StackTrace, title: ex.Message);
                }
            }
        }
    }
}
