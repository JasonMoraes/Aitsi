using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Controllers;

public class FacebookLoginDto
{
    public string AccessToken { get; set; } = string.Empty;
}

public class GoogleLoginDto
{
    public string Credential { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    private string GenerateJwt(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? "DomyslnyKluczJWT-minimum-32-znaki!!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.DisplayName),
                new Claim(ClaimTypes.Role, user.Role.ToString().ToLower())
            },
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private object UserResponse(User user) => new
    {
        user.Id,
        user.DisplayName,
        user.AvatarUrl,
        user.Role,
        HasFacebook = user.FacebookId != null,
        HasGoogle = user.GoogleId != null
    };

    [HttpPost("facebook")]
    public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginDto dto)
    {
        var httpClient = new HttpClient();
        var fbJson = await httpClient.GetStringAsync(
            $"https://graph.facebook.com/me?fields=id,name,picture&access_token={dto.AccessToken}");

        var fbData = JsonDocument.Parse(fbJson).RootElement;

        if (!fbData.TryGetProperty("id", out var idProp))
            return BadRequest("Invalid Facebook token");

        var facebookId = idProp.GetString()!;
        var name = fbData.GetProperty("name").GetString() ?? "User";
        var avatarUrl = fbData.GetProperty("picture").GetProperty("data").GetProperty("url").GetString();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.FacebookId == facebookId);
        if (user == null)
        {
            user = new User
            {
                FacebookId = facebookId,
                DisplayName = name,
                AvatarUrl = avatarUrl,
                Role = UserRole.Creator
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        else
        {
            if (user.IsBlocked)
                return Unauthorized(new { message = "Konto zostało zablokowane. Powód: " + (user.BlockReason ?? "brak podanego powodu") });

            user.AvatarUrl = avatarUrl;
            user.DisplayName = name;
            await _db.SaveChangesAsync();
        }

        return Ok(new { token = GenerateJwt(user), user = UserResponse(user) });
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Credential))
            return BadRequest("Invalid Google credential");

        // Verify the ID token using Google's tokeninfo endpoint.
        // This validates the signature, expiry and issuer server-side.
        string googleId, name, email, avatarUrl;
        try
        {
            using var httpClient = new HttpClient();
            var tokenInfoResponse = await httpClient.GetAsync(
                $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(dto.Credential)}");

            if (!tokenInfoResponse.IsSuccessStatusCode)
                return Unauthorized("Invalid Google credential");

            var tokenInfoJson = await tokenInfoResponse.Content.ReadAsStringAsync();
            var tokenInfo = JsonDocument.Parse(tokenInfoJson).RootElement;

            // Verify audience matches our configured client ID (if set)
            var configuredClientId = _config["Google:ClientId"];
            if (!string.IsNullOrEmpty(configuredClientId))
            {
                var aud = tokenInfo.TryGetProperty("aud", out var audProp) ? audProp.GetString() : null;
                if (aud != configuredClientId)
                    return Unauthorized("Token audience mismatch");
            }

            googleId = tokenInfo.TryGetProperty("sub", out var subProp)
                ? subProp.GetString() ?? string.Empty : string.Empty;
            name = tokenInfo.TryGetProperty("name", out var nameProp)
                ? nameProp.GetString() ?? "User" : "User";
            email = tokenInfo.TryGetProperty("email", out var emailProp)
                ? emailProp.GetString() ?? string.Empty : string.Empty;
            avatarUrl = tokenInfo.TryGetProperty("picture", out var picProp)
                ? picProp.GetString() ?? string.Empty : string.Empty;
        }
        catch
        {
            return Unauthorized("Google credential validation failed");
        }

        if (string.IsNullOrEmpty(googleId))
            return BadRequest("Invalid Google credential");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.GoogleId == googleId);
        if (user == null)
        {
            user = new User
            {
                GoogleId = googleId,
                DisplayName = name,
                Email = email ?? string.Empty,
                AvatarUrl = avatarUrl,
                Role = UserRole.Creator
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }
        else
        {
            if (user.IsBlocked)
                return Unauthorized(new { message = "Konto zostało zablokowane. Powód: " + (user.BlockReason ?? "brak podanego powodu") });

            user.AvatarUrl = avatarUrl;
            user.DisplayName = name;
            if (!string.IsNullOrEmpty(email)) user.Email = email;
            await _db.SaveChangesAsync();
        }

        return Ok(new { token = GenerateJwt(user), user = UserResponse(user) });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return NotFound();

        // Re-check block status on every request — existing tokens remain valid
        // after blocking, so we must verify against the database here.
        if (user.IsBlocked)
            return Unauthorized(new { message = "Konto zostało zablokowane. Powód: " + (user.BlockReason ?? "brak podanego powodu") });

        return Ok(UserResponse(user));
    }
}
