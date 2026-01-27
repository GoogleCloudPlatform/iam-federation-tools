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

using Google.Apis.Logging;
using Google.Apis.Util;
using Google.Solutions.WWAuth.Data;
using Google.Solutions.WWAuth.Data.Saml2;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.WWAuth.Adapters.Adfs
{
    /// <summary>
    /// Adapter for acquiring tokens using the SAML 2.0 POST binding.
    /// </summary>
    internal class AdfsSamlPostAdapter : AdfsAdapterBase
    {
        internal AuthenticationRequest Request { get; }

        /// <summary>
        /// SPN to verify in pre-flight checks.
        /// </summary>
        protected override string ServicePrincipalName
            => $"http/{this.IssuerUrl.Host}";

        public AdfsSamlPostAdapter(
            Uri issuerUrl,
            string relyingPartyId,
            string acsUrl,
            X509Certificate2 requestSigningCertificate,
            ILogger logger) : base(issuerUrl, logger)
        {
            this.Request = new AuthenticationRequest(
                $"{this.IssuerUrl}ls",
                relyingPartyId,
                acsUrl)
            {
                SigningCertificate = requestSigningCertificate
            };
        }

        protected async override Task<ISubjectToken> AcquireTokenCoreAsync(
            CancellationToken cancellationToken)
        {
            this.Logger.Info("Using SAML endpoint {0}",
                this.Request.Destination);

            if (this.Request.SigningCertificate != null)
            {
                //
                // Try to access the private key to prevent the XML
                // signing process from throwing a non-descript
                // error later.
                //

                if (!this.Request.SigningCertificate.HasPrivateKey)
                {
                    throw new TokenAcquisitionException(
                        $"The certificate '{this.Request.SigningCertificate.Subject}' " +
                        $"(Thumbprint: {this.Request.SigningCertificate.Thumbprint}) " +
                        $"does not have a private key and cannot be used for signing");
                }

                try
                {
                    this.Request.SigningCertificate.GetRSAPrivateKey();
                }
                catch (CryptographicException e)
                {
                    throw new TokenAcquisitionException(
                        $"The private key for the " +
                        $"certificate '{this.Request.SigningCertificate.Subject}' " +
                        $"(Thumbprint: {this.Request.SigningCertificate.Thumbprint}) " +
                        $"is not RSA key, or it cannot be accessed by the current user",
                        e);
                }
            }

            using (var handler = new HttpClientHandler())
            {
                handler.Credentials = this.Credential;

                //
                // Initiate a SAML-POST request like a browser would.
                //
                using (var client = new HttpClient(handler))
                {
                    this.Logger.Info(
                        "Sending SAML AuthnRequest for relying party ID '{0}' " +
                            "and ACS '{1}' using IWA",
                        this.Request.RelyingPartyId,
                        this.Request.AssertionConsumerServiceUrl);

                    //
                    // AD FS occasionally fails requests for no good reason, so use
                    // a backoff/retry loop.
                    //
                    var backoff = new ExponentialBackOff();
                    for (var retries = 0; ; retries++)
                    {
                        var request = new HttpRequestMessage(
                            HttpMethod.Post,
                            $"{this.Request.Destination}?" +
                            $"SAMLRequest={WebUtility.UrlEncode(this.Request.ToString())}");

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

                            //
                            // If the sign-in succeeded, the resulting HTML page must
                            // have a form that includes a field named 'SAMLResponse'. Instead
                            // of POST-ing the form to the ACS, extract the field's value.
                            //
                            if (responseType == "text/html" &&
                                response.StatusCode == HttpStatusCode.OK)
                            {
                                var responseString = await response
                                    .Content
                                    .ReadAsStringAsync()
                                    .ConfigureAwait(false);

                                //
                                // Load HTML document and look for a form.
                                //
                                var html = new HtmlResponse(responseString);
                                if (html.Error != null)
                                {
                                    this.Logger.Error("Response: {0}", responseString);

                                    throw new TokenAcquisitionException(
                                        $"Authentication failed: {html.Error}. See logs for " +
                                        "full response message");
                                }
                                else if (html.IsSamlLoginForm)
                                {
                                    throw new TokenAcquisitionException(
                                        "The server returned a login form instead of performing a silent " +
                                        "login using Kerberos. Verify that:\n" +
                                        "- The authentication method 'Windows Authentication' is enabled\n" +
                                        "- You are connecting directly to AD FS, not via WAP\n" +
                                        $"- WiaSupportedUserAgents includes the " +
                                        $"agent '{AdfsAdapterBase.IwaUserAgent}'");
                                }
                                else if (html.IsSamlPostbackForm && string.IsNullOrEmpty(html.SamlResponse))
                                {
                                    this.Logger.Error("Response: {0}", responseString);

                                    throw new TokenAcquisitionException(
                                        "Failed to extract SAML assertion from postback form." +
                                        "See log for full response.");
                                }
                                else if (html.IsSamlPostbackForm && !string.IsNullOrEmpty(html.SamlResponse))
                                {
                                    this.Logger.Info("Acquiring SAML response succeeded");
                                    return AuthenticationResponse.Parse(html.SamlResponse);
                                }
                                else
                                {
                                    this.Logger.Error("Response: {0}", responseString);

                                    throw new TokenAcquisitionException(
                                        "The server responded with an unrecognized HTML response. " +
                                        "See log for full response.");
                                }
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
    }
}
