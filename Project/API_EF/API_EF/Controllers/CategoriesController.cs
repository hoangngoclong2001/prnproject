using API_EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace API_EF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : Controller
    {
        public PRN231DBContext dBContext;
        public CategoriesController(PRN231DBContext dBContext)
        {
            this.dBContext = dBContext;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var C = await dBContext.Categories.ToListAsync();
            if (C == null)
                return NotFound();
            else
            {
                return Ok(C);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> Get(int? id)
        {
            if (id == null)
                return BadRequest();

            var C = await dBContext.Categories.Where(c => c.CategoryId == id).FirstOrDefaultAsync();
            if (C == null)
                return NotFound();

            return Ok(C);
        }

        [HttpGet]
        [Route("allCategoryName")]
        public async Task<IActionResult> AllCategory()
        {
            List<string> allCategory = await dBContext.Categories.Select(c => c.CategoryName).ToListAsync();
            if (allCategory == null)
                return NotFound();

            return Ok(allCategory);
        }

        [HttpGet]
        [Route("selectlist")]
        public List<SelectListItem> GetSelectList()
        {
            List<SelectListItem> categories = (from p in dBContext.Categories.AsEnumerable()
                                               select new SelectListItem
                                               {
                                                   Text = p.CategoryName,
                                                   Value = p.CategoryId.ToString()
                                               }).ToList();
            return categories;
        }
    }
}
