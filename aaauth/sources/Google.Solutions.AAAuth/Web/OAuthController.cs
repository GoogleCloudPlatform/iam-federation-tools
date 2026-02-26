//
// Copyright 2026 Google LLC
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Solutions.AAAuth.Authorizers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.AAAuth.Web
{
    /// <summary>
    /// Controller that implements a minimal "federating" OAuth provider:
    /// 
    /// - The Authorize endpoint redirects users to an external identity
    ///   provider. The provider is determined by the T:IAuthorizer.
    ///   
    /// - The Token endpoint acts as facade for the external identity
    ///   provider's token endpoint. Depending on the T:IAuthorizer,
    ///   the endpoint might return the identity provider's tokens in
    ///   an altered way, or may return a different set of tokens.
    ///   
    /// </summary>
    [ApiController]
    public class OAuthController(
        IServiceProvider serviceProvider,
        OAuthController.Options options)
        : ControllerBase
    {
        private readonly IServiceProvider serviceProvider = serviceProvider;
        private readonly Options options = options;

        private Uri CreateAuthorizeContinueUri(string authorizerKey)
        {
            return new Uri($"https://{this.Request.Host}/{authorizerKey}/continue");
        }

        /// <summary>
        /// OIDC Discovery endpoint.
        /// </summary>
        /// <see href="https://openid.net/specs/openid-connect-discovery-1_0.html#ProviderMetadata"/>
        [HttpGet]
        [Produces("application/json")]
        [Route("{authorizerKey}/.well-known/openid-configuration")]
        public async Task<IActionResult> Get(
            [FromRoute(Name = "authorizerKey")] string authorizerKey,
            CancellationToken cancellationToken)
        {
            //
            // Lookup the authorizer to handle this request.
            //
            IAuthorizer authorizer;
            try
            {
                authorizer = this.serviceProvider
                    .GetRequiredKeyedService<IAuthorizer>(authorizerKey);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            var host = this.Request.Host;

            var metadata = await authorizer
                .GetMetadataAsync(cancellationToken)
                .ConfigureAwait(false);

            //
            // Return a configuration that reflects the authorizer's metadata, 
            // but installs this controller as intermediary for the authorization
            // and token endpoints.
            //

            return Ok(new OpenIdProviderMetadata()
            {
                //
                // Use the original issuer metadata.
                //
                Issuer = metadata.Issuer.AbsoluteUri,
                JsonWebKeySetEndpoint = metadata.JsonWebKeySetEndpoint.AbsoluteUri,
                ResponseTypesSupported = ["code"],
                IdTokenSigningAlgValuesSupported = metadata.IdTokenSigningAlgValuesSupported,
                SubjectTypesSupported = metadata.SubjectTypesSupported,

                //
                // Own endpoints as intermediary.
                //
                AuthorizationEndpoint = $"https://{host.ToUriComponent()}/{authorizerKey}/authorize",
                TokenEndpoint = $"https://{host.ToUriComponent()}/{authorizerKey}/token",
            });
        }

        /// <summary>
        /// OAuth 2.0 Authorization endpoint.
        /// </summary>
        /// <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.1"/>
        [HttpGet]
        [Route("{authorizerKey}/authorize")]
        public async Task<IActionResult> BeginAuthorize(
            [FromRoute(Name = "authorizerKey")] string authorizerKey,
            [FromQuery(Name = "response_type")] string? responseType,
            [FromQuery(Name = "client_id")] string? clientId,
            [FromQuery(Name = "scope")] string? scope,
            [FromQuery(Name = "redirect_uri")] string? clientRedirectUriString,
            [FromQuery(Name = "state")] string? clientState,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(responseType))
            {
                //
                // Assume 'code' (to compensate for b/479926222).
                //
                responseType = "code";
            }

            //
            // Lookup the authorizer to handle this request.
            //
            IAuthorizer authorizer;
            try
            {
                authorizer = this.serviceProvider
                    .GetRequiredKeyedService<IAuthorizer>(authorizerKey);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            //
            // Validate parameters.
            //
            if (!Uri.TryCreate(
                    clientRedirectUriString,
                    UriKind.Absolute,
                    out var clientRedirectUri) ||
                !this.options.AllowedRedirectUris.Contains(clientRedirectUri))
            {
                return BadRequest("The redirect URI is invalid");
            }

            if (responseType != "code")
            {
                return Redirect(
                    QueryHelpers.AddQueryString(
                        clientRedirectUriString,
                        "error",
                        "unsupported_response_type"));
            }

            if (string.IsNullOrEmpty(clientId))
            {
                return Redirect(
                    QueryHelpers.AddQueryString(
                        clientRedirectUriString,
                        new Dictionary<string, string?>
                        {
                            { "error", "unauthorized_client" },
                            { "error_description", "client_id parameter is missing" }
                        }));
            }

            //
            // Create a state parameter that preserves the client's
            // redirect URI and state.
            //
            var packedState = new PackedParameter(
                clientRedirectUri,
                clientState ?? string.Empty);

            //
            // Let the authorizer create an authorization URI to redirect to.
            //
            var authorizeUri = await authorizer
                .CreateAuthorizeUriAsync(
                    clientId,
                    scope,
                    packedState.ToString(),
                    CreateAuthorizeContinueUri(authorizerKey),
                    cancellationToken)
                .ConfigureAwait(false);

            return Redirect(authorizeUri.AbsoluteUri);
        }

        /// <summary>
        /// OAuth 2.0 Authorization redirect endpoint, acting as a trampoline.
        /// </summary>
        /// <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.2"/>
        [HttpGet]
        [Route("{authorizerKey}/continue")]
        public IActionResult ContinueAuthorize(
            [FromRoute(Name = "authorizerKey")] string authorizerKey,
            [FromQuery(Name = "state")] string packedState,
            [FromQuery(Name = "code")] string? code,
            [FromQuery(Name = "error")] string? errorCode,
            [FromQuery(Name = "error_description")] string? errorDescription)
        {
            //
            // Lookup the authorizer to handle this request.
            //
            try
            {
                _ = this.serviceProvider
                    .GetRequiredKeyedService<IAuthorizer>(authorizerKey);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            if (!PackedParameter.TryParse(packedState, out var state) ||
                !this.options.AllowedRedirectUris.Contains(state.Uri))
            {
                return BadRequest("Invalid state");
            }

            Dictionary<string, string?> redirectParameters;
            if (!string.IsNullOrEmpty(errorCode))
            {
                //
                // Propagate error to client.
                //
                redirectParameters = new Dictionary<string, string?>
                {
                    { "error", errorCode },
                    { "error_description", errorDescription }
                };
            }
            else
            {
                //
                // Propagate code to client so that it can redeem it using
                // our token endpoint.
                //
                redirectParameters = new Dictionary<string, string?>
                {
                    { "code", code },
                    { "state", state.Data }
                };
            }

            return Redirect(QueryHelpers.AddQueryString(
                state.Uri.AbsoluteUri,
                redirectParameters));
        }

        /// <summary>
        /// OAuth 2.0 Token endpoint.
        /// </summary>
        /// <see href="https://datatracker.ietf.org/doc/html/rfc6749#section-3.2"/>
        [HttpPost]
        [Route("{authorizerKey}/token")]
        public async Task<IActionResult> RedeemAuthorizationCodeAsync(
            [FromRoute(Name = "authorizerKey")] string authorizerKey,
            [FromForm(Name = "grant_type")] string grantType,
            [FromForm(Name = "code")] string code,
            [FromForm(Name = "redirect_uri")] string clientRedirectUri,
            [FromForm(Name = "client_id")] string? clientId,
            [FromForm(Name = "client_secret")] string? clientSecret,
            CancellationToken cancellationToken)
        {
            //
            // Lookup the authorizer to handle this request.
            //
            IAuthorizer authorizer;
            try
            {
                authorizer = this.serviceProvider
                    .GetRequiredKeyedService<IAuthorizer>(authorizerKey);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }

            //
            // Validate parameters.
            //
            if (grantType != "authorization_code")
            {
                return BadRequest(new TokenErrorResponse()
                {
                    Error = "invalid_grant",
                    ErrorDescription = "The grant type is invalid",
                });
            }
            else if (!this.options.AllowedRedirectUris.Contains(new Uri(clientRedirectUri)))
            {
                return BadRequest(new TokenErrorResponse()
                {
                    Error = "invalid_request",
                    ErrorDescription = "The redirect URI is invalid",
                });
            }

            //
            // Get client credentials. These might be passed as Basic auth or
            // as form parameters.
            //
            ClientSecrets clientCredentials;

            var rawAuthHeader = this.HttpContext.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrEmpty(rawAuthHeader) &&
                rawAuthHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase) &&
                AuthenticationHeaderValue.TryParse(rawAuthHeader, out var authHeader) &&
                Encoding.UTF8
                    .GetString(Convert.FromBase64String(authHeader.Parameter ?? ""))
                    .Split(':', 2) is string[] credentials &&
                credentials.Length == 2)

            {
                clientCredentials = new ClientSecrets()
                {
                    ClientId = credentials[0],
                    ClientSecret = credentials[1]
                };
            }
            else if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
            {
                clientCredentials = new ClientSecrets()
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                };
            }
            else
            {
                return BadRequest(new TokenErrorResponse()
                {
                    Error = "unauthorized_client"
                });
            }

            //
            // Let the authorizer redeem the code against a set of tokens.
            //
            var tokenResult = await authorizer
                .GetTokenAsync(
                    clientCredentials,
                    code,
                    CreateAuthorizeContinueUri(authorizerKey),
                    cancellationToken)
                .ConfigureAwait(false);

            return Ok(tokenResult);
        }

        /// <summary>
        /// Options for the controller.
        /// </summary>
        public record Options(
            ISet<Uri> AllowedRedirectUris)
        { }
    }
}
