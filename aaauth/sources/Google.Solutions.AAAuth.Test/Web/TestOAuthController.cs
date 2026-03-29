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
using Google.Solutions.AAAuth.Web;
using Moq;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.AAAuth.Test.Web
{
    [TestFixture]
    public class TestOAuthController : ControllerFixtureBase
    {
        //---------------------------------------------------------------------
        // GetOpenIdConfiguration.
        //---------------------------------------------------------------------

        [Test]
        public async Task GetOpenIdConfiguration_WhenAuthorizerNotFound()
        {
            var response = await CreateAuthenticatedClient()
                .GetAsync($"/unknown/.well-known/openid-configuration")
                .ConfigureAwait(false);

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task GetOpenIdConfiguration()
        {
            this.Authorizer
                .Setup(a => a.GetMetadataAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AuthorizerMetadata(
                    new Uri("http://example.com/issuer"),
                    new Uri("http://example.com/jwks"),
                    ["pairwise"],
                    ["RS256"]));

            var response = await CreateAuthenticatedClient()
                .GetAsync($"/{AuthorizerKey}/.well-known/openid-configuration")
                .ConfigureAwait(false);

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.OK));
            Assert.That(
                response.Content.Headers.ContentType?.MediaType,
                Is.EqualTo("application/json"));

            var configuration = (await response
                .Content
                .ReadFromJsonAsync<OpenIdProviderMetadata>())!;

            Assert.That(
                configuration.Issuer,
                Is.EqualTo("http://example.com/issuer"));
            Assert.That(
                configuration.JsonWebKeySetEndpoint,
                Is.EqualTo("http://example.com/jwks"));

            Assert.That(
                configuration?.AuthorizationEndpoint,
                Is.EqualTo($"https://localhost/{AuthorizerKey}/authorize"));
            Assert.That(
                configuration?.TokenEndpoint,
                Is.EqualTo($"https://localhost/{AuthorizerKey}/token"));
        }

        //---------------------------------------------------------------------
        // BeginAuthorize.
        //---------------------------------------------------------------------

        [Test]
        public async Task BeginAuthorize_WhenAuthorizerNotFound()
        {
            var response = await CreateAuthenticatedClient()
                .GetAsync($"/unknown/authorize")
                .ConfigureAwait(false);

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task BeginAuthorize_WhenRedirectUriNotAllowed(
            [Values("http://unknown/")] string uri)
        {
            var response = await CreateAuthenticatedClient()
                .GetAsync(
                    $"/{AuthorizerKey}/authorize" +
                    $"?redirect_uri={WebUtility.UrlEncode(uri)}")
                .ConfigureAwait(false);

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task BeginAuthorize_WhenResponseTypeNotAllowed(
            [Values("invalid")] string responseType)
        {
            var response = await CreateAuthenticatedClient()
                .GetAsync(
                    $"/{AuthorizerKey}/authorize" +
                    $"?redirect_uri={AuthorizedRedirectUri}" +
                    $"&response_type={responseType}" +
                    $"&client_id=client-1")
                .ConfigureAwait(false);

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.Redirect));
            Assert.That(
                response.Headers.Location?.AbsoluteUri,
                Does.Contain("error=unsupported_response_type"));
        }

        [Test]
        public async Task BeginAuthorize_WhenClientIdMissing()
        {
            var response = await CreateAuthenticatedClient()
                .GetAsync(
                    $"/{AuthorizerKey}/authorize" +
                    $"?redirect_uri={AuthorizedRedirectUri}" +
                    $"&response_type=code" +
                    $"&client_id=")
                .ConfigureAwait(false);

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.Redirect));
            Assert.That(
                response.Headers.Location?.AbsoluteUri,
                Does.Contain("error=unauthorized_client"));
        }

        [Test]
        public async Task BeginAuthorize()
        {
            this.Authorizer
                .Setup(a => a.CreateAuthorizeUriAsync(
                    "client-1",
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Uri("http://example.com/authorize/"));

            var response = await CreateAuthenticatedClient()
                .GetAsync(
                    $"/{AuthorizerKey}/authorize" +
                    $"?redirect_uri={AuthorizedRedirectUri}" +
                    $"&client_id=client-1")
                .ConfigureAwait(false);

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.Found));
            Assert.That(
                response.Headers.Location?.AbsoluteUri,
                Is.EqualTo("http://example.com/authorize/"));
        }

        //---------------------------------------------------------------------
        // ContinueAuthorize.
        //---------------------------------------------------------------------

        [Test]
        public async Task ContinueAuthorize_WhenAuthorizerNotFound()
        {
            var response = await CreateAuthenticatedClient()
                .GetAsync($"/unknown/continue?code=x&state=y")
                .ConfigureAwait(false);

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task ContinueAuthorize_WhenRedirectUriNotAllowed(
            [Values("http://unknown/")] string uri)
        {
            var response = await CreateAuthenticatedClient()
                .GetAsync(
                    $"/{AuthorizerKey}/continue" +
                    $"?state={WebUtility.UrlEncode(new PackedParameter(
                        new Uri(uri),
                        string.Empty).ToString())}" +
                    "&code=x")
                .ConfigureAwait(false);

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task ContinueAuthorize()
        {
            var response = await CreateAuthenticatedClient()
                .GetAsync(
                    $"/{AuthorizerKey}/continue" +
                    $"?state={WebUtility.UrlEncode(new PackedParameter(
                        new Uri(AuthorizedRedirectUri),
                        "state-1").ToString())}" +
                    "&code=x")
                .ConfigureAwait(false);

            Assert.That(
                response.StatusCode,
                Is.EqualTo(HttpStatusCode.Redirect));
            Assert.That(
                response.Headers.Location?.AbsoluteUri,
                Does.StartWith(AuthorizedRedirectUri));
            Assert.That(
                response.Headers.Location?.AbsoluteUri,
                Does.Contain("state=state-1"));
            Assert.That(
                response.Headers.Location?.AbsoluteUri,
                Does.Contain("code=x"));
        }
    }
}
