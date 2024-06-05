using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportServer.Data;
using SportServer.Models;

namespace SportServer.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("[controller]")]
    public class ExercicesController : Controller
    {
        private readonly ApplicationDbContext _dbContext; 

        public ExercicesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("/")]
        public async Task<IActionResult> List()
        {
            var exercises = await _dbContext.ExerciseParts.Include(x => x.Exercise)
                .ToListAsync();
            return View(exercises);
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var exercisePart = await _dbContext.ExerciseParts.Include(x => x.Exercise)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (exercisePart == null)
            {
                return RedirectToAction("List");
            }
            return View(exercisePart);
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add(ExercisePart exercisePart)
        {
            await _dbContext.ExerciseParts.AddAsync(exercisePart);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("List");
        }

        [HttpPost("edit")]
        public async Task<IActionResult> Edit(ExercisePart exercisePart)
        {
            _dbContext.ExerciseParts.Update(exercisePart);
            await _dbContext.SaveChangesAsync();
            return RedirectToAction("List");
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var exercisePart = await _dbContext.ExerciseParts.Include(x => x.Exercise).FirstOrDefaultAsync(x => x.Id == id);
            if (exercisePart == null)
            {
                return NotFound("Не найдено");
            }
            _dbContext.ExerciseParts.Remove(exercisePart);
            _dbContext.Exercises.Remove(exercisePart.Exercise);
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}
