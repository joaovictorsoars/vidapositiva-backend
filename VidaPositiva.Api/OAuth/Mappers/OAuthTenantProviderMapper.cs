using Google.Apis.Auth.AspNetCore3;
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
}