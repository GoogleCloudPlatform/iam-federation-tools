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
using Google.Apis.Logging;
using Google.Apis.Util;
using Google.Solutions.WWAuth.Adapters.Adfs;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Google.Solutions.WWAuth.Adapters
{
    internal static class AdapterFactory
    {
        private static X509Certificate2 GetCertificate(string thumbprint)
        {
            return new CertificateStoreAdapter()
                .TryGetSigningCertificate(thumbprint)
                ?? throw new ArgumentException(
                    $"Certificate '{thumbprint}' does not exist in computer certificate store " +
                    $"or is not suitable for signing");
        }

        public static ITokenAdapter CreateTokenAdapter(
            UnattendedCommandLineOptions options,
            ILogger logger)
        {
            switch (options.Protocol)
            {
                case UnattendedCommandLineOptions.AuthenticationProtocol.AdfsOidc:
                    options.IssuerUrl.ThrowIfNull(nameof(UnattendedCommandLineOptions.IssuerUrl));
                    options.OidcClientId.ThrowIfNull(nameof(UnattendedCommandLineOptions.OidcClientId));
                    options.RelyingPartyId.ThrowIfNull(nameof(UnattendedCommandLineOptions.RelyingPartyId));

                    return new AdfsOidcAdapter(
                        new Uri(options.IssuerUrl),
                        options.OidcClientId,
                        options.RelyingPartyId,
                        logger);

                case UnattendedCommandLineOptions.AuthenticationProtocol.AdfsWsTrust:
                    options.IssuerUrl.ThrowIfNull(nameof(UnattendedCommandLineOptions.IssuerUrl));
                    options.RelyingPartyId.ThrowIfNull(nameof(UnattendedCommandLineOptions.RelyingPartyId));

                    return new AdfsWsTrustAdapter(
                        new Uri(options.IssuerUrl),
                        options.RelyingPartyId,
                        logger);

                case UnattendedCommandLineOptions.AuthenticationProtocol.AdfsSamlPost:
                    options.IssuerUrl.ThrowIfNull(nameof(UnattendedCommandLineOptions.IssuerUrl));
                    options.RelyingPartyId.ThrowIfNull(nameof(UnattendedCommandLineOptions.RelyingPartyId));

                    return new AdfsSamlPostAdapter(
                        new Uri(options.IssuerUrl),
                        options.RelyingPartyId,
                        options.SamlAcsUrl,
                        string.IsNullOrEmpty(options.SamlRequestSigningCertificate)
                            ? null
                            : GetCertificate(options.SamlRequestSigningCertificate),
                        logger);

                default:
                    throw new ArgumentException("Unknown protocol: " + options.Protocol);
            }
        }

        public static ClientSecrets ClientSecrets
        {
            get
            {
                //
                // Use credentials from environment variable, if available.
                //
                if (Environment.GetEnvironmentVariable("WWAUTH_CLIENT_SECRET")
                    is var credentials &&
                    !string.IsNullOrEmpty(credentials) &&
                    credentials.Split(':') is var credentialParts &&
                    credentialParts.Length == 2)
                {
                    return new ClientSecrets()
                    {
                        ClientId = credentialParts[0],
                        ClientSecret = credentialParts[1]
                    };
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
