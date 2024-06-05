using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportServer.Data;

namespace SportServer.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("[controller]")]
    public class PlansController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public PlansController(ApplicationDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var plans = await _dbContext.Plans.ToListAsync();
            return View(plans);
        }
    }
}
