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
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.AAAuth.Authorizers
{
    /// <summary>
    /// Authorizer that authenticates the user using Google OpenID Connect
    /// and returns the resulting ID token _as access token_.
    /// 
    /// When used as IdP for Gemini Enterprise agent registration,
    /// this authorizer lets agents identify the user (by means of the
    /// ID token) and make calls to Cloud Run and IAP on the user's
    /// behalf.
    /// </summary>
    public class GoogleIdentityAuthorizer : IAuthorizer
    {
        internal const string Key = "google-identity";

        private readonly IAuthorizer oAuthAuthorizer;
        private readonly ILogger<GoogleIdentityAuthorizer> logger;

        /// <summary>
        /// Immutable validity of Google ID tokens.
        /// </summary>
        private static readonly TimeSpan IdTokenValidity = TimeSpan.FromHours(1);

        /// <summary>
        /// Default scopes to use for an OpenID authorization.
        /// </summary>
        public static readonly IReadOnlyCollection<string> DefaultScopes =
            ["openid", "email"];

        internal GoogleIdentityAuthorizer(
            IAuthorizer oAuthAuthorizer,
            ILogger<GoogleIdentityAuthorizer> logger)
        {
            this.oAuthAuthorizer = oAuthAuthorizer;
            this.logger = logger;
        }

        public GoogleIdentityAuthorizer(
            ILogger<GoogleIdentityAuthorizer> logger)
            : this(
                  new OAuthAuthorizer(
                      new Uri("https://accounts.google.com"),
                      DefaultScopes,
                      logger),
                  logger)
        {
        }

        //---------------------------------------------------------------------
        // IAuthorizer.
        //---------------------------------------------------------------------

        /// <inheritdoc/>
        public async Task<Uri> CreateAuthorizeUriAsync(
            string clientId,
            string? scope,
            string? passthroughState,
            Uri redirectUri,
            CancellationToken cancellationToken)
        {
            var uri = await this.oAuthAuthorizer
                .CreateAuthorizeUriAsync(
                    clientId,
                    scope,
                    passthroughState,
                    redirectUri,
                    cancellationToken)
                .ConfigureAwait(false);

            //
            // Append access_type parameter so that we receive
            // a refresh token.
            //
            return new Uri(QueryHelpers.AddQueryString(
                uri.AbsoluteUri,
                "access_type", "offline"));
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
            // Redeem code for an Google tokens.
            //
            var result = await this.oAuthAuthorizer
                .GetTokenAsync(
                    clientCredentials,
                    authorizationCode,
                    redirectUri,
                    cancellationToken)
                .ConfigureAwait(false);

            //
            // Return the ID token as access token.
            //
            return new TokenResult()
            {
                TokenType = "bearer",
                AccessToken = result.IdToken,
                ExpiresInSeconds = (int)IdTokenValidity.TotalSeconds,
                RefreshToken = result.RefreshToken,
                Scope = result.Scope,
            };
        }
    }
}
