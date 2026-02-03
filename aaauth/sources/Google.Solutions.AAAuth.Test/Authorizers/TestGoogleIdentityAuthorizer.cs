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

using Google.Apis.Auth.OAuth2;
using Google.Solutions.AAAuth.Authorizers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.AAAuth.Test.Authorizers
{
    [TestFixture]
    public class TestGoogleIdentityAuthorizer
    {
        //---------------------------------------------------------------------
        // CreateAuthorizeUri.
        //---------------------------------------------------------------------

        [Test]
        public async Task CreateAuthorizeUri_IncludesAccessType()
        {
            var oAuthAuthorizer = new Mock<IAuthorizer>();
            oAuthAuthorizer
                .Setup(a => a.CreateAuthorizeUriAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Uri>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Uri("https://idp/"));

            var authorizer = new GoogleIdentityAuthorizer(
                oAuthAuthorizer.Object,
                new Mock<ILogger<GoogleIdentityAuthorizer>>().Object);

            var url = await authorizer
                .CreateAuthorizeUriAsync(
                    "client-id",
                    "client-secret",
                    null,
                    new Uri("http://example.com/continue"),
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.That(
                url.AbsoluteUri,
                Contains.Substring("access_type=offline"));
        }

        //---------------------------------------------------------------------
        // GetToken.
        //---------------------------------------------------------------------

        [Test]
        public async Task GetToken()
        {
            var clientCredentials = new ClientSecrets()
            {
                ClientId = "client-id"
            };

            var code = "code";
            var redirectUri = new Uri("http://example.com/continue");

            var oAuthAuthorizer = new Mock<IAuthorizer>();
            oAuthAuthorizer
                .Setup(a => a.GetTokenAsync(
                    clientCredentials,
                    code,
                    redirectUri,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResult()
                {
                    IdToken = "id-token"
                });

            var authorizer = new GoogleIdentityAuthorizer(
                oAuthAuthorizer.Object,
                new Mock<ILogger<GoogleIdentityAuthorizer>>().Object);

            var result = await authorizer
                .GetTokenAsync(
                    clientCredentials,
                    code,
                    redirectUri,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.That(result.TokenType, Is.EqualTo("bearer"));
            Assert.That(result.AccessToken, Is.EqualTo("id-token"));
            Assert.That(result.ExpiresInSeconds, Is.EqualTo(3600));
        }
    }
}
