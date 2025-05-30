using System.Security.Claims;
using VidaPositiva.Api.OAuth.Enums;
using VidaPositiva.Api.OAuth.Mappers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace VidaPositiva.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    [HttpGet("signin/{provider}")]
    public IActionResult SignIn(OAuthTenantProviderEnum provider, [FromQuery] string returnUrl)
    {
        var authenticationScheme = OAuthTenantProviderMapper.GetAuthenticationSchemeByProvider(provider);
        
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback), new { provider }),
            Items =
            {
                { "returnUrl", returnUrl }
            }
        };

        return Challenge(properties, authenticationScheme);
    }
    
    [HttpGet("signin/{provider}/callback")]
    public async Task<IActionResult> Callback(OAuthTenantProviderEnum provider)
    {
        var authenticationScheme = OAuthTenantProviderMapper.GetAuthenticationSchemeByProvider(provider);
        
        var authenticateResult = await HttpContext.AuthenticateAsync(authenticationScheme);
    
        if (!authenticateResult.Succeeded || authenticateResult.Properties.Items.All(item => item.Key != "returnUrl"))
            return Unauthorized(new { Message = "Falha na autenticação" });
    
        var claims = authenticateResult.Principal.Claims.ToList();
        var userId = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { Message = "ID de usuário não encontrado" });
    
        var userIdClaim = new Claim("user_id", userId);
        var providerClaim = new Claim("provider", provider.ToString());
        
        claims.Add(userIdClaim);
        claims.Add(providerClaim);
    
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                IsPersistent = true
            });
        
        var refreshToken = await HttpContext.GetTokenAsync("refresh_token");
        
        if (!string.IsNullOrEmpty(refreshToken))
            Response.Cookies.Append("RefreshCookie", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
    
        return Redirect(authenticateResult.Properties.Items["returnUrl"]!);
    }
}