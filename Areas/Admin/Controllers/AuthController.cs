using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;
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
        if (!await _db.AdminUsers.AnyAsync())
        {
            TempData["Success"] = "Henüz admin hesabı yok. İlk kurulum için kayıt ekranını kullan.";
            return Redirect("/admin/auth/register");
        }

        var normalizedUsername = (username ?? string.Empty).Trim().ToLower();
        var admin = await _db.AdminUsers
            .FirstOrDefaultAsync(x => x.Username.ToLower() == normalizedUsername);

        if (admin == null || !BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
        {
            ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
            return View();
        }

        var token = _jwtService.CreateAdminToken(admin);
        var isLoopbackHost = IsLoopbackHost(Request.Host.Host);

        Response.Cookies.Append("admin_token", token, new CookieOptions
        {
            HttpOnly = true,
            // Local development over HTTP needs non-secure cookie (localhost/127.0.0.1/::1).
            Secure = !isLoopbackHost,
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
        var normalizedAdminKey = (adminKey ?? string.Empty).Trim();
        var normalizedUsername = (username ?? string.Empty).Trim();

        if (await _db.AdminUsers.AnyAsync())
            return NotFound();

        if (string.IsNullOrWhiteSpace(_adminAuth.Key) || normalizedAdminKey != _adminAuth.Key.Trim())
        {
            ViewBag.Error = "Admin Key hatalı.";
            return View();
        }

        if (await _db.AdminUsers.AnyAsync())
        {
            ViewBag.Error = "Register kapalı. Zaten admin mevcut.";
            return View();
        }

        if (await _db.AdminUsers.AnyAsync(x => x.Username.ToLower() == normalizedUsername.ToLower()))
        {
            ViewBag.Error = "Bu kullanıcı adı zaten var.";
            return View();
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        _db.AdminUsers.Add(new AdminUser
        {
            Username = normalizedUsername,
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

    [HttpGet("forgot-password")]
    public IActionResult ForgotPassword() => View();

    [HttpPost("forgot-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string adminKey, string username, string newPassword, string newPasswordConfirm)
    {
        var normalizedAdminKey = (adminKey ?? string.Empty).Trim();
        var normalizedUsername = (username ?? string.Empty).Trim().ToLower();

        if (string.IsNullOrWhiteSpace(_adminAuth.Key) || normalizedAdminKey != _adminAuth.Key.Trim())
        {
            ViewBag.Error = "Admin Key hatalı.";
            return View();
        }

        if (string.IsNullOrWhiteSpace(normalizedUsername) || string.IsNullOrWhiteSpace(newPassword))
        {
            ViewBag.Error = "Kullanıcı adı ve yeni şifre zorunludur.";
            return View();
        }

        if (newPassword.Length < 6)
        {
            ViewBag.Error = "Yeni şifre en az 6 karakter olmalı.";
            return View();
        }

        if (!string.Equals(newPassword, newPasswordConfirm, StringComparison.Ordinal))
        {
            ViewBag.Error = "Yeni şifreler eşleşmiyor.";
            return View();
        }

        var admin = await _db.AdminUsers
            .FirstOrDefaultAsync(x => x.Username.ToLower() == normalizedUsername);
        if (admin == null)
        {
            ViewBag.Error = "Kullanıcı bulunamadı.";
            return View();
        }

        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Şifre başarıyla güncellendi. Yeni şifreyle giriş yapabilirsin.";
        return Redirect("/admin/auth/login");
    }

    private static bool IsLoopbackHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return false;
        }

        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (IPAddress.TryParse(host, out var ip))
        {
            return IPAddress.IsLoopback(ip);
        }

        return false;
    }
}
