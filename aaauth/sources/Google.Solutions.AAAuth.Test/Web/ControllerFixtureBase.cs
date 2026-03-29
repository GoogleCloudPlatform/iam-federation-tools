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
using Google.Solutions.AAAuth.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http;

namespace Google.Solutions.AAAuth.Test.Web
{
    /// <summary>
    /// Base class for integration tests that target a controller.
    /// </summary>
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public abstract class ControllerFixtureBase
        : WebApplicationFactory<Startup>
    {
        /// <summary>
        /// Pseudo-service account email address.
        /// </summary>
        protected const string ApplicationIdentityEmail = "app@example.com";

        /// <summary>
        /// Fake STS client.
        /// </summary>
        protected Mock<IStsClient> StsClient { get; }
            = new Mock<IStsClient>();

        /// <summary>
        /// Fake authorizer.
        /// </summary>
        protected Mock<IAuthorizer> Authorizer { get; }
            = new Mock<IAuthorizer>();

        /// <summary>
        /// Key of fake authorizer.
        /// </summary>
        protected const string AuthorizerKey = "fake-authorizer";

        protected const string AuthorizedRedirectUri = "http://localhost:5000/authorize/";
        /// <summary>
        /// Configuration for tests.
        /// </summary>
        protected Dictionary<string, string?> Configuration { get; }
            = new Dictionary<string, string?>
            {
                { "ASPNETCORE_URLS", "http://:80/" },
                { "ImpersonateServiceAccount", ApplicationIdentityEmail },
                { "AuthorizedRedirectUris:0", AuthorizedRedirectUri }
            };

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseUrls(
                "http://example.com:5000",
                "https://example.com:5001");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear();
                config.AddInMemoryCollection(this.Configuration);
            });
        }

        /// <summary>
        /// Create an HTTP client for sending synthetic requests to the 
        /// application.
        /// </summary>
        protected HttpClient CreateAuthenticatedClient()
        {
            return
                WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        //
                        // Register fake clients.
                        //
                        services.AddSingleton(this.StsClient.Object);

                        //
                        // Register fake authorizer.
                        //
                        services.AddKeyedSingleton<IAuthorizer>(
                            AuthorizerKey,
                            this.Authorizer.Object);
                    });
                })
                .CreateClient(
                    new WebApplicationFactoryClientOptions
                    {
                        AllowAutoRedirect = false
                    });
        }
    }
}
