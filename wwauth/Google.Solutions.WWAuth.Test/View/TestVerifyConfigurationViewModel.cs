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

using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Logging;
using Google.Solutions.WWAuth.Adapters;
using Google.Solutions.WWAuth.Data;
using Google.Solutions.WWAuth.View;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.WWAuth.Test.View
{
    [TestFixture]
    public class TestVerifyConfigurationViewModel
    {
        //---------------------------------------------------------------------
        // PerformTestAsync - token acquisition.
        //---------------------------------------------------------------------

        [Test]
        public async Task WhenTokenAcquisitionFails_ThenStatusIsUpdated()
        {
            var tokenAdapter = new Mock<ITokenAdapter>();
            tokenAdapter.Setup(t => t.AcquireTokenAsync(
                    It.IsAny<TokenAcquisitionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("test"));

            var vm = new VerifyConfigurationViewModel(
                tokenAdapter.Object,
                new Mock<IStsAdapter>().Object,
                new Mock<IServiceAccountAdapter>().Object,
                new MemoryLogger(LogLevel.All));

            await vm.PerformTestAsync(
                    CancellationToken.None)
                .ConfigureAwait(true);

            Assert.IsNotNull(vm.AcquireTokenStatusImage);
            Assert.IsNull(vm.ExchangeTokenStatusImage);
            Assert.IsNull(vm.ImpersonateStatusImage);
            Assert.IsFalse(vm.IsShowExternalTokenDetailsLinkEnabled);
        }

        [Test]
        public async Task WhenTokenAcquisitionFails_ThenResultShowsError()
        {
            var tokenAdapter = new Mock<ITokenAdapter>();
            tokenAdapter.Setup(t => t.AcquireTokenAsync(
                    It.IsAny<TokenAcquisitionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ArgumentException("test"));

            var vm = new VerifyConfigurationViewModel(
                tokenAdapter.Object,
                new Mock<IStsAdapter>().Object,
                new Mock<IServiceAccountAdapter>().Object,
                new MemoryLogger(LogLevel.All));

            await vm.PerformTestAsync(
                    CancellationToken.None)
                .ConfigureAwait(true);

            Assert.IsNotNull(vm.ResultImage);
            Assert.That(vm.ResultText, Is.EqualTo("test"));
            Assert.IsTrue(vm.IsResultPanelVisible);
        }

        [Test]
        public async Task WhenTokenAcquisitionSucceeds_ThenTokenDetailsLinkIsVisible()
        {
            var tokenAdapter = new Mock<ITokenAdapter>();
            tokenAdapter.Setup(t => t.AcquireTokenAsync(
                    It.IsAny<TokenAcquisitionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ISubjectToken>().Object);

            var vm = new VerifyConfigurationViewModel(
                tokenAdapter.Object,
                new Mock<IStsAdapter>().Object,
                new Mock<IServiceAccountAdapter>().Object,
                new MemoryLogger(LogLevel.All));

            await vm.PerformTestAsync(
                    CancellationToken.None)
                .ConfigureAwait(true);

            Assert.IsTrue(vm.IsShowExternalTokenDetailsLinkEnabled);
        }

        //---------------------------------------------------------------------
        // PerformTestAsync - token exchange.
        //---------------------------------------------------------------------

        [Test]
        public async Task WhenTokenExchangeFails_ThenStatusIsUpdated()
        {
            var tokenAdapter = new Mock<ITokenAdapter>();
            tokenAdapter.Setup(t => t.AcquireTokenAsync(
                    It.IsAny<TokenAcquisitionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ISubjectToken>().Object);

            var stsAdapter = new Mock<IStsAdapter>();
            stsAdapter.Setup(s => s.ExchangeTokenAsync(
                    It.IsAny<ISubjectToken>(),
                    It.IsAny<IList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TokenAcquisitionException("test"));

            var vm = new VerifyConfigurationViewModel(
                tokenAdapter.Object,
                stsAdapter.Object,
                new Mock<IServiceAccountAdapter>().Object,
                new MemoryLogger(LogLevel.All));

            await vm.PerformTestAsync(
                    CancellationToken.None)
                .ConfigureAwait(true);

            Assert.IsNotNull(vm.AcquireTokenStatusImage);
            Assert.IsNotNull(vm.ExchangeTokenStatusImage);
            Assert.IsNull(vm.ImpersonateStatusImage);
        }

        [Test]
        public async Task WhenTokenExchangeFails_ThenResultShowsError()
        {
            var tokenAdapter = new Mock<ITokenAdapter>();
            tokenAdapter.Setup(t => t.AcquireTokenAsync(
                    It.IsAny<TokenAcquisitionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ISubjectToken>().Object);

            var stsAdapter = new Mock<IStsAdapter>();
            stsAdapter.Setup(s => s.ExchangeTokenAsync(
                    It.IsAny<ISubjectToken>(),
                    It.IsAny<IList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TokenAcquisitionException("test"));

            var vm = new VerifyConfigurationViewModel(
                tokenAdapter.Object,
                stsAdapter.Object,
                new Mock<IServiceAccountAdapter>().Object,
                new MemoryLogger(LogLevel.All));

            await vm.PerformTestAsync(
                    CancellationToken.None)
                .ConfigureAwait(true);

            Assert.IsNotNull(vm.ResultImage);
            Assert.That(vm.ResultText, Is.EqualTo("test"));
            Assert.IsTrue(vm.IsResultPanelVisible);
        }

        //---------------------------------------------------------------------
        // PerformTestAsync - token introspection.
        //---------------------------------------------------------------------

        [Test]
        public async Task WhenTokenIntrospectionFails_ThenStatusIsUpdated()
        {
            var tokenAdapter = new Mock<ITokenAdapter>();
            tokenAdapter.Setup(t => t.AcquireTokenAsync(
                    It.IsAny<TokenAcquisitionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ISubjectToken>().Object);

            var stsAdapter = new Mock<IStsAdapter>();
            stsAdapter.Setup(s => s.ExchangeTokenAsync(
                    It.IsAny<ISubjectToken>(),
                    It.IsAny<IList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResponse());
            stsAdapter.Setup(s => s.IntrospectTokenAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TokenAcquisitionException("test"));

            var vm = new VerifyConfigurationViewModel(
                tokenAdapter.Object,
                stsAdapter.Object,
                new Mock<IServiceAccountAdapter>().Object,
                new MemoryLogger(LogLevel.All));

            await vm.PerformTestAsync(
                    CancellationToken.None)
                .ConfigureAwait(true);

            Assert.IsTrue(vm.IsShowExternalTokenDetailsLinkEnabled);
            Assert.IsFalse(vm.IsShowStsTokenDetailsLinkEnabled);
        }

        [Test]
        public async Task WhenTokenIntrospectionSucceeds_ThenStatusIsUpdated()
        {
            var tokenAdapter = new Mock<ITokenAdapter>();
            tokenAdapter.Setup(t => t.AcquireTokenAsync(
                    It.IsAny<TokenAcquisitionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ISubjectToken>().Object);

            var stsAdapter = new Mock<IStsAdapter>();
            stsAdapter.Setup(s => s.ExchangeTokenAsync(
                    It.IsAny<ISubjectToken>(),
                    It.IsAny<IList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResponse());
            stsAdapter.Setup(s => s.IntrospectTokenAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ISubjectToken>().Object);

            var vm = new VerifyConfigurationViewModel(
                tokenAdapter.Object,
                stsAdapter.Object,
                new Mock<IServiceAccountAdapter>().Object,
                new MemoryLogger(LogLevel.All));

            await vm.PerformTestAsync(
                    CancellationToken.None)
                .ConfigureAwait(true);

            Assert.IsTrue(vm.IsShowExternalTokenDetailsLinkEnabled);
            Assert.IsTrue(vm.IsShowStsTokenDetailsLinkEnabled);
        }

        //---------------------------------------------------------------------
        // PerformTestAsync - impersonation.
        //---------------------------------------------------------------------

        [Test]
        public async Task WhenServiceAccountDoesNotExist_ThenStatusIsUpdated()
        {
            var tokenAdapter = new Mock<ITokenAdapter>();
            tokenAdapter.Setup(t => t.AcquireTokenAsync(
                    It.IsAny<TokenAcquisitionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ISubjectToken>().Object);

            var stsAdapter = new Mock<IStsAdapter>();
            stsAdapter.Setup(s => s.ExchangeTokenAsync(
                    It.IsAny<ISubjectToken>(),
                    It.IsAny<IList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResponse());

            var serviceAccountAdapter = new Mock<IServiceAccountAdapter>();
            serviceAccountAdapter.SetupGet(s => s.IsEnabled)
                .Returns(true);
            serviceAccountAdapter.Setup(s => s.ExistsAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            serviceAccountAdapter.Setup(s => s.ImpersonateAsync(
                        It.IsAny<string>(),
                        It.IsAny<IList<string>>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotImplementedException());

            var vm = new VerifyConfigurationViewModel(
                tokenAdapter.Object,
                stsAdapter.Object,
                serviceAccountAdapter.Object,
                new MemoryLogger(LogLevel.All));

            await vm.PerformTestAsync(
                    CancellationToken.None)
                .ConfigureAwait(true);

            Assert.IsTrue(vm.IsImpersonationStatusVisible);
            Assert.IsFalse(vm.IsShowServiceAccountTokenDetailsLinkEnabled);
            Assert.IsNotNull(vm.AcquireTokenStatusImage);
            Assert.IsNotNull(vm.ExchangeTokenStatusImage);
            Assert.IsNotNull(vm.ImpersonateStatusImage);
        }

        [Test]
        public async Task WhenImpersonationFails_ThenStatusIsUpdated()
        {
            var tokenAdapter = new Mock<ITokenAdapter>();
            tokenAdapter.Setup(t => t.AcquireTokenAsync(
                    It.IsAny<TokenAcquisitionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ISubjectToken>().Object);

            var stsAdapter = new Mock<IStsAdapter>();
            stsAdapter.Setup(s => s.ExchangeTokenAsync(
                    It.IsAny<ISubjectToken>(),
                    It.IsAny<IList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResponse());

            var serviceAccountAdapter = new Mock<IServiceAccountAdapter>();
            serviceAccountAdapter.SetupGet(s => s.IsEnabled)
                .Returns(true);
            serviceAccountAdapter.Setup(s => s.ExistsAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            serviceAccountAdapter.Setup(s => s.ImpersonateAsync(
                        It.IsAny<string>(),
                        It.IsAny<IList<string>>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TokenAcquisitionException("test"));

            var vm = new VerifyConfigurationViewModel(
                tokenAdapter.Object,
                stsAdapter.Object,
                serviceAccountAdapter.Object,
                new MemoryLogger(LogLevel.All));

            await vm.PerformTestAsync(
                    CancellationToken.None)
                .ConfigureAwait(true);

            Assert.IsTrue(vm.IsImpersonationStatusVisible);
            Assert.IsFalse(vm.IsShowServiceAccountTokenDetailsLinkEnabled);
            Assert.IsNotNull(vm.AcquireTokenStatusImage);
            Assert.IsNotNull(vm.ExchangeTokenStatusImage);
            Assert.IsNotNull(vm.ImpersonateStatusImage);
        }

        [Test]
        public async Task WhenImpersonationFails_ThenResultShowsError()
        {
            var tokenAdapter = new Mock<ITokenAdapter>();
            tokenAdapter.Setup(t => t.AcquireTokenAsync(
                    It.IsAny<TokenAcquisitionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ISubjectToken>().Object);

            var stsAdapter = new Mock<IStsAdapter>();
            stsAdapter.Setup(s => s.ExchangeTokenAsync(
                    It.IsAny<ISubjectToken>(),
                    It.IsAny<IList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResponse());

            var serviceAccountAdapter = new Mock<IServiceAccountAdapter>();
            serviceAccountAdapter.SetupGet(s => s.IsEnabled)
                .Returns(true);
            serviceAccountAdapter.Setup(s => s.ExistsAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            serviceAccountAdapter.Setup(s => s.ImpersonateAsync(
                        It.IsAny<string>(),
                        It.IsAny<IList<string>>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TokenAcquisitionException("test"));

            var vm = new VerifyConfigurationViewModel(
                tokenAdapter.Object,
                stsAdapter.Object,
                serviceAccountAdapter.Object,
                new MemoryLogger(LogLevel.All));

            await vm.PerformTestAsync(
                    CancellationToken.None)
                .ConfigureAwait(true);

            Assert.IsTrue(vm.IsImpersonationStatusVisible);
            Assert.IsFalse(vm.IsShowServiceAccountTokenDetailsLinkEnabled);
            Assert.IsNotNull(vm.ResultImage);
            Assert.That(vm.ResultText, Is.EqualTo("test"));
            Assert.IsTrue(vm.IsResultPanelVisible);
        }
        [Test]
        public async Task WhenServiceAccountTokenIntrospectionFails_ThenResultShowsError()
        {
            var tokenAdapter = new Mock<ITokenAdapter>();
            tokenAdapter.Setup(t => t.AcquireTokenAsync(
                    It.IsAny<TokenAcquisitionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ISubjectToken>().Object);

            var stsAdapter = new Mock<IStsAdapter>();
            stsAdapter.Setup(s => s.ExchangeTokenAsync(
                    It.IsAny<ISubjectToken>(),
                    It.IsAny<IList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResponse());

            var serviceAccountAdapter = new Mock<IServiceAccountAdapter>();
            serviceAccountAdapter.SetupGet(s => s.IsEnabled)
                .Returns(true);
            serviceAccountAdapter.Setup(s => s.ExistsAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            serviceAccountAdapter.Setup(s => s.ImpersonateAsync(
                    It.IsAny<string>(),
                    It.IsAny<IList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResponse());
            serviceAccountAdapter.Setup(s => s.IntrospectTokenAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TokenExchangeException("test"));

            var vm = new VerifyConfigurationViewModel(
                tokenAdapter.Object,
                stsAdapter.Object,
                serviceAccountAdapter.Object,
                new MemoryLogger(LogLevel.All));

            await vm.PerformTestAsync(
                    CancellationToken.None)
                .ConfigureAwait(true);

            Assert.IsTrue(vm.IsImpersonationStatusVisible);
            Assert.IsFalse(vm.IsShowServiceAccountTokenDetailsLinkEnabled);
            Assert.IsNotNull(vm.ResultImage);
            Assert.That(vm.ResultText, Is.EqualTo("test"));
            Assert.IsTrue(vm.IsResultPanelVisible);
        }

        [Test]
        public async Task WhenImpersonationSucceeds_ThenResultShowsSuccess()
        {
            var tokenAdapter = new Mock<ITokenAdapter>();
            tokenAdapter.Setup(t => t.AcquireTokenAsync(
                    It.IsAny<TokenAcquisitionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ISubjectToken>().Object);

            var stsAdapter = new Mock<IStsAdapter>();
            stsAdapter.Setup(s => s.ExchangeTokenAsync(
                    It.IsAny<ISubjectToken>(),
                    It.IsAny<IList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResponse());

            var serviceAccountAdapter = new Mock<IServiceAccountAdapter>();
            serviceAccountAdapter.SetupGet(s => s.IsEnabled)
                .Returns(true);
            serviceAccountAdapter.Setup(s => s.ExistsAsync(
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            serviceAccountAdapter.Setup(s => s.ImpersonateAsync(
                        It.IsAny<string>(),
                        It.IsAny<IList<string>>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResponse());

            var vm = new VerifyConfigurationViewModel(
                tokenAdapter.Object,
                stsAdapter.Object,
                serviceAccountAdapter.Object,
                new MemoryLogger(LogLevel.All));

            await vm.PerformTestAsync(
                    CancellationToken.None)
                .ConfigureAwait(true);

            Assert.IsTrue(vm.IsImpersonationStatusVisible);
            Assert.IsTrue(vm.IsShowServiceAccountTokenDetailsLinkEnabled);
            Assert.IsNotNull(vm.ResultImage);
            Assert.That(vm.ResultText, Does.Contain("success"));
            Assert.IsTrue(vm.IsResultPanelVisible);
        }

        [Test]
        public async Task WhenImpersonationDisabled_ThenResultShowsSuccess()
        {
            var tokenAdapter = new Mock<ITokenAdapter>();
            tokenAdapter.Setup(t => t.AcquireTokenAsync(
                    It.IsAny<TokenAcquisitionOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Mock<ISubjectToken>().Object);

            var stsAdapter = new Mock<IStsAdapter>();
            stsAdapter.Setup(s => s.ExchangeTokenAsync(
                    It.IsAny<ISubjectToken>(),
                    It.IsAny<IList<string>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenResponse());

            var serviceAccountAdapter = new Mock<IServiceAccountAdapter>();
            serviceAccountAdapter.SetupGet(s => s.IsEnabled)
                .Returns(false);

            var vm = new VerifyConfigurationViewModel(
                tokenAdapter.Object,
                stsAdapter.Object,
                serviceAccountAdapter.Object,
                new MemoryLogger(LogLevel.All));

            await vm.PerformTestAsync(
                    CancellationToken.None)
                .ConfigureAwait(true);

            Assert.IsFalse(vm.IsImpersonationStatusVisible);
            Assert.IsFalse(vm.IsShowServiceAccountTokenDetailsLinkEnabled);
            Assert.IsNotNull(vm.ResultImage);
            Assert.That(vm.ResultText, Does.Contain("success"));
            Assert.IsTrue(vm.IsResultPanelVisible);

            serviceAccountAdapter.Verify(
                s => s.ExistsAsync(It.IsAny<CancellationToken>()),
                Times.Never);
            serviceAccountAdapter.Verify(
                s => s.ImpersonateAsync(
                    It.IsAny<string>(),
                    It.IsAny<IList<string>>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
