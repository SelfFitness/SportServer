using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportServer.Data;
using SportServer.Models;
using System.Security.Claims;

namespace SportServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "user")]
    [Route("api/[controller]")]
    [ApiController]
    public class PlansApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        private readonly ILogger _logger;

        private readonly UserManager<AppUser> _userManager;

        public PlansApiController(ApplicationDbContext context,
            ILogger<PlansApiController> logger,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var plans = await _context.PlanGroups.Include(x => x.Plans)
                .ThenInclude(x => x.ExerciseParts)
                .ThenInclude(x => x.Exercise)
                .ToListAsync();
            return Ok(plans);
        }

        [HttpPost("savetrain/{planId}")]
        public async Task<IActionResult> SaveTrain(int planId)
        {
            var userId = User.FindFirstValue("Id");
            if (userId == null)
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();
            if (!_context.Plans.Any(x => x.Id == planId))
                return NotFound(planId);
            var trainHistory = new TrainHistory()
            {
                AppUser = user,
                Date = DateTime.UtcNow,
                PlanId = planId
            };
            await _context.TrainHistory.AddAsync(trainHistory);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
