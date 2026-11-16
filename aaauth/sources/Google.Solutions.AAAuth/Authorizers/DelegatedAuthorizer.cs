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
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.AAAuth.Authorizers
{
    /// <summary>
    /// Authorizer that obtains user authorization from an OAuth IdP
    /// and exchanges the resulting token for a workforce identity 
    /// access token.
    /// 
    /// When used as IdP for Gemini Enterprise agent registration,
    /// this authorizer lets agents access Google resources
    /// on the user's behalf.
    /// </summary>
    public class DelegatedAuthorizer : IAuthorizer
    {
        internal const string Key = "delegated";

        private readonly ILogger logger;
        private readonly WorkforceIdentityProviderName provider;


        protected IStsClient StsClient { get; }

        /// <summary>
        /// Authorizer that uses the OAuth IdP.
        /// </summary>
        protected IAuthorizer IdPAuthorizer { get; }

        protected DelegatedAuthorizer(
            IStsClient stsClient,
            WorkforceIdentityProviderName provider,
            IAuthorizer idpAuthorizer,
            ILogger logger)
        {
            this.provider = provider;
            this.IdPAuthorizer = idpAuthorizer;
            this.StsClient = stsClient;
            this.logger = logger;
        }

        internal DelegatedAuthorizer(
            IStsClient stsClient,
            Options options,
            IAuthorizer idpAuthorizer,
            ILogger<DelegatedAuthorizer> logger) : this(
                stsClient,
                options.WorkforceIdentityProviderName,
                idpAuthorizer,
                logger)
        {
        }

        public DelegatedAuthorizer(
            IStsClient stsClient,
            Options options,
            ILogger<DelegatedAuthorizer> logger) : this(
                stsClient,
                options,
                new OAuthAuthorizer(
                    options.Issuer,
                    new HashSet<string>(options.Scopes),
                    logger),
                logger)
        {
        }

        /// <summary>
        /// Perform token exchange to obtain a workforce identity
        /// federation token.
        /// </summary>
        protected virtual async Task<TokenResult> ExchangeTokenAsync(
            string tokenToExchange,
            string? refreshToken,
            CancellationToken cancellationToken)
        {
            try
            {
                var exchangeResult = await this.StsClient
                    .ExchangeTokenAsync(
                        this.provider,
                        tokenToExchange,
                        [Scopes.CloudPlatform],
                        cancellationToken)
                    .ConfigureAwait(false);

                return new TokenResult()
                {
                    //
                    // Return the original refresh token...
                    //
                    RefreshToken = refreshToken,

                    //
                    // and the federated token as access token.
                    //
                    AccessToken = exchangeResult.AccessToken,
                    IdToken = exchangeResult.IdToken,
                    Scope = exchangeResult.Scope,

#if DEBUG
                    ExpiresInSeconds = 300,
#else

                    ExpiresInSeconds = exchangeResult.ExpiresInSeconds,
#endif
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
        /// Transform token result received from IdP.
        /// </summary>
        protected virtual async Task<TokenResult> TransformTokenResultAsync(
            TokenResult result,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(result.IdToken))
            {
                throw new ArgumentException(
                    "The token result does not contain an ID token");
            }

            return await ExchangeTokenAsync(
                    result.IdToken,
                    result.RefreshToken,
                    cancellationToken)
                .ConfigureAwait(false);
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
            return this.IdPAuthorizer.CreateAuthorizeUriAsync(
                clientId,
                scope,
                passthroughState,
                redirectUri,
                cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<AuthorizerMetadata> GetMetadataAsync(
            CancellationToken cancellationToken)
        {
            return await this.IdPAuthorizer
                .GetMetadataAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<TokenResult> RedeemAuthorizationCodeAsync(
            ClientSecrets clientCredentials,
            string authorizationCode,
            Uri redirectUri,
            CancellationToken cancellationToken)
        {
            var tokenResult = await this.IdPAuthorizer
                .RedeemAuthorizationCodeAsync(
                    clientCredentials,
                    authorizationCode,
                    redirectUri,
                    cancellationToken)
                .ConfigureAwait(false);

            return await TransformTokenResultAsync(
                    tokenResult, 
                    cancellationToken)
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<TokenResult> RedeemRefreshTokenAsync(
            ClientSecrets clientCredentials,
            string refreshToken,
            string? scope,
            CancellationToken cancellationToken)
        {
            var tokenResult = await this.IdPAuthorizer
                .RedeemRefreshTokenAsync(
                    clientCredentials,
                    refreshToken,
                    scope,
                    cancellationToken)
                .ConfigureAwait(false);

            return await TransformTokenResultAsync(
                    tokenResult, 
                    cancellationToken)
                .ConfigureAwait(false);
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
            Uri Issuer,
            WorkforceIdentityProviderName WorkforceIdentityProviderName,
            ICollection<string> Scopes)
        { }
    }
}
