using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Portfolyo.Data;
using Portfolyo.Models;
using Portfolyo.Options;
using Portfolyo.Services;

namespace Portfolyo.Areas.Admin.Controllers;

[Area("Admin")]
[AllowAnonymous]
[Route("admin/auth")]
public class AuthController : Controller
{
    private readonly AdminAuthDbContext _db;
    private readonly JwtTokenService _jwtService;
    private readonly AdminAuthOptions _adminAuth;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        AdminAuthDbContext db,
        JwtTokenService jwtService,
        IOptions<AdminAuthOptions> adminAuthOptions,
        IOptions<JwtOptions> jwtOptions)
    {
        _db = db;
        _jwtService = jwtService;
        _adminAuth = adminAuthOptions.Value;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpGet("login")]
    public IActionResult Login() => View();

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        var admin = await _db.AdminUsers.FirstOrDefaultAsync(x => x.Username == username);

        if (admin == null || !BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
        {
            ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
            return View();
        }

        var token = _jwtService.CreateAdminToken(admin);

        Response.Cookies.Append("admin_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !Request.Host.Host.Contains("localhost"),
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.ExpiresMinutes)
        });

        return Redirect("/Dashboard/Index");
    }

    [HttpGet("register")]
    public async Task<IActionResult> Register()
    {
        if (await _db.AdminUsers.AnyAsync())
            return NotFound();

        return View();
    }

    [HttpPost("register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(string adminKey, string username, string password)
    {
        if (await _db.AdminUsers.AnyAsync())
            return NotFound();

        if (string.IsNullOrWhiteSpace(_adminAuth.Key) || adminKey != _adminAuth.Key)
        {
            ViewBag.Error = "Admin Key hatalı.";
            return View();
        }

        if (await _db.AdminUsers.AnyAsync())
        {
            ViewBag.Error = "Register kapalı. Zaten admin mevcut.";
            return View();
        }

        if (await _db.AdminUsers.AnyAsync(x => x.Username == username))
        {
            ViewBag.Error = "Bu kullanıcı adı zaten var.";
            return View();
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        _db.AdminUsers.Add(new AdminUser
        {
            Username = username,
            PasswordHash = hash,
            CreatedAtUtc = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Redirect("/admin/auth/login");
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("admin_token");
        return Redirect("/admin/auth/login");
    }
}