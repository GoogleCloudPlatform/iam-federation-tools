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
using Google.Solutions.AAAuth.Iam;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.AAAuth.Authorizers
{
    /// <summary>
    /// Authorizer that obtains user authorization from Entra and 
    /// exchanges the resulting token for a workforce identity 
    /// access token.
    /// 
    /// When used as IdP for Gemini Enterprise agent registration,
    /// this authorizer lets agents access Google resources
    /// on the user's behalf.
    /// </summary>
    public class EntraDelegatedAuthorizer : IAuthorizer
    {
        internal const string Key = "entra-delegated";

        private readonly IStsClient stsClient;
        private readonly IAuthorizer oAuthAuthorizer;
        private readonly Options options;
        private readonly ILogger<EntraDelegatedAuthorizer> logger;

        /// <summary>
        /// Default scopes to use for an OpenID authorization, equivalent
        /// to the default scopes used by MSAL.
        /// </summary>
        /// <remarks>
        /// Gemini Enterprise requires a refresh token, so the 
        /// offline_access scope is always necessary.
        /// </remarks>
        public static readonly IReadOnlyCollection<string> DefaultScopes =
            ["openid", "profile", "offline_access"];

        internal EntraDelegatedAuthorizer(
            IStsClient stsClient,
            Options options,
            IAuthorizer oAuthAuthorizer,
            ILogger<EntraDelegatedAuthorizer> logger)
        {
            this.stsClient = stsClient;
            this.oAuthAuthorizer = oAuthAuthorizer;
            this.options = options;
            this.logger = logger;
        }

        public EntraDelegatedAuthorizer(
            IStsClient stsClient,
            Options options,
            ILogger<EntraDelegatedAuthorizer> logger)
            : this(
                  stsClient,
                  options,
                  new OAuthAuthorizer(
                      new Uri($"https://login.microsoftonline.com/{options.TenantId}/v2.0/"),
                      new HashSet<string>(options.Scopes.Concat(DefaultScopes)),
                      logger),
                  logger)
        {
        }

        //---------------------------------------------------------------------
        // IAuthorizer.
        //---------------------------------------------------------------------

        /// <inheritdoc/>
        public Task<Uri> CreateAuthorizeUriAsync(
            string clientId,
            string? scope,
            string? passthroughState,
            Uri redirectUri,
            CancellationToken cancellationToken)
        {
            return this.oAuthAuthorizer.CreateAuthorizeUriAsync(
                clientId,
                scope,
                passthroughState,
                redirectUri,
                cancellationToken);
        }

        /// <inheritdoc/>
        public Task<AuthorizerMetadata> GetMetadataAsync(
            CancellationToken cancellationToken)
        {
            return this.oAuthAuthorizer.GetMetadataAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TokenResult> GetTokenAsync(
            ClientSecrets clientCredentials,
            string authorizationCode,
            Uri redirectUri,
            CancellationToken cancellationToken)
        {
            //
            // Redeem code for an Entra tokens.
            //
            var result = await this.oAuthAuthorizer
                .GetTokenAsync(
                    clientCredentials,
                    authorizationCode,
                    redirectUri,
                    cancellationToken)
                .ConfigureAwait(false);

            //
            // Use the Entra ID token to obtain a federated token from
            // the workforce identity pool.
            //

            string tokenToExchange;
            if (result.AccessToken != null &&
                result.Scope != null &&
                result.Scope.Contains("api://"))
            {
                //
                // The authorization was done with a custom scope, which
                // suggests we should use the access token to perform the
                // token exchange. 
                //

                this.logger.LogInformation(
                    "Exchanging access token with scope {scope}",
                    result.Scope);
                tokenToExchange = result.AccessToken;
            }
            else if (result.IdToken != null)
            {
                this.logger.LogInformation(
                    "Exchanging ID token with scope {scope}",
                    result.Scope);
                tokenToExchange = result.IdToken;
            }
            else
            {
                throw new InvalidOperationException(
                    "The token result contains neither an access token " +
                    "nor an ID token");
            }

            try
            {
                var exchangeResult = await this.stsClient
                    .ExchangeTokenAsync(
                        this.options.WorkforceIdentityProviderName,
                        tokenToExchange,
                        [Scopes.CloudPlatform],
                        cancellationToken)
                    .ConfigureAwait(false);

                return new TokenResult()
                {
                    AccessToken = exchangeResult.AccessToken,
                    RefreshToken = "placeholder", // TODO: propagate Entra refresh token
                    IdToken = exchangeResult.IdToken,
                    Scope = exchangeResult.Scope,
                    ExpiresInSeconds = exchangeResult.ExpiresInSeconds,
                };
            }
            catch (TokenResponseException e)
            {
                this.logger.LogError(
                    "Token exchange failed: {details}",
                    e.Error.ErrorDescription);

                throw;
            }
        }

        /// <summary>
        /// Options.
        /// </summary>
        /// <param name="TenantId">Entra tenant ID</param>
        /// <param name="WorkforceIdentityProviderName">
        /// Provider to use for token exchange, must be configured to
        /// accept tokens from Entra.
        /// </param>
        /// <param name="Scopes">Entra scope to use, optional</param>
        public record Options(
            Guid TenantId,
            WorkforceIdentityProviderName WorkforceIdentityProviderName,
            ICollection<string> Scopes)
        { }
    }
}
