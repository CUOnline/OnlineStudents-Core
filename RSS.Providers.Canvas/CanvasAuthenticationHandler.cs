using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RSS.Providers.Canvas.Models;

namespace RSS.Providers.Canvas
{
    public class CanvasAuthenticationHandler : OAuthHandler<CanvasAuthenticationOptions>
    {
        public CanvasAuthenticationHandler(
            [NotNull] IOptionsMonitor<CanvasAuthenticationOptions> options,
            [NotNull] ILoggerFactory logger,
            [NotNull] UrlEncoder encoder,
            [NotNull] ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync([NotNull] ClaimsIdentity identity,
            [NotNull] AuthenticationProperties properties, [NotNull] OAuthTokenResponse tokens)
        {
            var userId = tokens.Response.SelectToken("user").Value<string>("id");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{Options.UserInformationEndpoint}api/v1/users/{userId}/profile");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted);
            
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError("An error occurred while retrieving the user profile: the remote server " +
                                "returned a {Status} response with the following payload: {Headers} {Body}.",
                                /* Status: */ response.StatusCode,
                                /* Headers: */ response.Headers.ToString(),
                                /* Body: */ await response.Content.ReadAsStringAsync());

                throw new HttpRequestException("An error occurred while retrieving the user from the Canvas identity service.");
            }

            var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
            var principal = new ClaimsPrincipal(identity);

            var roles = await GetAccountRolesForUserAsync(Backchannel, "1", userId, tokens.AccessToken);
            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            var context = new OAuthCreatingTicketContext(principal, properties, Context, Scheme, Options, Backchannel, tokens, payload);
            context.RunClaimActions(payload);

            await Options.Events.CreatingTicket(context);
            return new AuthenticationTicket(context.Principal, context.Properties, Scheme.Name);
        }

        public async Task<List<string>> GetAccountRolesForUserAsync(HttpClient canvasClient, string accountId, string userId, string accessToken)
        {
            var roles = new List<string>();

            // Get the account
            var accountRequest = new HttpRequestMessage(HttpMethod.Get, $"{Options.UserInformationEndpoint}api/v1/accounts/{accountId}");
            accountRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            accountRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var accountResponse = await canvasClient.SendAsync(accountRequest, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted);
            if (!accountResponse.IsSuccessStatusCode)
            {
                return roles;
            }

            var account = JsonConvert.DeserializeObject<Account>(await accountResponse.Content.ReadAsStringAsync());

            // Get roles for account
            var rolesRequest = new HttpRequestMessage(HttpMethod.Get, $"{Options.UserInformationEndpoint}api/v1/accounts/{accountId}/admins?user_id[]={userId}");
            rolesRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            rolesRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var rolesResponse = await canvasClient.SendAsync(rolesRequest, HttpCompletionOption.ResponseHeadersRead, Context.RequestAborted);

            if (!rolesResponse.IsSuccessStatusCode)
            {
                return roles;
            }

            var result = JArray.Parse(await rolesResponse.Content.ReadAsStringAsync());
            if (result.Count > 0)
            {
                foreach (var role in result.Children<JObject>())
                {
                    roles.Add(JsonConvert.DeserializeObject<Role>(role.ToString()).Name);
                }
            }

            // Get roles for parent account (recursive)
            if (!string.IsNullOrWhiteSpace(account.ParentAccountId))
            {
                roles.AddRange(await GetAccountRolesForUserAsync(canvasClient, account.ParentAccountId, userId, accessToken));
            }

            return roles;
        }
    }
}