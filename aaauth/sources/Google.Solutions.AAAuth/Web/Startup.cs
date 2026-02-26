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

            //
            // Register services.
            //
            services.AddSingleton<IStsClient, StsClient>();
            services.AddSingleton(new OAuthController.Options(
                new HashSet<Uri>(authorizedRedirectUris.Select(u => new Uri(u)))));

            //
            // Register the Entra authorizer.
            //
            if (this.configuration.GetSection(EntraOptions.Key).Get<EntraOptions>() 
                    is EntraOptions entraOptions &&
                !string.IsNullOrWhiteSpace(entraOptions.TenantId) &&
                WorkforceIdentityProviderName.TryParse(
                    entraOptions.WorkforceIdentityProviderName,
                    out var entraWorkforceProvider))
            {
                services.AddSingleton(new EntraDelegatedAuthorizer.Options(
                    Guid.Parse(entraOptions.TenantId),
                    entraWorkforceProvider,
                    entraOptions.Scopes ?? []));
                services.AddKeyedTransient<IAuthorizer, EntraDelegatedAuthorizer>(
                    EntraDelegatedAuthorizer.Key);
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
        /// Settings for the Entra app registration.
        /// </summary>
        public class EntraOptions
        {
            /// <summary>
            /// Key used to identify the options.
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
    }
}
