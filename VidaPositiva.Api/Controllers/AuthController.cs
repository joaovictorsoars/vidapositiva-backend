using System.Security.Claims;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using VidaPositiva.Api.OAuth.Enums;
using VidaPositiva.Api.OAuth.Mappers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VidaPositiva.Api.OAuth.Constants;
using VidaPositiva.Api.Services.UserService;
using VidaPositiva.Api.ValueObjects.Validation;

namespace VidaPositiva.Api.Controllers;

[Route("[controller]")]
[ApiController]
public class AuthController(IUserService userService) : ControllerBase
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
    public async Task<IActionResult> Callback(OAuthTenantProviderEnum provider, CancellationToken cancellationToken)
    {
        var authenticationScheme = OAuthTenantProviderMapper.GetAuthenticationSchemeByProvider(provider);
        
        var authenticateResult = await HttpContext.AuthenticateAsync(authenticationScheme);
    
        if (!authenticateResult.Succeeded || authenticateResult.Properties.Items.All(item => item.Key != "returnUrl"))
            return new ValidationError
            {
                HttpCode = 401,
                Message = "The returnUrl parameter is incorrect",
            }.AsActionResult();
        
        var userId = authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { Message = "ID de usuário não encontrado" });
        
        var claims = authenticateResult.Principal.Claims.ToList();
    
        var userIdClaim = new Claim("user_id", userId);
        var providerClaim = new Claim("provider", provider.ToString());
        
        claims.Add(userIdClaim);
        claims.Add(providerClaim);
    
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var userDto = OAuthTenantProviderMapper.GetUserCreationDtoByProvider(provider, userId, claims);

        await userService.SignInWithGoogle(userDto, cancellationToken);
    
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
            Response.Cookies.Append(CookiesConstants.RefreshCookieName, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });
        
        var frontendUrl = Environment.GetEnvironmentVariable("VIDA_POSITIVA_FRONTEND_URL");
    
        return Redirect(frontendUrl + authenticateResult.Properties.Items["returnUrl"]!);
    }
    
    [HttpGet("refresh")]
    public async Task<IActionResult> RefreshSession(CancellationToken cancellationToken)
    {
        try
        {
            var refreshCookie = await HttpContext.GetTokenAsync(CookiesConstants.RefreshCookieName);
            
            if (string.IsNullOrWhiteSpace(refreshCookie))
                return new ValidationError { Code = "no_refresh_token", Message = "Refresh Cookie não encontrado.", HttpCode = 401 }.AsActionResult();
            
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
                return Unauthorized(new { Message = "Usuário não autenticado" });
            
            var userId = authenticateResult.Principal.FindFirst("user_id")?.Value;
            
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID"),
                    ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET")
                }
            });
            
            var newToken = await flow.RefreshTokenAsync(userId, refreshCookie, cancellationToken);

            if (newToken == null || string.IsNullOrWhiteSpace(newToken.AccessToken))
                return new ValidationError
                {
                    Code = "token_refresh_failed",
                    Message = "Erro ao renovar token de acesso.",
                    HttpCode = 401
                }.AsActionResult();
            
            return Ok(new { Message = "Sessão renovada com sucesso." });
        }
        catch
        {
            return new ValidationError
            {
                Code = "session_refresh_error",
                HttpCode = 401,
                Message = "Erro ao renovar sessão."
            }.AsActionResult();
        }
    }
    
    [HttpGet("signout")]
    [Authorize]
    public async Task<IActionResult> Signout()
    {
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            return Ok(new { Message = "Signout realizado com sucesso" });
        }
        catch
        {
            return StatusCode(500, new { error = "Ocorreu um erro ao fazer logout." });
        }
    }
    
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var result = await userService.GetByGoogleUserId(userId, cancellationToken);

        return result.Fold(
            l => l.AsActionResult(),
            Ok);
    }
    
    [Authorize]
    [HttpGet("validate")]
    public async Task<IActionResult> Validate(CancellationToken cancellationToken)
    {
        var isAuthenticated = User.Identity?.IsAuthenticated ?? false;

        if (!isAuthenticated)
            return new ValidationError
            {
                Code = "user_not_authenticated",
                HttpCode = 401,
                Message = "Não autenticado."
            }.AsActionResult();
        
        var userExists = await userService.GetByGoogleUserId(User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value, cancellationToken);
        
        if (userExists.IsLeft)
            return new ValidationError
            {
                Code = "user_not_authenticated",
                HttpCode = 401,
                Message = "Não autenticado."
            }.AsActionResult();
        
        return Ok(new { Message = "Autenticado." });
    }
}