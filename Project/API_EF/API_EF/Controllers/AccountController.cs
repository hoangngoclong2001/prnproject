using API_EF.Dtos;
using API_EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace API_EF.Controllers
{
    [Authorize(Roles = "1")]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        public PRN231DBContext dBContext;
        public AccountController(PRN231DBContext _dBContext)
        {
            dBContext = _dBContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> Get()
        {
            var E = await dBContext.Accounts.ToListAsync();
            if (E == null)
                return NoContent();
            
            return Ok(E);
        }

        [HttpGet]
        [Route("profile/email/{email}")]
        public async Task<ActionResult<IEnumerable<Account>>> Get(string email)
        {
            var E = await dBContext.Accounts.Where(a => a.Email == email).ToListAsync();
            if (E == null)
                return NoContent();

            return Ok(E);
        }

        [HttpGet]
        [Route("profile/customer/{customerid}")]
        public async Task<ActionResult<IEnumerable<Customer>>> Get(string id, int i)
        {
            var E = await dBContext.Customers.Where(a => a.CustomerId == id).ToListAsync();
            if (E == null)
                return NoContent();

            return Ok(E);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<ActionResult<Account>> Get(int id)
        {
            var E = await dBContext.Accounts.Where(a => a.AccountId == id).FirstOrDefaultAsync();
            if (E == null)
                return NoContent();

            return Ok(E);
        }

        [HttpPost]
        public async Task<ActionResult<Account>> Create(Account account)
        {
            await dBContext.Accounts.AddAsync(account);
            await dBContext.SaveChangesAsync();

            return Ok(account);
        }
    }
}
