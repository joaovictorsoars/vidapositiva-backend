using Google.Apis.Auth.AspNetCore3;
using Microsoft.AspNetCore.Authentication;

namespace VidaPositiva.Api.OAuth.Extensions;

public static class AuthenticationExtensions
{
    public static AuthenticationBuilder AddGoogleOAuth(
        this AuthenticationBuilder builder)
    {
        builder.AddGoogleOpenIdConnect(GoogleOpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.ClientId = Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID");
            options.ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET");

            options.Authority = "https://accounts.google.com";
            options.ResponseType = "code";
            options.SaveTokens = true;

            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");

            options.Events.OnRedirectToIdentityProvider = context =>
            {
                context.ProtocolMessage.SetParameter("access_type", "offline");
                context.ProtocolMessage.SetParameter("prompt", "consent");
                return Task.CompletedTask;
            };
        });

            
        return builder;
    }
}