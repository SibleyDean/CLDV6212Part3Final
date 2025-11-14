using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABCRetailers.Data;
using ABCRetailers.Models;
using ABCRetailers.Models.ViewModels;

namespace ABCRetailers.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginVM { ReturnUrl = returnUrl });
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Remove IsActive check since column doesn't exist
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.UserName == vm.Username);

            // Use PasswordHash instead of Password
            if (user == null || user.PasswordHash != vm.Password)
            {
                ModelState.AddModelError("", "Invalid username or password");
                return View(vm);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View(new RegisterVM());
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Check if username already exists
            if (await _db.Users.AnyAsync(u => u.UserName == vm.Username))
            {
                ModelState.AddModelError("Username", "Username is already taken");
                return View(vm);
            }

            // Check if email already exists
            if (await _db.Users.AnyAsync(u => u.Email == vm.Email))
            {
                ModelState.AddModelError("Email", "Email is already registered");
                return View(vm);
            }

            var user = new User
            {
                UserName = vm.Username,
                Email = vm.Email,
                PasswordHash = vm.Password, // Use PasswordHash instead of Password
                Role = "Customer",
                CreatedAt = DateTime.UtcNow
                // Remove IsActive since column doesn't exist
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Auto-login after registration
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}