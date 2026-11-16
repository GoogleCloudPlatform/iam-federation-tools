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

using Google.Solutions.AAAuth.Authorizers;
using Google.Solutions.AAAuth.Iam;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Google.Solutions.AAAuth.Web
{
    public class Startup(IConfiguration configuration)
    {
        private readonly IConfiguration configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            //
            // Read configuration.
            //

            if (this.configuration
                .GetSection("AuthorizedRedirectUris")
                .Get<string[]>()
                is not string[] authorizedRedirectUris)
            {
                throw new InvalidOperationException(
                    $"AuthorizedRedirectUris are missing or incomplete");
            }

            var defaultAuthorizerKey = this.configuration["DefaultAuthorizer"];
            if (string.IsNullOrEmpty(defaultAuthorizerKey))
            {
                defaultAuthorizerKey = GoogleIdentityAuthorizer.Key;
            }

            //
            // Register services.
            //
            services.AddSingleton<IStsClient, StsClient>();
            services.AddSingleton(new OAuthController.Options(
                new HashSet<Uri>(authorizedRedirectUris.Select(u => new Uri(u))),
                defaultAuthorizerKey));

            //
            // Conditionally register the Entra authorizer.
            //
            if (this.configuration
                .GetSection(EntraSection.Key)
                .Get<EntraSection>() is EntraSection entraSection &&
                !string.IsNullOrWhiteSpace(entraSection.TenantId) &&
                WorkforceIdentityProviderName.TryParse(
                    entraSection.WorkforceIdentityProviderName,
                    out var entraWorkforceProvider))
            {
                services.AddSingleton(new EntraDelegatedAuthorizer.Options(
                    Guid.Parse(entraSection.TenantId),
                    entraWorkforceProvider,
                    entraSection.Scopes ?? []));
                services.AddKeyedTransient<IAuthorizer, EntraDelegatedAuthorizer>(
                    EntraDelegatedAuthorizer.Key);
            }

            //
            // Conditionally register the generic delegated authorizer.
            //
            if (this.configuration
                .GetSection(EntraSection.Key)
                .Get<IdpSection>() is IdpSection idpSection &&
                Uri.TryCreate(idpSection.IsserUri, UriKind.Absolute, out var issuerUri) &&
                WorkforceIdentityProviderName.TryParse(
                    idpSection.WorkforceIdentityProviderName,
                    out var idpWorkforceProvider))
            {
                services.AddSingleton(new DelegatedAuthorizer.Options(
                    issuerUri,
                    idpWorkforceProvider,
                    idpSection.Scopes ?? []));
                services.AddKeyedTransient<IAuthorizer, DelegatedAuthorizer>(
                    DelegatedAuthorizer.Key);
            }

            //
            // Register the Google authorizer.
            //
            services.AddKeyedTransient<IAuthorizer, GoogleIdentityAuthorizer>(
                GoogleIdentityAuthorizer.Key);

            // 
            // Register ASP.NET Web API controllers.
            //
            services.AddControllers();
            services.AddLogging(logging =>
            {
                logging.AddConsole(options =>
                {
                    options.FormatterName = CloudRunLogFormatter.FormatterName;
                });
                logging.AddConsoleFormatter<
                    CloudRunLogFormatter,
                    ConsoleFormatterOptions>();
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            // 
            // Configure ASP.NET Web API.
            //
            app.UseStatusCodePages();
            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseExceptionHandler(ErrorController.Path);
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /// <summary>
        /// Section for Entra-specific settings.
        /// </summary>
        public class EntraSection
        {
            /// <summary>
            /// Key used to identify the section.
            /// </summary>
            internal const string Key = "Entra";

            /// <summary>
            /// GUID to identify the Entra tenant.
            /// </summary>
            public string? TenantId { get; set; }

            /// <summary>
            /// Scope to use for OBO flow.
            /// </summary>
            public List<string>? Scopes { get; set; }

            /// <summary>
            /// Name of the workforce identity provider to use for
            /// token exchanges, in format
            /// locations/global/workforcePools/POOL/providers/PROVIDER
            /// </summary>
            public string? WorkforceIdentityProviderName { get; set; }
        }

        /// <summary>
        /// Section for generic IdP settings.
        /// </summary>
        public class IdpSection
        {
            /// <summary>
            /// Key used to identify the section.
            /// </summary>
            internal const string Key = "Idp";

            /// <summary>
            /// Issuer IdP that identifies the IdP.
            /// </summary>
            public string? IsserUri { get; set; }

            /// <summary>
            /// Scope to use for OAuth flow.
            /// </summary>
            public List<string>? Scopes { get; set; }

            /// <summary>
            /// Name of the workforce identity provider to use for
            /// token exchanges, in format
            /// locations/global/workforcePools/POOL/providers/PROVIDER
            /// </summary>
            public string? WorkforceIdentityProviderName { get; set; }
        }
    }
}
