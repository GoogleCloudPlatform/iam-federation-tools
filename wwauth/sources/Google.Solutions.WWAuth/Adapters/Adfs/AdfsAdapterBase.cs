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
using System;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.WWAuth.Adapters.Adfs
{
    /// <summary>
    /// Base class for AD FS adapters.
    /// </summary>
    internal abstract class AdfsAdapterBase : ITokenAdapter
    {
        protected ILogger Logger { get; }

        /// <summary>
        /// Base URL of AD FS, typically ending in '/adfs/'.
        /// </summary>
        public Uri IssuerUrl { get; }

        /// <summary>
        /// Kerberos SPN to verify before attempting a token
        /// acquisition.
        /// </summary>
        protected abstract string ServicePrincipalName { get; }

        /// <summary>
        /// User agent to use for IWA requests. Include MSIE and
        /// Trident in compatible versions because these user agents
        /// are on the default "IWA" white-list in AD FS. Other
        /// use agents would require explicit white-listing.
        /// </summary>
        public static string IwaUserAgent
            => $"{UserAgent.Default.Name}/{UserAgent.Default.Version} " +
                "(compatible; MSIE 7.0; MSIE 8.0; MSIE 9.0; MSIE 10.0; Trident/7.0)";

        protected NetworkCredential Credential
            => CredentialCache.DefaultNetworkCredentials;

        protected AdfsAdapterBase(
            Uri issuerUrl,
            ILogger logger)
        {
            if (!issuerUrl.ToString().EndsWith("/"))
            {
                issuerUrl = new Uri(issuerUrl.ToString() + "/");
            }

            this.IssuerUrl = issuerUrl.ThrowIfNull(nameof(issuerUrl));

            this.Logger = logger;
            this.IssuerUrl = issuerUrl;
        }

        /// <summary>
        /// Perform a "pre-flight" check to see if we can connect to AD.
        /// </summary>
        public async Task VerifyActiveDirectoryConnectivity()
        {
            await Task.Run(() =>
            {
                //
                // Check if computer is domain joined at all
                // (irrespective of the user we're running as).
                //
                Domain domain;
                string domainContainer;
                try
                {
                    domain = Domain.GetComputerDomain();
                    domainContainer = domain
                        .GetDirectoryEntry()
                        .Properties["distinguishedName"]
                        .Value as string;

                    this.Logger.Info(
                        "Computer joined to domain '{0}', ({1})",
                        domain.Name,
                        domainContainer);
                }
                catch (Exception e)
                {
                    throw new TokenAcquisitionException(
                        "The current computer is not domain-joined and can't use " +
                        "integrated windows authentication", e);
                }

                //
                // Check if we can lookup the server's SPN. If that fails, we
                // know that obtaining a Kerberos ticket will fail (causing a
                // fallback to NTLM).
                //
                var spn = this.ServicePrincipalName;
                try
                {
                    using (var root = new DirectoryEntry())
                    using (var searcher = new DirectorySearcher(root)
                    {
                        Filter = $"(servicePrincipalName={spn})"
                    })
                    {
                        var result = searcher.FindOne();
                        if (result == null)
                        {
                            throw new ActiveDirectoryObjectNotFoundException(
                                $"SPN '{spn}' not found in directory");
                        }

                        this.Logger.Info("SPN '{0}' resolved to '{1}'", spn, result.Path);
                    }
                }
                catch (Exception e)
                {
                    throw new TokenAcquisitionException(
                        $"The Kerberos SPN '{spn}' does not exist in Active Directory. " +
                        $"Add the SPN to the service account used by AD FS to enable " +
                        $"Kerberos authentication.", e);
                }
            });
        }

        public async Task<ISubjectToken> AcquireTokenAsync(
            TokenAcquisitionOptions options,
            CancellationToken cancellationToken)
        {
            if (options.HasFlag(TokenAcquisitionOptions.ExtensiveValidation))
            {
                await VerifyActiveDirectoryConnectivity()
                    .ConfigureAwait(false);
            }

            return await AcquireTokenCoreAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        protected abstract Task<ISubjectToken> AcquireTokenCoreAsync(
            CancellationToken cancellationToken);

        /// <summary>
        /// Helper class for parsing AD FS responses. 
        /// 
        /// AD FS return errors in HTML format. These HTML pages are 
        /// sometimes not well-formed XML, so it's not possible
        /// to parse them with an XML parser. MSHTML can't be used 
        /// either because it'll conflict with IE ESC which is enabled
        /// on most servers. Thus, use RegEx.
        /// </summary>
        internal class HtmlResponse
        {
            public HtmlResponse(string document)
            {
                document = document
                    .Replace("\n", string.Empty)
                    .Replace("\r", string.Empty);

                this.IsSamlLoginForm = new Regex(@"action=.*SAMLRequest=.*")
                    .IsMatch(document);

                var errorMatch = new Regex(@"(MSIS(\d+):[^<]*)").Match(document);
                if (errorMatch.Success)
                {
                    this.Error = WebUtility.HtmlDecode(errorMatch.Groups[1].Value);
                }

                //
                // Extract input element.
                //
                var input = new Regex("<input.*name\\s*=\\s*\"SAMLResponse\"[^>]*")
                    .Match(document);
                this.IsSamlPostbackForm = input.Success;

                if (this.IsSamlPostbackForm)
                {
                    //
                    // Extract value.
                    //
                    var value = new Regex("value\\s*=\\s*\"([^\"]*)\"").Match(input.Value);
                    if (value.Success)
                    {
                        this.SamlResponse = value.Groups[1].Value;
                    }
                }
            }

            /// <summary>
            /// Check if the page contains a login form for SAML/POST
            /// login.
            /// </summary>
            public bool IsSamlLoginForm { get; }

            /// <summary>
            /// Check if the page contains a postback-form for SAML/POST
            /// login.
            /// </summary>
            public bool IsSamlPostbackForm { get; }

            /// <summary>
            /// Exract SAML response, if any.
            /// </summary>
            public string SamlResponse { get; }

            /// <summary>
            /// Error message, if any.
            /// </summary>
            public string Error { get; }
        }
    }
}
