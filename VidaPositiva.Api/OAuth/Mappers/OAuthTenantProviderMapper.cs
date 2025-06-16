using System.Security.Claims;
using Google.Apis.Auth.AspNetCore3;
using VidaPositiva.Api.DTOs.Inputs.User;
using VidaPositiva.Api.OAuth.Enums;

namespace VidaPositiva.Api.OAuth.Mappers;

public static class OAuthTenantProviderMapper
{
    public static string GetAuthenticationSchemeByProvider(OAuthTenantProviderEnum provider)
    {
        return provider switch
        {
            OAuthTenantProviderEnum.Google => GoogleOpenIdConnectDefaults.AuthenticationScheme,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }

    public static UserCreationInputDto GetUserCreationDtoByProvider(OAuthTenantProviderEnum provider, string userId, IList<Claim> claims)
    {
        return provider switch
        {
            OAuthTenantProviderEnum.Google => new UserCreationInputDto
            {
                UserId = userId,
                Name = claims.First(claim => claim.Type == "name").Value,
                Email = claims.First(claim => claim.Type == ClaimTypes.Email).Value,
                PictureUrl = claims.First(claim => claim.Type == "picture").Value
            },
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
}