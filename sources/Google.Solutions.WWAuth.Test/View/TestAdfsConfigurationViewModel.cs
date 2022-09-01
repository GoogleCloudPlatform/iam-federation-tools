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

using Google.Solutions.WWAuth.Adapters;
using Google.Solutions.WWAuth.Data;
using Google.Solutions.WWAuth.View;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace Google.Solutions.WWAuth.Test.View
{
    [TestFixture]
    public class TestAdfsConfigurationViewModel : TestConfigurationViewModelBase
    {
#if NET472_OR_GREATER
        private static X509Certificate2 CreateSelfSignedCertificate()
        {
            return new CertificateRequest(
                    "CN=test",
                    RSA.Create(),
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1)
                .CreateSelfSigned(
                    DateTimeOffset.UtcNow.AddMinutes(-5),
                    DateTimeOffset.UtcNow.AddMinutes(5));
        }
#endif

        //---------------------------------------------------------------------
        // IssuerUrl.
        //---------------------------------------------------------------------

        [Test]
        public void WhenIssuerUrlChanged_ThenEventIsRaisedAndDirtyIsSet()
        {
            var config = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            config.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsOidc;
            config.Configuration.Options.RelyingPartyId = "rp-id";

            var vm = new AdfsConfigurationViewModel(
                config,
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);

            Assert.IsFalse(vm.IsDirty);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IssuerUrl,
                () => vm.IssuerUrl = "value");
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsDirty,
                () => vm.IssuerUrl = "value");

            Assert.IsTrue(vm.IsDirty);
        }

        //---------------------------------------------------------------------
        // RelyingPartyId.
        //---------------------------------------------------------------------

        [Test]
        public void WhenRelyingPartyIdChanged_ThenEventIsRaisedAndDirtyIsSet()
        {
            var config = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            config.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsOidc;
            config.Configuration.Options.RelyingPartyId = "rp-id";

            var vm = new AdfsConfigurationViewModel(
                config,
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);

            Assert.IsFalse(vm.IsDirty);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.RelyingPartyId,
                () => vm.RelyingPartyId = "value");
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsDirty,
                () => vm.RelyingPartyId = "value");

            Assert.IsTrue(vm.IsDirty);
        }

        //---------------------------------------------------------------------
        // ClientId.
        //---------------------------------------------------------------------

        [Test]
        public void WhenClientIdChanged_ThenEventIsRaisedAndDirtyIsSet()
        {
            var config = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            config.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsOidc;
            config.Configuration.Options.RelyingPartyId = "rp-id";

            var vm = new AdfsConfigurationViewModel(
                config,
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);

            Assert.IsFalse(vm.IsDirty);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.ClientId,
                () => vm.ClientId = "value");
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsDirty,
                () => vm.ClientId = "value");

            Assert.IsTrue(vm.IsDirty);
        }

        //---------------------------------------------------------------------
        // AvailableProtocols.
        //---------------------------------------------------------------------

        [Test]
        public void AvailableProtocolsContainsProtocolNames()
        {
            var vm = new AdfsConfigurationViewModel(
                CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile(),
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);

            CollectionAssert.Contains(vm.AvailableProtocols, "AdfsOidc");
            CollectionAssert.Contains(vm.AvailableProtocols, "AdfsWsTrust");
            CollectionAssert.Contains(vm.AvailableProtocols, "AdfsSamlPost");
        }

        //---------------------------------------------------------------------
        // ProtocolIndex.
        //---------------------------------------------------------------------

        [Test]
        public void WhenProtocolIndexChanged_ThenEventIsRaisedAndDirtyIsSet()
        {
            var config = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            config.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsOidc;
            config.Configuration.Options.RelyingPartyId = "rp-id";

            var vm = new AdfsConfigurationViewModel(
                config,
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);

            Assert.IsFalse(vm.IsDirty);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.ProtocolIndex,
                () => vm.ProtocolIndex = 1);
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsDirty,
                () => vm.ProtocolIndex = 0);
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsClientIdTextBoxVisible,
                () => vm.ProtocolIndex = 0);
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsAcsUrlTextBoxVisible,
                () => vm.ProtocolIndex = 0);

            Assert.IsTrue(vm.IsDirty);
        }

        //---------------------------------------------------------------------
        // Signing.
        //---------------------------------------------------------------------

        [Test]
        public void WhenProtocolIsOidc_ThenSigningControlsAreNotVisible()
        {
            var config = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            config.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsOidc;
            config.Configuration.Options.RelyingPartyId = "rp-id";

            var vm = new AdfsConfigurationViewModel(
                config,
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);

            Assert.IsFalse(vm.IsSignRequestControlVisible);
        }

        [Test]
        public void WhenProtocolIsWsTrust_ThenSigningControlsAreNotVisible()
        {
            var config = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            config.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsWsTrust;
            config.Configuration.Options.RelyingPartyId = "rp-id";

            var vm = new AdfsConfigurationViewModel(
                config,
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);

            Assert.IsFalse(vm.IsSignRequestControlVisible);
        }

        [Test]
        public void WhenCommandLineContainsNoCertificate_ThenSigningControlsAreDisabled()
        {
            var config = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            config.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsSamlPost;
            config.Configuration.Options.RelyingPartyId = "rp-id";
            config.Configuration.Options.SamlRequestSigningCertificate = null;

            var vm = new AdfsConfigurationViewModel(
                config,
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);

            vm.ReapplyProtocolDefaults();

            Assert.IsTrue(vm.IsSignRequestControlVisible);
            Assert.IsFalse(vm.IsSignRequestControlEnabled);
            Assert.IsNull(vm.SigningCertificateSubject);
            Assert.IsNull(vm.RequestSigningCertificate);
            Assert.IsFalse(vm.IsViewCertificateMenuItemEnabled);
        }

        [Test]
        public void WhenCommandLineContainsNonexistingCertificate_ThenSigningControlsAreEnabled()
        {
            var config = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            config.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsSamlPost;
            config.Configuration.Options.RelyingPartyId = "rp-id";
            config.Configuration.Options.SamlRequestSigningCertificate = "AAAA";

            var certStoreAdapter = new Mock<ICertificateStoreAdapter>();
            certStoreAdapter
                .Setup(a => a.TryGetSigningCertificate(It.IsAny<string>()))
                .Returns((X509Certificate2)null);

            var vm = new AdfsConfigurationViewModel(
                config,
                new Mock<IShellAdapter>().Object,
                certStoreAdapter.Object);

            Assert.IsTrue(vm.IsSignRequestControlVisible);
            Assert.IsFalse(vm.IsSignRequestControlEnabled);
            Assert.IsNull(vm.SigningCertificateSubject);
            Assert.IsNull(vm.RequestSigningCertificate);
            Assert.IsFalse(vm.IsViewCertificateMenuItemEnabled);
        }

        [Test]
        public void WhenCommandLineContainsValidCertificate_ThenSigningControlsAreEnabled()
        {
#if NET472_OR_GREATER
            var config = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            config.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsSamlPost;
            config.Configuration.Options.RelyingPartyId = "rp-id";
            config.Configuration.Options.SamlRequestSigningCertificate = "AAAA";

            var cert = CreateSelfSignedCertificate();
            var certStoreAdapter = new Mock<ICertificateStoreAdapter>();
            certStoreAdapter
                .Setup(a => a.TryGetSigningCertificate(It.Is<string>(h => h == "AAAA")))
                .Returns(cert);

            var vm = new AdfsConfigurationViewModel(
                config,
                new Mock<IShellAdapter>().Object,
                certStoreAdapter.Object);

            Assert.IsTrue(vm.IsSignRequestControlVisible);
            Assert.IsTrue(vm.IsSignRequestControlEnabled);
            Assert.AreEqual("CN=test", vm.SigningCertificateSubject);
            Assert.AreSame(cert, vm.RequestSigningCertificate);
            Assert.IsTrue(vm.IsViewCertificateMenuItemEnabled);
#else
            Assert.Inconclusive("Test requires .NET 4.7.2+");
#endif
        }

        [Test]
        public void WhenDisablingSigning_ThenCertfificateIsSetToNull()
        {
#if NET472_OR_GREATER
            var config = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            config.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsSamlPost;
            config.Configuration.Options.RelyingPartyId = "rp-id";
            config.Configuration.Options.SamlRequestSigningCertificate = "AAAA";

            var cert = CreateSelfSignedCertificate();
            var certStoreAdapter = new Mock<ICertificateStoreAdapter>();
            certStoreAdapter
                .Setup(a => a.TryGetSigningCertificate(It.Is<string>(h => h == "AAAA")))
                .Returns(cert);

            var vm = new AdfsConfigurationViewModel(
                config,
                new Mock<IShellAdapter>().Object,
                certStoreAdapter.Object);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.SigningCertificateSubject,
                () => vm.IsSignRequestControlEnabled = false);
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.RequestSigningCertificate,
                () => vm.IsSignRequestControlEnabled = false);
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.SigningCertificateSubject,
                () => vm.IsSignRequestControlEnabled = false);

            Assert.IsTrue(vm.IsSignRequestControlVisible);
            Assert.IsFalse(vm.IsSignRequestControlEnabled);
            Assert.IsNull(vm.SigningCertificateSubject);
            Assert.IsNull(vm.RequestSigningCertificate);
            Assert.IsFalse(vm.IsViewCertificateMenuItemEnabled);
#else
            Assert.Inconclusive("Test requires .NET 4.7.2+");
#endif
        }

        //---------------------------------------------------------------------
        // ApplyChanges.
        //---------------------------------------------------------------------

        [Test]
        public void WhenFilePathSet_ThenApplyChangesClearsDirtyBit()
        {
            var file = NewSampleWorkloadIdentityPoolConfigurationFile();
            file.SaveAs(Path.GetTempFileName());

            var vm = new AdfsConfigurationViewModel(
                file,
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object)
            {
                IssuerUrl = "url"
            };

            Assert.IsTrue(vm.IsDirty);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsDirty,
                () => vm.ApplyChanges(null));
            Assert.IsFalse(vm.IsDirty);
        }

        [Test]
        public void WhenFilePathNotSetAndFileDialogCancelled_ThenApplyChangesAborts()
        {
            var file = new CredentialConfigurationFile(
                null,
                NewSampleConfigurartion());

            var shellAdapter = new Mock<IShellAdapter>();
            shellAdapter.Setup(s => s.ShowSaveFileDialog(
                    It.IsAny<IWin32Window>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    out It.Ref<string>.IsAny))
                .Returns(DialogResult.Cancel);

            var vm = new AdfsConfigurationViewModel(
                file,
                shellAdapter.Object,
                new Mock<ICertificateStoreAdapter>().Object)
            {
                IssuerUrl = "url"
            };

            Assert.IsTrue(vm.IsDirty);

            Assert.AreEqual(DialogResult.Cancel, vm.ApplyChanges(null));
            Assert.IsTrue(vm.IsDirty);
        }

        //---------------------------------------------------------------------
        // ValidateChanges.
        //---------------------------------------------------------------------

        [Test]
        public void WhenPropertiesNull_ThenValidateChangesThrowsException()
        {
            var vm = new AdfsConfigurationViewModel(
                CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile(),
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);

            Assert.Throws<InvalidCredentialConfigurationException>(() => vm.ValidateChanges());
        }

        //---------------------------------------------------------------------
        // ReapplyProtocolDefaults.
        //---------------------------------------------------------------------

        [Test]
        public void WhenProtolIsOidcAndPoolNotConfigured_ThenDefaultsAreApplied()
        {
            var file = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            file.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsOidc;

            var vm = new AdfsConfigurationViewModel(
                file,
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);
            vm.ReapplyProtocolDefaults();

            Assert.IsNull(vm.AcsUrl);
            Assert.IsNull(vm.ClientId);
            Assert.IsNull(vm.RelyingPartyId);

            Assert.IsFalse(vm.IsDirty);
        }

        [Test]
        public void WhenProtolIsOidcAndPoolConfigured_ThenDefaultsAreApplied()
        {
            var file = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            file.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsOidc;

            var poolConfig = (WorkloadIdentityPoolConfiguration)file.Configuration.PoolConfiguration;
            poolConfig.ProjectNumber = 123;
            poolConfig.ProviderName = "p-1";
            poolConfig.PoolName = "p-1"; ;

            var vm = new AdfsConfigurationViewModel(
                file,
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);
            vm.ReapplyProtocolDefaults();

            Assert.IsNull(vm.AcsUrl);
            Assert.IsNull(vm.ClientId);
            Assert.AreEqual(vm.DefaultRelyingPartyId, vm.RelyingPartyId);

            Assert.IsTrue(vm.IsDirty);
        }

        [Test]
        public void WhenProtolIsWsTrust_ThenDefaultsAreApplied()
        {
            var file = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            file.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsWsTrust;

            var vm = new AdfsConfigurationViewModel(
                file,
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);
            vm.ReapplyProtocolDefaults();

            Assert.AreEqual(vm.DefaultRelyingPartyId, vm.RelyingPartyId);
            Assert.IsNull(vm.AcsUrl);
            Assert.IsNull(vm.ClientId);

            Assert.IsTrue(vm.IsDirty);
        }

        [Test]
        public void WhenProtolIsSamlPost_ThenDefaultsAreApplied()
        {
            var file = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            file.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsSamlPost;

            var vm = new AdfsConfigurationViewModel(
                file,
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);
            vm.ReapplyProtocolDefaults();

            Assert.AreEqual(vm.DefaultRelyingPartyId, vm.RelyingPartyId);
            Assert.AreEqual(StsAdapter.DefaultTokenUrl, vm.AcsUrl);
            Assert.IsNull(vm.ClientId);

            Assert.IsTrue(vm.IsDirty);
        }

        [Test]
        public void WhenApplyingChanges_DefaultsAreReapplied()
        {
            var file = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            file.Configuration.Options.IssuerUrl = "http://issuer/";
            file.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsSamlPost;

            var vm = new AdfsConfigurationViewModel(
                file,
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);
            vm.ReapplyProtocolDefaults();

            StringAssert.EndsWith("/-", vm.RelyingPartyId);

            //
            // Change the provider name, which affects the relying party ID.
            //
            var poolConfig = (WorkloadIdentityPoolConfiguration)
                file.Configuration.PoolConfiguration;
            poolConfig.ProjectNumber = 1;
            poolConfig.PoolName = "pool";
            poolConfig.ProviderName = "changed";

            vm.ApplyChanges(null);
            StringAssert.EndsWith("/changed", vm.RelyingPartyId);
        }

        //---------------------------------------------------------------------
        // ResetExecutable.
        //---------------------------------------------------------------------

        [Test]
        public void WhenExecutableReset_ThenEventIsRaisedAndDirtyIsSet()
        {
            var vm = new AdfsConfigurationViewModel(
                CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile(),
                new Mock<IShellAdapter>().Object,
                new Mock<ICertificateStoreAdapter>().Object);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsDirty,
                () => vm.ResetExecutable());

            Assert.IsTrue(vm.IsDirty);
        }
    }
}
