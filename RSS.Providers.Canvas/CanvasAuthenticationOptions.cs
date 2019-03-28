using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace RSS.Providers.Canvas
{
    /// <summary>
    /// Defines a set of options used by <see cref="CanvasAuthenticationHandler"/>.
    /// </summary>
    public class CanvasAuthenticationOptions : OAuthOptions
    {
        public CanvasAuthenticationOptions()
        {
            ClaimsIssuer = CanvasAuthenticationDefaults.Issuer;

            CallbackPath = new PathString(CanvasAuthenticationDefaults.CallbackPath);

            ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
            ClaimActions.MapJsonKey(ClaimTypes.Email, "primary_email");
            ClaimActions.MapJsonKey(ClaimTypes.Role, "role");
            ClaimActions.MapJsonKey("urn:canvas:avatar_url", "avatar_url");
        }
    }
}