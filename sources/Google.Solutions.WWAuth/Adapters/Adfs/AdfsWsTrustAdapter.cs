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
using System.IdentityModel.Protocols.WSTrust;
using System.IdentityModel.Tokens;
using System.ServiceModel;
using System.ServiceModel.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.WWAuth.Adapters.Adfs
{
    /// <summary>
    /// Adapter for acquiring tokens using WS-Trust.
    /// </summary>
    internal class AdfsWsTrustAdapter : AdfsAdapterBase
    {
        /// <summary>
        /// AD FS Relying Party ID . Typed as string (and not Uri) to prevent
        /// canonicaliuation, which might break the STS token exchange.
        /// </summary>
        public string RelyingPartyId { get; }

        /// <summary>
        /// SPN to verify in pre-flight checks.
        /// </summary>
        protected override string ServicePrincipalName
            => $"HOST/{this.IssuerUrl.Host}";   // WS-Trust needs host/, not http/!

        public AdfsWsTrustAdapter(
            Uri issuerUrl,
            string relyingPartyId,
            ILogger logger) : base(issuerUrl, logger)
        {
            if (!Uri.IsWellFormedUriString(relyingPartyId, UriKind.Absolute))
            {
                throw new ArgumentException("Relying party ID must be a URI");
            }

            this.RelyingPartyId = relyingPartyId.ThrowIfNull(nameof(relyingPartyId));
        }

        /// <summary>
        /// Create a WS-Trust channel factory that uses the given credential
        /// to authenticate to AD FS.
        /// </summary>
        private WSTrustChannelFactory CreateChannelFactory()
        {
            var binding = new WS2007HttpBinding(SecurityMode.TransportWithMessageCredential);
            binding.Security.Message.EstablishSecurityContext = false;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

            //
            // Use Integrated Windows Authentication (IWA).
            //
            binding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;

            var factory = new WSTrustChannelFactory(
                binding,
                new EndpointAddress(
                    new Uri(this.IssuerUrl, "services/trust/13/windowsmixed")))
            {
                TrustVersion = TrustVersion.WSTrust13
            };

            factory.Credentials.Windows.ClientCredential = this.Credential;
            return factory;
        }

        /// <summary>
        /// Use WS-Trust to obtain a SAML assertion from AD FS.
        /// </summary>
        private async Task<GenericXmlSecurityToken> AcquireSamlSecurityTokenAsync(
            WSTrustChannelFactory factory)
        {
            //
            // Request a SAML 2.0 assertion (as opposed to SAML 1.1, which is
            // the default for WS-Trust).
            //
            var tokenRequest = new RequestSecurityToken
            {
                RequestType = RequestTypes.Issue,
                AppliesTo = new EndpointReference(this.RelyingPartyId),
                KeyType = KeyTypes.Bearer,
                TokenType = "urn:oasis:names:tc:SAML:2.0:assertion"
            };

            var channel = factory.CreateChannel();

            try
            {
                this.Logger.Info(
                    "Acquiring SAML assertion for {0} and relying party {1} using WS-Trust",
                    factory.Credentials.UserName,
                    factory.Endpoint.Address);

                return await Task.Factory.FromAsync(
                    channel.BeginIssue(tokenRequest, null, null),
                    ar => (GenericXmlSecurityToken)channel.EndIssue(ar, out var _));
            }
            catch (SecurityNegotiationException e)
            {
                throw new TokenAcquisitionException(
                    "Authentication failed. " +
                    "If AD FS is deployed behind a load balancer, verify that the " +
                    "token binding settings (ExtendedProtectionTokenCheck) are compatible " +
                    "with your load balancer setup.", e);
            }
            catch (FaultException e) when (
                e.Code != null &&
                e.Code.IsSenderFault &&
                e.Code.SubCode.Name == "InvalidScope")
            {
                throw new TokenAcquisitionException(
                    $"The relying party ID '{this.RelyingPartyId}' " +
                    "is invalid or does not exist", e);
            }
            catch (FaultException e) when (
                e.Code != null &&
                e.Code.IsSenderFault &&
                e.Code.SubCode.Name == "FailedAuthentication" &&
                factory.Credentials.UserName?.UserName != null)
            {
                throw new TokenAcquisitionException(
                    "Authentication failed, verify that the credentials " +
                    $"for {factory.Credentials.UserName.UserName} are correct", e);
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Acquiring assertion failed: {0}", e.Message);
                throw;
            }
        }

        protected override async Task<ISubjectToken> AcquireTokenCoreAsync(
            CancellationToken cancellationToken)
        {
            var samlToken = await AcquireSamlSecurityTokenAsync(
                    CreateChannelFactory())
                .ConfigureAwait(false);
            return new Assertion(samlToken);
        }
    }
}
