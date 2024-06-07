using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SportServer.Data;
using SportServer.Models;
using SportServer.Models.Viewmodels;
using SportServer.Predictors;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SportServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "user")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserApiController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly UserManager<AppUser> _userManager;

        private readonly ILogger<UserApiController> _logger;

        private readonly JwtOptions _jwtOptions;

        private readonly ApplicationDbContext _appDbContext;

        private readonly IServiceProvider _serviceProvider;

        public UserApiController(RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            ApplicationDbContext context,
            JwtOptions jwtOptions,
            ILogger<UserApiController> logger,
            IServiceProvider serviceProvider) 
        {
            _appDbContext = context;
            _roleManager = roleManager;
            _userManager = userManager;
            _jwtOptions = jwtOptions;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewmodel model)
        {
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user != null)
            {
                return BadRequest(new
                {
                    error = "Пользователь уже зарегистрирован"
                });
            }
            var appUser = new AppUser()
            {
                Email = model.Email,
                UserName = model.Email
            };
            var res = await _userManager.CreateAsync(appUser, model.Password);
            if (!res.Succeeded)
            {
                return BadRequest(new
                {
                    error = res.Errors.FirstOrDefault()
                });
            }
            var result = await _userManager.AddToRoleAsync(appUser, "user");
            if (!result.Succeeded)
            {
                return BadRequest(new 
                {
                    error = result.Errors.FirstOrDefault()
                });
            }
            await _appDbContext.SaveChangesAsync();
            var claims = await CreateClaims(appUser);
            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                expires: DateTime.MaxValue,
                audience: _jwtOptions.Audience,
                claims: claims,
                signingCredentials: new SigningCredentials(_jwtOptions.SymmetricSecurityKey, SecurityAlgorithms.HmacSha256));
            return Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewmodel model)
        {
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null)
                return Unauthorized();
            var isValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isValid)
                return Unauthorized();
            var claims = await CreateClaims(user);

            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                expires: DateTime.MaxValue,
                audience: _jwtOptions.Audience,
                claims: claims,
                signingCredentials: new SigningCredentials(_jwtOptions.SymmetricSecurityKey, SecurityAlgorithms.HmacSha256));
            return Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
        }

        [HttpPost("setparams")]
        public async Task<IActionResult> SetParams(SetUserParamsViewmodel setUserParams)
        {
            var userId = User.FindFirstValue("Id");
            if (userId == null)
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();
            if (setUserParams.Weight != null)
            {
                user.Weigth = setUserParams.Weight.GetValueOrDefault();
                await _appDbContext.WeigthHistory.AddAsync(new WeigthHistory()
                {
                    Date = DateTime.UtcNow,
                    AppUser = user,
                    Weigth = setUserParams.Weight,
                });
            }
            if (setUserParams.Heigth != null)
                user.Heigth = setUserParams.Heigth.GetValueOrDefault();
            await _appDbContext.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("getweigthhistory")]
        public async Task<IActionResult> GetWeigthHistory()
        {
            var userId = User.FindFirstValue("Id");
            if (userId == null)
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();
            var weigthHistory = await _appDbContext.WeigthHistory.Where(x => x.AppUser == user && x.Weigth.HasValue)
                .OrderByDescending(x => x.Date)
                .Take(10)
                .Reverse()
                .ToListAsync();
            return Ok(weigthHistory);
        }

        [HttpGet("predictweigth")]
        public async Task<IActionResult> PredictNextWeigth()
        {
            var predictor = _serviceProvider.GetRequiredService<WeightPredictor>();
            var userId = User.FindFirstValue("Id");
            if (userId == null)
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId);
            var weigthHistory = await _appDbContext.WeigthHistory.Where(x => x.AppUser == user && x.Weigth.HasValue)
                .OrderByDescending(x => x.Date)
                .Take(10)
                .Reverse()
                .ToListAsync();
            if (weigthHistory.Count < 6)
                return Ok();
            var weigthList = weigthHistory.Select(x => x.Weigth.Value).ToList();
            var datesList = weigthHistory.Select(x => x.Date).ToList();
            predictor.Train(weigthList, datesList);
            var dateDelta = new List<TimeSpan>();
            var lastEl = datesList.Count - 1 % 2 == 0 ? datesList.Count - 1 : datesList.Count - 2;
            for (int i = 0; i < lastEl; i += 2)
            {
                var delta = datesList[i] > datesList[i + 1] ? datesList[i] - datesList[i + 1] : datesList[i + 1] - datesList[i];
                dateDelta.Add(delta);
            }
            var avgDelta = dateDelta.Average(x => x.TotalSeconds);
            var predictDate = datesList.Last().AddSeconds(avgDelta);
            var predictWeigth = predictor.Predict(datesList.First(), predictDate);
            return Ok(new WeigthHistory()
            {
                Date = predictDate,
                Weigth = predictWeigth,
            });
        }

        [HttpGet("check")]
        public async Task<IActionResult> Check()
        {
            var userId = User.FindFirstValue("Id");
            if (userId == null)
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();
            return Ok();
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetUserStats()
        {
            var userId = User.FindFirstValue("Id");
            if (userId == null)
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();
            var bmi = user.Heigth <= 0 ? 0 : Math.Round(user.Weigth / Math.Pow(user.Heigth / 100, 2), 2);
            var stats = new UserStats()
            {
                Bmi = bmi,
                Heigth = user.Heigth,
                Weigth = user.Weigth
            };
            return Ok(stats);
        }

        [HttpGet("gettrainhistory")]
        public async Task<IActionResult> GetTrainHistory()
        {
            var userId = User.FindFirstValue("Id");
            if (userId == null)
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();
            var trainHistory = await _appDbContext.TrainHistory.Where(x => x.AppUser == user)
                .ToListAsync();
            return Ok(trainHistory);
        }

        [HttpGet("changepassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewmodel changePasswordVm)
        {
            var userId = User.FindFirstValue("Id");
            if (userId == null)
                return Unauthorized();
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();
            var result = await _userManager.ChangePasswordAsync(user, changePasswordVm.OldPassword, changePasswordVm.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors.FirstOrDefault());
            await _appDbContext.SaveChangesAsync();
            return Ok();
        }

        private async Task<List<Claim>> CreateClaims(AppUser user)
        {
            IdentityOptions _options = new IdentityOptions();
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(_options.ClaimsIdentity.UserIdClaimType, user.Id.ToString()),
                new Claim(_options.ClaimsIdentity.UserNameClaimType, user.UserName),
            };

            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await _roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (var roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim);
                    }
                }
            }
            return claims;
        }
    }
}
