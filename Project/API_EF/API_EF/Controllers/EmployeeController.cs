using API_EF.Dtos;
using API_EF.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace API_EF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : Controller
    {
        public PRN231DBContext dBContext;
        public EmployeeController(PRN231DBContext dBContext)
        {
            this.dBContext = dBContext;
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var E = await dBContext.Employees.ToListAsync();
            var EDto = from e in E
                       select new EmployeeDto()
                       {
                           EmployeeId = e.EmployeeId,
                           LastName = e.LastName,
                           FirstName = e.FirstName,
                           Title = e.Title,
                           TitleOfCourtesy = e.TitleOfCourtesy,
                           BirthDate = e.BirthDate,
                           Address = e.Address
                       };
            if (EDto != null)
            {
                return Json(EDto);
            }
            else
            {
                return NoContent();
            }
        }

        [Authorize]
        [HttpGet]
        [Route("{id}")]
        public IActionResult Get(int? id)
        {
            if (id == null)
                return BadRequest();
            else
            {
                var E = dBContext.Employees.Where(e => e.EmployeeId == id).FirstOrDefault();
                if (E == null)
                    return NotFound();
                else
                {
                    var emp = new EmployeeDto()
                    {
                        EmployeeId = E.EmployeeId,
                        FirstName = E.FirstName,
                        LastName = E.LastName,
                        Title = E.Title,
                        TitleOfCourtesy = E.TitleOfCourtesy,
                        BirthDate = E.BirthDate,
                        Address = E.Address
                    };
                    return Json(emp);
                }
            }
        }

        [HttpGet("employeeNames")]
        public async Task<ActionResult<List<string>>> EmpNames()
        {
            List<string> empNames = await dBContext.Employees.Select(e => e.LastName).ToListAsync();

            return Ok(empNames);
        }

        [HttpGet("lastName/{lastName}")]
        public IActionResult Get(string? lastName)
        {
            if (lastName == null)
                return BadRequest();
            else
            {
                var E = dBContext.Employees.Where(e => e.LastName == lastName).FirstOrDefault();
                if (E == null)
                    return NotFound();
                else
                {
                    var emp = new EmployeeDto()
                    {
                        FirstName = E.FirstName,
                        LastName = E.LastName,
                        Title = E.Title,
                        TitleOfCourtesy = E.TitleOfCourtesy,
                        BirthDate = E.BirthDate,
                        Address = E.Address
                    };
                    return Json(emp);
                }
            }
        }

        [HttpPost]
        public IActionResult Post(EmployeeDto E)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            else
            {
                var emp = new Employee
                {
                    LastName = E.LastName,
                    FirstName = E.FirstName,
                    Title = E.Title,
                    TitleOfCourtesy = E.TitleOfCourtesy,
                    BirthDate = E.BirthDate,
                    Address = E.Address
                };
                try
                {

                    dBContext.Add<Employee>(emp);
                    dBContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    return Problem(detail: ex.StackTrace, title: ex.Message);
                }
                return Json(emp);
            }
        }

        [HttpPut]
        public IActionResult Put(EmployeeDto E)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            else
            {
                var emp = dBContext.Employees.Where(e => e.EmployeeId == E.EmployeeId).FirstOrDefault();
                if (emp == null)
                    return NotFound();
                else
                {
                    emp.LastName = E.LastName;
                    emp.FirstName = E.FirstName;
                    emp.Title = E.Title;
                    emp.TitleOfCourtesy = E.TitleOfCourtesy;
                    emp.BirthDate = E.BirthDate;
                    emp.Address = E.Address;
                    try
                    {
                        dBContext.Update<Employee>(emp);
                        dBContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        return Problem(detail: ex.StackTrace, title: ex.Message);
                    }
                    return Json(emp);
                }
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
                return BadRequest();
            else
            {
                try
                {
                    var E = dBContext.Employees.Where(e => e.EmployeeId == id).FirstOrDefault();
                    if (E == null)
                        return NotFound();
                    else
                    {
                        dBContext.Remove<Employee>(E);
                        dBContext.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    return Problem(detail: ex.StackTrace, title: ex.Message);
                }
                return Ok("Deleted");
            }
        }
    }
}
