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

using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.CloudSecurityToken.v1;
using Google.Apis.CloudSecurityToken.v1.Data;
using Google.Apis.Json;
using Google.Apis.Services;
using Google.Solutions.AAAuth.Authorizers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.AAAuth.Iam
{
    /// <summary>
    /// Client for the STS API.
    /// </summary>
    public interface IStsClient
    {
        /// <summary>
        /// Exchange an external token for a federated access token
        /// by using workload identity.
        /// </summary>
        Task<TokenResult> ExchangeTokenAsync(
            WorkloadIdentityProviderName audience,
            string subjectToken,
            IEnumerable<string> scopes,
            CancellationToken cancellationToken);

        /// <summary>
        /// Exchange an external token for a federated access token
        /// by using workforce identity
        /// </summary>
        Task<TokenResult> ExchangeTokenAsync(
            WorkforceIdentityProviderName audience,
            string subjectToken,
            IEnumerable<string> scopes,
            CancellationToken cancellationToken);
    }

    public class StsClient : IStsClient
    {
        private readonly CloudSecurityTokenService service;

        public StsClient()
        {
            this.service = new CloudSecurityTokenService(
                new BaseClientService.Initializer()
                {
                    ApplicationName = UserAgent.Instance.ToString()
                });
        }

        private async Task<TokenResult> ExchangeTokenAsync(
            string audience,
            string subjectToken,
            IEnumerable<string> scopes,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await this.service
                    .V1
                    .Token(
                        new GoogleIdentityStsV1ExchangeTokenRequest()
                        {
                            Audience = audience,
                            SubjectToken = subjectToken,
                            SubjectTokenType = TokenTypes.IdToken,
                            RequestedTokenType = TokenTypes.AccessToken,
                            GrantType = "urn:ietf:params:oauth:grant-type:token-exchange",
                            Scope = string.Join(" ", scopes)
                        })
                    .ExecuteAsync(cancellationToken)
                    .ConfigureAwait(false);

                return new TokenResult()
                {
                    TokenType = "bearer",
                    AccessToken = response.AccessToken,
                    ExpiresInSeconds = response.ExpiresIn,
                };
            }
            catch (Exception e)
            when (e.Unwrap() is GoogleApiException gae &&
                gae.Error?.ErrorResponseContent is string json)
            {
                //
                // Try to obtain the real error message, which is hidden inside
                // the exception.
                //
                TokenErrorResponse? error = null;
                try
                {
                    error = (TokenErrorResponse)NewtonsoftJsonSerializer
                        .Instance
                        .Deserialize(json, typeof(TokenErrorResponse));
                }
                catch { }

                if (!string.IsNullOrEmpty(error?.ErrorDescription))
                {
                    throw new TokenResponseException(error);
                }
                else
                {
                    //
                    // A different exception occurred, rethrow.
                    //
                    throw;
                }
            }
        }

        public Task<TokenResult> ExchangeTokenAsync(
            WorkloadIdentityProviderName audience,
            string subjectToken,
            IEnumerable<string> scopes,
            CancellationToken cancellationToken)
        {
            return ExchangeTokenAsync(
                audience.ResourceName,
                subjectToken,
                scopes,
                cancellationToken);
        }

        public Task<TokenResult> ExchangeTokenAsync(
            WorkforceIdentityProviderName audience,
            string subjectToken,
            IEnumerable<string> scopes,
            CancellationToken cancellationToken)
        {
            return ExchangeTokenAsync(
                audience.ResourceName,
                subjectToken,
                scopes,
                cancellationToken);
        }
    }
}
