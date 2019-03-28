using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RSS.Providers.Canvas
{
    /// <summary>
    /// Default values used by the Canvas authentication middleware.
    /// </summary>
    public static class CanvasAuthenticationDefaults
    {
        public const string AuthenticationScheme = "Canvas";
        public const string DisplayName = "Canvas";
        public const string Issuer = "Canvas";
        public const string CallbackPath = "/signin-canvas";
    }
}
