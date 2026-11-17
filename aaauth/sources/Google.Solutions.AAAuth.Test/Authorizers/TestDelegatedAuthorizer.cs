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
using Google.Solutions.AAAuth.Iam;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.AAAuth.Test.Authorizers
{
    [TestFixture]
    public class TestDelegatedAuthorizer
    {
        private static DelegatedAuthorizer.Options CreateOptions(
            string? scope = null)
        {
            return new DelegatedAuthorizer.Options(
                new Uri("https://idp/"),
                new WorkforceIdentityProviderName("pool-1", "provider-1"),
                scope != null
                    ? [scope] :
                    Array.Empty<string>());
        }

        //---------------------------------------------------------------------
        // RedeemAuthorizationCode.
        //---------------------------------------------------------------------

        [Test]
        public async Task RedeemAuthorizationCode_ExchangesIdToken()
        {
            var clientCredentials = new ClientSecrets()
            {
                ClientId = "client-id"
            };

            var code = "code";
            var redirectUri = new Uri("http://example.com/continue");

            var oAuthAuthorizer = new Mock<IAuthorizer>();
            oAuthAuthorizer
                .Setup(a => a.RedeemAuthorizationCodeAsync(
                    clientCredentials,
                    code,
                    redirectUri,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResult()
                {
                    IdToken = "id-token",
                    RefreshToken = "refresh-token"
                });

            var stsClient = new Mock<IStsClient>();
            stsClient
                .Setup(c => c.ExchangeTokenAsync(
                    It.IsAny<WorkforceIdentityProviderName>(),
                    "id-token",
                    new string[] { Scopes.CloudPlatform },
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResult()
                {
                    TokenType = "bearer",
                    AccessToken = "sts-token",
                    ExpiresInSeconds = 60
                });

            var authorizer = new DelegatedAuthorizer(
                stsClient.Object,
                CreateOptions(),
                oAuthAuthorizer.Object,
                new Mock<ILogger<DelegatedAuthorizer>>().Object);

            var result = await authorizer
                .RedeemAuthorizationCodeAsync(
                    clientCredentials,
                    code,
                    redirectUri,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.That(result.TokenType, Is.EqualTo("bearer"));
            Assert.That(result.AccessToken, Is.EqualTo("sts-token"));
            Assert.That(result.RefreshToken, Is.EqualTo("refresh-token"));
            Assert.That(result.ExpiresInSeconds, Is.GreaterThanOrEqualTo(60));
        }

        [Test]
        public void RedeemAuthorizationCode_WhenIdTokenEmpty()
        {
            var clientCredentials = new ClientSecrets()
            {
                ClientId = "client-id"
            };

            var code = "code";
            var redirectUri = new Uri("http://example.com/continue");

            var oAuthAuthorizer = new Mock<IAuthorizer>();
            oAuthAuthorizer
                .Setup(a => a.RedeemAuthorizationCodeAsync(
                    clientCredentials,
                    code,
                    redirectUri,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResult()
                {
                    AccessToken = "access-token",
                    IdToken = string.Empty,
                });

            var authorizer = new DelegatedAuthorizer(
                new Mock<IStsClient>().Object,
                CreateOptions(),
                oAuthAuthorizer.Object,
                new Mock<ILogger<DelegatedAuthorizer>>().Object);

            Assert.That(async () => await authorizer
                .RedeemAuthorizationCodeAsync(
                    clientCredentials,
                    code,
                    redirectUri,
                    CancellationToken.None),
                Throws.TypeOf<ArgumentException>());
        }

        //---------------------------------------------------------------------
        // RedeemRefreshToken.
        //---------------------------------------------------------------------

        [Test]
        public async Task RedeemRefreshToken_ExchangesIdToken()
        {
            var clientCredentials = new ClientSecrets()
            {
                ClientId = "client-id"
            };

            var refreshToken = "refresh-token";
            var oAuthAuthorizer = new Mock<IAuthorizer>();
            oAuthAuthorizer
                .Setup(a => a.RedeemRefreshTokenAsync(
                    clientCredentials,
                    refreshToken,
                    null,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResult()
                {
                    IdToken = "id-token",
                    RefreshToken = "new-refresh-token"
                });

            var stsClient = new Mock<IStsClient>();
            stsClient
                .Setup(c => c.ExchangeTokenAsync(
                    It.IsAny<WorkforceIdentityProviderName>(),
                    "id-token",
                    new string[] { Scopes.CloudPlatform },
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResult()
                {
                    TokenType = "bearer",
                    AccessToken = "sts-token",
                    ExpiresInSeconds = 60
                });

            var authorizer = new DelegatedAuthorizer(
                stsClient.Object,
                CreateOptions(),
                oAuthAuthorizer.Object,
                new Mock<ILogger<DelegatedAuthorizer>>().Object);

            var result = await authorizer
                .RedeemRefreshTokenAsync(
                    clientCredentials,
                    refreshToken,
                    null,
                    CancellationToken.None)
                .ConfigureAwait(false);

            Assert.That(result.TokenType, Is.EqualTo("bearer"));
            Assert.That(result.AccessToken, Is.EqualTo("sts-token"));
            Assert.That(result.RefreshToken, Is.EqualTo("new-refresh-token"));
            Assert.That(result.ExpiresInSeconds, Is.GreaterThanOrEqualTo(60));
        }

    }
}
