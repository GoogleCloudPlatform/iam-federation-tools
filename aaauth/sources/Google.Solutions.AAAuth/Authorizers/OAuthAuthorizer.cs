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
using Google.Solutions.AAAuth.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.AAAuth.Authorizers
{
    /// <summary>
    /// Authorizer that performs a generic OAuth authorization flow.
    /// The authorizer uses OpenID Discovery to determine the right
    /// endpoints to use.
    /// </summary>
    /// <param name="scopes">Entra scopes to use, optional</param>
    public class OAuthAuthorizer(
        Uri issuer,
        IReadOnlyCollection<string> scopes,
        ILogger logger)
        : IAuthorizer
    {
        private readonly HttpClient httpClient = new();
        private readonly ILogger logger = logger;

        /// <summary>
        /// The provider's issuer URL.
        /// </summary>
        public Uri Issuer
        {
            get => issuer;
        }

        /// <summary>
        /// Fetch the IdP's OpenID Discovery metadata.
        /// </summary>
        protected async Task<OpenIdProviderMetadata> GetProviderMetadataAsync(
            CancellationToken cancellationToken)
        {
            //
            // Fetch the IdP's discovery information.
            //
            var metadataEndpoint =
                $"{this.Issuer.AbsoluteUri}/.well-known/openid-configuration";

            var idpMetadata = await this.httpClient
                .GetFromJsonAsync<OpenIdProviderMetadata>(
                metadataEndpoint,
                cancellationToken);

            if (idpMetadata == null ||
                string.IsNullOrEmpty(idpMetadata.JsonWebKeySetEndpoint) ||
                idpMetadata.SubjectTypesSupported == null ||
                idpMetadata.IdTokenSigningAlgValuesSupported == null)
            {
                throw new InvalidOperationException(
                    $"The OpenID discovery endpoint at " +
                    $"{metadataEndpoint} returned empty or incomplete " +
                    $"metadata");
            }

            return idpMetadata;
        }

        //---------------------------------------------------------------------
        // IAuthorizer.
        //---------------------------------------------------------------------

        /// <inheritdoc/>
        public virtual async Task<AuthorizerMetadata> GetMetadataAsync(
            CancellationToken cancellationToken)
        {
            var idpMetadata = await GetProviderMetadataAsync(cancellationToken)
                .ConfigureAwait(false);

            return new AuthorizerMetadata(
                this.Issuer,
                new Uri(idpMetadata.JsonWebKeySetEndpoint),
                idpMetadata.SubjectTypesSupported,
                idpMetadata.IdTokenSigningAlgValuesSupported);
        }

        /// <inheritdoc/>
        public virtual async Task<Uri> CreateAuthorizeUriAsync(
            string clientId,
            string? scope,
            string? passthroughState,
            Uri redirectUri,
            CancellationToken cancellationToken)
        {
            var metadata = await
                GetProviderMetadataAsync(cancellationToken)
                .ConfigureAwait(false);

            return new Uri(QueryHelpers.AddQueryString(
                metadata.AuthorizationEndpoint,
                new Dictionary<string, string?>
                {
                    { "response_type", "code" },
                    { "client_id", clientId },
                    { "redirect_uri", redirectUri.AbsoluteUri },
                    { "scope", string.Join(" ", scopes) },
                    { "state", passthroughState }
                }));
        }

        /// <inheritdoc/>
        public virtual async Task<TokenResult> GetTokenAsync(
            ClientSecrets clientCredentials,
            string authorizationCode,
            Uri redirectUri,
            CancellationToken cancellationToken)
        {
            var metadata = await
                GetProviderMetadataAsync(cancellationToken)
                .ConfigureAwait(false);

            try
            {
                var response = await this.httpClient
                    .PostAsync(
                        metadata.TokenEndpoint,
                        new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "client_id", clientCredentials.ClientId },
                            { "client_secret", clientCredentials.ClientSecret },
                            { "redirect_uri", redirectUri.AbsoluteUri },
                            { "grant_type", "authorization_code" },
                            { "code", authorizationCode }
                        }),
                        cancellationToken)
                    .ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                var tokenResult = await response.Content
                    .ReadFromJsonAsync<TokenResult>(cancellationToken)
                    .ConfigureAwait(false);

                this.logger.LogInformation(
                    "Successfully redeemed authorization code for client {client}",
                    clientCredentials.ClientId);

                return tokenResult ??
                    throw new InvalidOperationException(
                        $"The token endpoint at {metadata.TokenEndpoint} " +
                        $"returned an empty or malformed response");
            }
            catch (HttpRequestException e)
            {
                this.logger.LogError(e, "Redeeming authorization code failed");
                throw;
            }
        }
    }
}
