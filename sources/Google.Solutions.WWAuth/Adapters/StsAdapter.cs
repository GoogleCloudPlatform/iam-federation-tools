//
// Copyright 2022 Google LLC
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

using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.CloudSecurityToken.v1;
using Google.Apis.CloudSecurityToken.v1.Data;
using Google.Apis.Logging;
using Google.Apis.Util;
using Google.Solutions.WWAuth.Data;
using Google.Solutions.WWAuth.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.WWAuth.Adapters
{
    /// <summary>
    /// Adapter for the STS API.
    /// </summary>
    public interface IStsAdapter
    {
        /// <summary>
        /// Exchange external token against STS token.
        /// </summary>
        Task<TokenResponse> ExchangeTokenAsync(
            ISubjectToken externalToken,
            IList<string> scopes,
            CancellationToken cancellationToken);
    }

    internal class StsAdapter : IStsAdapter
    {
        private readonly ILogger logger;

        internal const string DefaultTokenUrl = "https://sts.googleapis.com/v1/token";

        public string Audience { get; }

        public StsAdapter(
            string audience,
            ILogger logger)
        {
            this.Audience = audience.ThrowIfNull(nameof(audience));
            this.logger = logger.ThrowIfNull(nameof(logger));
        }

        public async Task<TokenResponse> ExchangeTokenAsync(
            ISubjectToken externalToken,
            IList<string> scopes,
            CancellationToken cancellationToken)
        {
            try
            {
                this.logger.Info(
                    "Exchanging token for audience '{0}'",
                    this.Audience);

                using (var service = new CloudSecurityTokenService(
                    new Apis.Services.BaseClientService.Initializer()
                    {
                        ApplicationName = UserAgent.Default.ToString()
                    }))
                {
                    var response = await service.V1
                        .Token(
                            new GoogleIdentityStsV1ExchangeTokenRequest()
                            {
                                Audience = this.Audience,
                                GrantType = "urn:ietf:params:oauth:grant-type:token-exchange",
                                RequestedTokenType = "urn:ietf:params:oauth:token-type:access_token",
                                Scope = string.Join(" ", scopes),
                                SubjectTokenType = externalToken.Type.GetDescription(),
                                SubjectToken = externalToken.Value,
                            })
                        .ExecuteAsync(cancellationToken)
                        .ConfigureAwait(false);

                    this.logger.Info("Successfully exchanged token");

                    return new TokenResponse()
                    {
                        AccessToken = response.AccessToken,
                        ExpiresInSeconds = response.ExpiresIn,
                        TokenType = response.TokenType
                    };
                }
            }
            catch (GoogleApiException e)
            {
                //
                // Try to convert the exception.
                //
                var tokenException = TokenExchangeException.FromApiException(e);

                this.logger.Error(tokenException, "{0}", tokenException.Message);

                throw tokenException;
            }
        }

    }

    public class TokenExchangeException : Exception
    {
        public TokenExchangeException(string message)
            : base(message)
        {
        }

        public TokenExchangeException(
            string message,
            Exception inner)
            : base(message, inner)
        {
        }

        public static TokenExchangeException FromApiException(
            GoogleApiException e)
        {
            //
            // The STS returns errors in OAuth format and not in the
            // "standard" format. This trips up the client library
            // (b/197825518).
            //
            // Try to extract and parse the raw error response.
            //
            if (e.Error?.ErrorResponseContent is var jsonError && jsonError != null)
            {
                var error = JsonConvert.DeserializeObject<TokenErrorResponse>(jsonError);
                return new TokenExchangeException(error.ErrorDescription);
            }
            else
            {
                return new TokenExchangeException("Failed to exchange token", e);
            }
        }
    }
}
