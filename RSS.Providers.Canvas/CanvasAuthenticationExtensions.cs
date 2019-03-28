using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using RSS.Providers.Canvas;

namespace Microsoft.Extensions.DependencyInjection
{

    /// <summary>
    /// Extension methods to add Canvas authentication capabilities to an HTTP application pipeline.
    /// </summary>
    public static class CanvasAuthenticationExtensions
    {
        /// <summary>
        /// Adds <see cref="CanvasAuthenticationHandler"/> to the specified
        /// <see cref="AuthenticationBuilder"/>, which enables Canvas authentication capabilities.
        /// </summary>
        /// <param name="builder">The authentication builder.</param>
        /// <returns>The <see cref="AuthenticationBuilder"/>.</returns>
        public static AuthenticationBuilder AddCanvas([NotNull] this AuthenticationBuilder builder)
        {
            return builder.AddCanvas(CanvasAuthenticationDefaults.AuthenticationScheme, options => { });
        }

        /// <summary>
        /// Adds <see cref="CanvasAuthenticationHandler"/> to the specified
        /// <see cref="AuthenticationBuilder"/>, which enables Canvas authentication capabilities.
        /// </summary>
        /// <param name="builder">The authentication builder.</param>
        /// <param name="configuration">The delegate used to configure the OpenID 2.0 options.</param>
        /// <returns>The <see cref="AuthenticationBuilder"/>.</returns>
        public static AuthenticationBuilder AddCanvas(
            [NotNull] this AuthenticationBuilder builder,
            [NotNull] Action<CanvasAuthenticationOptions> configuration)
        {
            return builder.AddCanvas(CanvasAuthenticationDefaults.AuthenticationScheme, configuration);
        }

        /// <summary>
        /// Adds <see cref="CanvasAuthenticationHandler"/> to the specified
        /// <see cref="AuthenticationBuilder"/>, which enables Canvas authentication capabilities.
        /// </summary>
        /// <param name="builder">The authentication builder.</param>
        /// <param name="scheme">The authentication scheme associated with this instance.</param>
        /// <param name="configuration">The delegate used to configure the Canvas options.</param>
        /// <returns>The <see cref="AuthenticationBuilder"/>.</returns>
        public static AuthenticationBuilder AddCanvas(
            [NotNull] this AuthenticationBuilder builder, [NotNull] string scheme,
            [NotNull] Action<CanvasAuthenticationOptions> configuration)
        {
            return builder.AddCanvas(scheme, CanvasAuthenticationDefaults.DisplayName, configuration);
        }

        /// <summary>
        /// Adds <see cref="CanvasAuthenticationHandler"/> to the specified
        /// <see cref="AuthenticationBuilder"/>, which enables Canvas authentication capabilities.
        /// </summary>
        /// <param name="builder">The authentication builder.</param>
        /// <param name="scheme">The authentication scheme associated with this instance.</param>
        /// <param name="caption">The optional display name associated with this instance.</param>
        /// <param name="configuration">The delegate used to configure the Canvas options.</param>
        /// <returns>The <see cref="AuthenticationBuilder"/>.</returns>
        public static AuthenticationBuilder AddCanvas(
            [NotNull] this AuthenticationBuilder builder,
            [NotNull] string scheme, [CanBeNull] string caption,
            [NotNull] Action<CanvasAuthenticationOptions> configuration)
        {
            return builder.AddOAuth<CanvasAuthenticationOptions, CanvasAuthenticationHandler>(scheme, caption, configuration);
        }
    }
}
