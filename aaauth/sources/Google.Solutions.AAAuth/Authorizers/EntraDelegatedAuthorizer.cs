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
    public class EntraDelegatedAuthorizer : DelegatedAuthorizer
    {
        new internal const string Key = "entra-delegated";
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
            IAuthorizer entraAuthorizer,
            ILogger<EntraDelegatedAuthorizer> logger) : base(
                stsClient,
                options.WorkforceIdentityProviderName,
                entraAuthorizer, 
                logger)
        {
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

        /// <summary>
        /// Transform token result received from Entra.
        /// </summary>
        protected override async Task<TokenResult> TransformTokenResultAsync(
            TokenResult result,
            CancellationToken cancellationToken)
        {
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
                throw new ArgumentException(
                    "The token result contains neither an access token " +
                    "nor an ID token");
            }

            return await ExchangeTokenAsync(
                    tokenToExchange,
                    result.RefreshToken,
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
        public new record Options(
            Guid TenantId,
            WorkforceIdentityProviderName WorkforceIdentityProviderName,
            ICollection<string> Scopes)
        { }
    }
}
