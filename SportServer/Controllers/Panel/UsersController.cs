using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportServer.Data;
using SportServer.Models.Viewmodels;

namespace SportServer.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        private readonly SignInManager<AppUser> _signInManager;

        public UsersController(UserManager<AppUser> userManager, 
            SignInManager<AppUser> signInManager) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewmodel loginViewmodel)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var user = await _userManager.FindByEmailAsync(loginViewmodel.Email);
            if (user != null)
            {
                var roleResult = await _userManager.IsInRoleAsync(user, "admin");
                if (!roleResult)
                {
                    ModelState.AddModelError(string.Empty, "Доступ запрещен");
                    return View();
                }
            }
            var result = await _signInManager.PasswordSignInAsync(loginViewmodel.Email, loginViewmodel.Password, true, false);
            if (result.Succeeded)
            {
                return RedirectToAction("List", "Exercices");
            }
            ModelState.AddModelError(string.Empty, "Неправильный логин или пароль");
            return View();
        }

        
    }
}
