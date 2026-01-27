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

            Assert.That(vm.IsDirty, Is.False);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IssuerUrl,
                () => vm.IssuerUrl = "value");
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsDirty,
                () => vm.IssuerUrl = "value");

            Assert.That(vm.IsDirty, Is.True);
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

            Assert.That(vm.IsDirty, Is.False);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.RelyingPartyId,
                () => vm.RelyingPartyId = "value");
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsDirty,
                () => vm.RelyingPartyId = "value");

            Assert.That(vm.IsDirty, Is.True);
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

            Assert.That(vm.IsDirty, Is.False);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.ClientId,
                () => vm.ClientId = "value");
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsDirty,
                () => vm.ClientId = "value");

            Assert.That(vm.IsDirty, Is.True);
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

            Assert.That(vm.AvailableProtocols, Does.Contain("AdfsOidc"));
            Assert.That(vm.AvailableProtocols, Does.Contain("AdfsWsTrust"));
            Assert.That(vm.AvailableProtocols, Does.Contain("AdfsSamlPost"));
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

            Assert.That(vm.IsDirty, Is.False);

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

            Assert.That(vm.IsDirty, Is.True);
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

            Assert.That(vm.IsSignRequestControlVisible, Is.False);
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

            Assert.That(vm.IsSignRequestControlVisible, Is.False);
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

            Assert.That(vm.IsSignRequestControlVisible, Is.True);
            Assert.That(vm.IsSignRequestControlEnabled, Is.False);
            Assert.That(vm.SigningCertificateSubject, Is.Null);
            Assert.That(vm.RequestSigningCertificate, Is.Null);
            Assert.That(vm.IsViewCertificateMenuItemEnabled, Is.False);
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

            Assert.That(vm.IsSignRequestControlVisible, Is.True);
            Assert.That(vm.IsSignRequestControlEnabled, Is.False);
            Assert.That(vm.SigningCertificateSubject, Is.Null);
            Assert.That(vm.RequestSigningCertificate, Is.Null);
            Assert.That(vm.IsViewCertificateMenuItemEnabled, Is.False);
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

            Assert.That(vm.IsSignRequestControlVisible, Is.True);
            Assert.That(vm.IsSignRequestControlEnabled, Is.True);
            Assert.That(vm.SigningCertificateSubject, Is.EqualTo("CN=test"));
            Assert.That(vm.RequestSigningCertificate, Is.SameAs(cert));
            Assert.That(vm.IsViewCertificateMenuItemEnabled, Is.True);
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

            Assert.That(vm.IsSignRequestControlVisible, Is.True);
            Assert.That(vm.IsSignRequestControlEnabled, Is.False);
            Assert.That(vm.SigningCertificateSubject, Is.Null);
            Assert.That(vm.RequestSigningCertificate, Is.Null);
            Assert.That(vm.IsViewCertificateMenuItemEnabled, Is.False);
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

            Assert.That(vm.IsDirty, Is.True);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsDirty,
                () => vm.ApplyChanges(null));
            Assert.That(vm.IsDirty, Is.False);
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

            Assert.That(vm.IsDirty, Is.True);

            Assert.That(vm.ApplyChanges(null), Is.EqualTo(DialogResult.Cancel));
            Assert.That(vm.IsDirty, Is.True);
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

            Assert.That(() => vm.ValidateChanges(), Throws.InstanceOf<InvalidCredentialConfigurationException>());
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

            Assert.That(vm.AcsUrl, Is.Null);
            Assert.That(vm.ClientId, Is.Null);
            Assert.That(vm.RelyingPartyId, Is.Null);

            Assert.That(vm.IsDirty, Is.False);
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

            Assert.That(vm.AcsUrl, Is.Null);
            Assert.That(vm.ClientId, Is.Null);
            Assert.That(vm.RelyingPartyId, Is.EqualTo(vm.DefaultRelyingPartyId));

            Assert.That(vm.IsDirty, Is.True);
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

            Assert.That(vm.RelyingPartyId, Is.EqualTo(vm.DefaultRelyingPartyId));
            Assert.That(vm.AcsUrl, Is.Null);
            Assert.That(vm.ClientId, Is.Null);

            Assert.That(vm.IsDirty, Is.True);
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

            Assert.That(vm.RelyingPartyId, Is.EqualTo(vm.DefaultRelyingPartyId));
            Assert.That(vm.AcsUrl, Is.EqualTo(StsAdapter.DefaultTokenUrl));
            Assert.That(vm.ClientId, Is.Null);

            Assert.That(vm.IsDirty, Is.True);
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

            Assert.That(vm.RelyingPartyId, Does.EndWith("/-"));

            //
            // Change the provider name, which affects the relying party ID.
            //
            var poolConfig = (WorkloadIdentityPoolConfiguration)
                file.Configuration.PoolConfiguration;
            poolConfig.ProjectNumber = 1;
            poolConfig.PoolName = "pool";
            poolConfig.ProviderName = "changed";

            vm.ApplyChanges(null);
            Assert.That(vm.RelyingPartyId, Does.EndWith("/changed"));
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

            Assert.That(vm.IsDirty, Is.True);
        }
    }
}
