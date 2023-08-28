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

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.IAMCredentials.v1;
using Google.Apis.IAMCredentials.v1.Data;
using Google.Apis.Logging;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Solutions.WWAuth.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.WWAuth.Adapters
{
    /// <summary>
    /// Adapter for Service Account API.
    /// </summary>
    public interface IServiceAccountAdapter
    {
        bool IsEnabled { get; }

        string ServiceAccountEmail { get; }

        /// <summary>
        /// Check if the service account exists.
        /// </summary>
        Task<bool> ExistsAsync(
            CancellationToken cancellationToken);

        /// <summary>
        /// Impersonate the service account.
        /// </summary>
        Task<TokenResponse> ImpersonateAsync(
            string stsToken,
            IList<string> scopes,
            CancellationToken cancellationToken);

        /// <summary>
        /// Introspect access token.
        /// </summary>
        Task<ISubjectToken> IntrospectTokenAsync(
            string accessToken,
            CancellationToken cancellationToken);
    }

    internal class ServiceAccountAdapter : IServiceAccountAdapter
    {
        private readonly ILogger logger;

        public bool IsEnabled => !string.IsNullOrEmpty(this.ServiceAccountEmail);

        public string ServiceAccountEmail { get; }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue(
                    UserAgent.Default.Name,
                    UserAgent.Default.Version));
            return client;
        }

        public ServiceAccountAdapter(
            string serviceAccountEmail,
            ILogger logger)
        {
            this.ServiceAccountEmail = serviceAccountEmail;
            this.logger = logger.ThrowIfNull(nameof(logger));
        }

        /// <summary>
        /// Check if the service account exists.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ExistsAsync(
            CancellationToken cancellationToken)
        {
            //
            // If the service account email address is valid, then
            // there must be a public JWKS.
            //
            // N.B. We don't have any Google credentials, so using
            // the IAM API isn't an option.
            //

            try
            {
                this.logger.Info(
                    "Fetching JWKS for service account '{0}'",
                    this.ServiceAccountEmail);

                using (var client = CreateHttpClient())
                using (var response = await client.GetAsync(
                        new Uri("https://www.googleapis.com/service_accounts/v1/" +
                            $"metadata/jwk/{this.ServiceAccountEmail}"),
                        cancellationToken)
                    .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    this.logger.Info(
                        "JWKS for service account '{0}' found",
                        this.ServiceAccountEmail);

                    //
                    // JWKS found, service account must exist.
                    //

                    return true;
                }
            }
            catch (HttpRequestException e)
            {
                this.logger.Error(e,
                    "Failed to fetch JWKS for service account '{0}'",
                    this.ServiceAccountEmail);

                return false;
            }
        }

        /// <summary>
        /// Impersonate service account using an STS token.
        /// </summary>
        public async Task<TokenResponse> ImpersonateAsync(
            string stsToken,
            IList<string> scopes,
            CancellationToken cancellationToken)
        {
            try
            {
                this.logger.Info(
                    "Using STS token to impersonate service account '{0}'",
                    this.ServiceAccountEmail);

                using (var service = new IAMCredentialsService(
                    new BaseClientService.Initializer()
                    {
                        //
                        // Use the STS token like an access token to authenticate
                        // requests.
                        //
                        HttpClientInitializer = GoogleCredential.FromAccessToken(stsToken),
                        ApplicationName = UserAgent.Default.ToString()
                    }))
                {
                    var response = await service.Projects.ServiceAccounts
                        .GenerateAccessToken(
                            new GenerateAccessTokenRequest()
                            {
                                Scope = scopes
                            },
                            $"projects/-/serviceAccounts/{this.ServiceAccountEmail}")
                        .ExecuteAsync(cancellationToken)
                        .ConfigureAwait(false);

                    this.logger.Info(
                        "Successfully obtained access token for service account '{0}'",
                        this.ServiceAccountEmail);

                    return new TokenResponse()
                    {
                        AccessToken = response.AccessToken,
                        ExpiresInSeconds = (long)(DateTime.UtcNow - DateTime.Parse(response.ExpireTime.ToString())).TotalSeconds
                    };
                }
            }
            catch (GoogleApiException e) when (e.Error?.Code == 403)
            {
                throw new TokenExchangeException(
                    $"Insufficient permissions to impersonate service account '{this.ServiceAccountEmail}', " +
                    $"the principal might be missing the 'Workload Identity User' role", e);
            }
            catch (GoogleApiException e)
            {
                this.logger.Error(e,
                    "Failed to impersonate service account '{0}': {1}, Code: {2}, Details: {3}",
                    this.ServiceAccountEmail,
                    e.Message,
                    e.Error?.Code,
                    e.Error?.ErrorResponseContent);

                throw;
            }
        }

        /// <summary>
        /// Introspect token by calling the token info endpoint.
        /// </summary>
        public async Task<ISubjectToken> IntrospectTokenAsync(
            string accessToken,
            CancellationToken cancellationToken)
        {
            try
            {
                this.logger.Info("Introspecting access token");

                using (var client = CreateHttpClient())
                using (var response = await client.GetAsync(
                        new Uri("https://www.googleapis.com/oauth2/v1/tokeninfo?access_token=" + accessToken),
                        cancellationToken)
                    .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    var body = await response.Content
                        .ReadAsStringAsync()
                        .ConfigureAwait(false);

                    return new TokenInfo(
                        accessToken,
                        JsonConvert.DeserializeObject<Dictionary<string, object>>(body));
                }
            }
            catch (HttpRequestException e)
            {
                throw new TokenExchangeException(
                    "Token introspection failed", e);
            }
        }

        //---------------------------------------------------------------------
        // Token info.
        //---------------------------------------------------------------------

        private class TokenInfo : ISubjectToken
        {
            public SubjectTokenType Type => SubjectTokenType.IdToken;

            public string Value { get; }

            public string Issuer => "https://accounts.google.com/";

            public DateTimeOffset? Expiry { get; }

            public IDictionary<string, object> Attributes { get; }

            public string Audience { get; }

            public bool IsEncrypted => false;

            public TokenInfo(
                string value,
                IDictionary<string, object> attributes)
            {
                this.Value = value;
                this.Attributes = attributes;

                if (attributes.TryGetValue("expires_in", out var expiresIn) &&
                    expiresIn is long expiresInSecs)
                {
                    this.Expiry = DateTimeOffset.UtcNow.AddSeconds(expiresInSecs);
                }

                if (attributes.TryGetValue("audience", out var audience) &&
                    audience is string audienceValue)
                {
                    this.Audience = audienceValue;
                }
            }
        }
    }
}
