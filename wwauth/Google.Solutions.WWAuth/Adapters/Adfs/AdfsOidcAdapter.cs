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
using Google.Apis.Logging;
using Google.Apis.Util;
using Google.Solutions.WWAuth.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.WWAuth.Adapters.Adfs
{
    /// <summary>
    /// Adapter for acquiring tokens using OIDC and the
    /// client credentials grant.
    /// </summary>
    internal class AdfsOidcAdapter : AdfsAdapterBase
    {
        /// <summary>
        /// AD FS Client ID, can have any format.
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// AD FS Web API. Typed as string (and not Uri) to prevent
        /// canonicaliuation, which might break the STS token exchange.
        /// </summary>
        public string Resource { get; }

        /// <summary>
        /// URI to OIDC configuration/metadata.
        /// </summary>
        public Uri OidcConfigurationUrl
            => new Uri(this.IssuerUrl, ".well-known/openid-configuration");

        /// <summary>
        /// SPN to verify in pre-flight checks.
        /// </summary>
        protected override string ServicePrincipalName
            => $"HTTP/{this.IssuerUrl.Host}";

        public AdfsOidcAdapter(
            Uri issuerUrl,
            string clientId,
            string resource,
            ILogger logger) : base(issuerUrl, logger)
        {
            this.ClientId = clientId.ThrowIfNull(nameof(clientId));
            this.Resource = resource.ThrowIfNull(nameof(resource));
        }

        private async Task<OidcConfiguration> FetchOidcConfigurationAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                this.Logger.Info(
                    "Fetching OpenID Connect configuration from '{0}'",
                    this.OidcConfigurationUrl);

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.Add(
                        new ProductInfoHeaderValue(
                            UserAgent.Default.Name,
                            UserAgent.Default.Version));
                    using (var response = await client.GetAsync(
                            this.OidcConfigurationUrl,
                            cancellationToken)
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();

                        return JsonConvert.DeserializeObject<OidcConfiguration>(
                            await response.Content
                                .ReadAsStringAsync()
                                .ConfigureAwait(false));
                    }
                }
            }
            catch (JsonException e)
            {
                throw new TokenAcquisitionException(
                    $"The OpenID Connect configuration located at '{this.OidcConfigurationUrl}' " +
                    $"contains invalid data. Verify that the issuer URL '{this.IssuerUrl}' " +
                    "is correct and that the URL is accessible.",
                    e);
            }
            catch (HttpRequestException e)
            {
                throw new TokenAcquisitionException(
                    $"The OpenID Connect configuration located at '{this.OidcConfigurationUrl}' " +
                    $"is not available. Verify that the issuer URL '{this.IssuerUrl}' " +
                    "is correct and that the URL is accessible.",
                    e);
            }
        }

        protected override async Task<ISubjectToken> AcquireTokenCoreAsync(
            CancellationToken cancellationToken)
        {
            //
            // Fetch the OIDC configuration. This implicitly validates the
            // issuer URL and provides us the token endpoint.
            //
            var configuration = await FetchOidcConfigurationAsync(cancellationToken)
                .ConfigureAwait(false);

            this.Logger.Info("Using token endpoint '{0}'", configuration.TokenEndpoint);

            //
            // Request a token from the token endpoint.
            //
            using (var handler = new HttpClientHandler())
            {
                handler.Credentials = this.Credential;

                using (var client = new HttpClient(handler))
                {
                    this.Logger.Info(
                        "Acquiring OIDC token for client ID '{0}' " +
                            "and resource '{1}' using IWA",
                        this.ClientId,
                        this.Resource);

                    //
                    // AD FS occasionally fails requests for no good reason, so use
                    // a backoff/retry loop.
                    //
                    var backoff = new ExponentialBackOff();
                    for (var retries = 0; ; retries++)
                    {
                        var request = new HttpRequestMessage(HttpMethod.Post, configuration.TokenEndpoint)
                        {
                            Content = new FormUrlEncodedContent(new Dictionary<string, string>
                                {
                                    { "client_id", this.ClientId },
                                    { "resource", this.Resource },
                                    { "grant_type", "client_credentials" },
                                    { "use_windows_client_authentication", "true" }, // Use IWA, see [OS-OAPX].
                                    { "scope", "openid" }
                                })
                        };

                        //
                        // Set a custom user-agent so that AD FS lets us use IWA.
                        //
                        request.Headers.Add("User-Agent", AdfsAdapterBase.IwaUserAgent);

                        using (var response = await client.SendAsync(
                                request,
                                HttpCompletionOption.ResponseHeadersRead,
                                cancellationToken)
                            .ConfigureAwait(false))
                        {
                            var responseType = response.Content?.Headers?.ContentType?.MediaType;
                            if (responseType == "application/json" &&
                                response.StatusCode == HttpStatusCode.OK)
                            {
                                //
                                // Use the existing TokenResponse class to parse
                                // the response (and handle errors).
                                //
                                this.Logger.Info("Acquiring token succeeded");
                                return JsonWebToken.FromResponse(
                                    await TokenResponse.FromHttpResponseAsync(
                                        response,
                                        SystemClock.Default,
                                        new NullLogger())
                                    .ConfigureAwait(false));
                            }
                            else if (responseType == "application/json")
                            {
                                //
                                // Request failed, but we got a proper OAuth-formatted error.
                                //
                                var error = JsonConvert.DeserializeObject<TokenErrorResponse>(
                                    await response.Content
                                        .ReadAsStringAsync()
                                        .ConfigureAwait(false));

                                throw new TokenAcquisitionException(
                                    $"Authentication failed: {error.ErrorDescription}\n" +
                                    $"Error code: {error.Error}\n" +
                                    $"HTTP Status: {response.StatusCode}");
                            }
                            else if (responseType == "text/html")
                            {
                                //
                                // AD FS returns some errors in HTML format.
                                //
                                var responseBody = await response.Content
                                    .ReadAsStringAsync()
                                    .ConfigureAwait(false);

                                this.Logger.Error(
                                    "Received unexpected response of type {0} from {1}: {2}",
                                    responseType,
                                    configuration.TokenEndpoint,
                                    responseBody);

                                var html = new HtmlResponse(responseBody);
                                if (html.Error != null)
                                {
                                    throw new TokenAcquisitionException(
                                        $"Authentication failed: {html.Error}. See logs for " +
                                        "full response message");
                                }
                                else
                                {
                                    throw new TokenAcquisitionException(
                                        "Authentication failed. The server sent an unexpected response of type " +
                                        $"{responseType}. See logs for " +
                                        "full response message");
                                }
                            }
                            else if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                throw new TokenAcquisitionException(
                                    $"Authentication failed. Verify that the client '{this.ClientId}' " +
                                    $"exists and is configured to allow access to the current AD user.\n\n" +
                                    "If AD FS is deployed behind a load balancer, verify that the " +
                                    "token binding settings (ExtendedProtectionTokenCheck) are compatible " +
                                    "with your load balancer setup.");
                            }
                            else if ((response.StatusCode == HttpStatusCode.BadRequest ||
                                        response.StatusCode == (HttpStatusCode)429) &&
                                    retries < backoff.MaxNumOfRetries)
                            {
                                //
                                // Retry.
                                //
                                this.Logger.Warning("Received Bad Request response, retrying");

                                await Task
                                    .Delay(backoff.DeltaBackOff)
                                    .ConfigureAwait(false);
                            }
                            else
                            {
                                //
                                // Unspecific error.
                                //
                                response.EnsureSuccessStatusCode();

                                throw new TokenAcquisitionException(
                                    $"Authentication failed: {response.StatusCode}");
                            }
                        }
                    }
                }
            }
        }

        public class OidcConfiguration
        {
            [JsonProperty("token_endpoint")]
            public string TokenEndpoint { get; set; }
        }
    }
}
