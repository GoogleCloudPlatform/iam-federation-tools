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
using Google.Solutions.WWAuth.Interop;
using Google.Solutions.WWAuth.View;
using Moq;
using NUnit.Framework;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Principal;
using System.Windows.Forms;

namespace Google.Solutions.WWAuth.Test.View
{
    [TestFixture]
    public class TestEditConfigurationViewModel
    {
        private static CredentialConfigurationFile NewSampleCredentialConfigurationFile()
        {
            var file = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            file.Configuration.Options.IssuerUrl = "http://issuer/";
            file.Configuration.Options.RelyingPartyId = "http://rp/";
            file.Configuration.Options.Protocol =
                UnattendedCommandLineOptions.AuthenticationProtocol.AdfsSamlPost;

            var poolConfig = (WorkloadIdentityPoolConfiguration)file.Configuration.PoolConfiguration;
            poolConfig.ProjectNumber = 1;
            poolConfig.Location = "local";
            poolConfig.PoolName = "pool-1";
            poolConfig.ProviderName = "provider-1";

            return file;
        }

        //---------------------------------------------------------------------
        // VerifyConfigurationAsUser.
        //---------------------------------------------------------------------

        [Test]
        public void WhenCredentialPromptCancelled_ThenVerifyConfigurationAsUserIsCancelled()
        {
            var shellAdapter = new Mock<IShellAdapter>();
            shellAdapter
                .Setup(a => a.PromptForCredentials(
                        It.IsAny<IWin32Window>(),
                        out It.Ref<NetworkCredential>.IsAny))
                .Returns(DialogResult.Cancel);

            var vm = new EditConfigurationViewModel(
                NewSampleCredentialConfigurationFile(),
                shellAdapter.Object);

            Assert.AreEqual(
                DialogResult.Cancel,
                vm.VerifyConfigurationAsUser(null));
        }

        [Test]
        public void WhenInvalidCredentialsSelected_ThenVerifyConfigurationAsUserThrowsException()
        {
            var shellAdapter = new Mock<IShellAdapter>();
            var credentials = new NetworkCredential("user", "password");
            shellAdapter
                .Setup(a => a.PromptForCredentials(
                    It.IsAny<IWin32Window>(),
                    out credentials))
                .Returns(DialogResult.OK);

            var vm = new EditConfigurationViewModel(
                NewSampleCredentialConfigurationFile(),
                shellAdapter.Object);

            Assert.Throws<IdentityNotMappedException>(
                () => vm.VerifyConfigurationAsUser(null));
        }

        [Test]
        public void WhenExecutableNotAccessibleForUser_ThenVerifyConfigurationAsUserThrowsException()
        {
            var shellAdapter = new Mock<IShellAdapter>();
            var credentials = new NetworkCredential(
                WindowsIdentity.GetCurrent().Name,
                "password");
            shellAdapter
                .Setup(a => a.PromptForCredentials(
                    It.IsAny<IWin32Window>(),
                    out credentials))
                .Returns(DialogResult.OK);
            shellAdapter
                .Setup(a => a.StartProcessAsUser(
                    It.Is<string>(s => s == Program.ExecutablePath),
                    It.IsAny<string>(),
                    It.IsAny<NetworkCredential>()))
                .Throws(new Win32Exception(NativeMethods.ERROR_DIRECTORY));

            var vm = new EditConfigurationViewModel(
                NewSampleCredentialConfigurationFile(),
                shellAdapter.Object);

            Assert.Throws<IOException>(
                () => vm.VerifyConfigurationAsUser(null));
        }

        [Test]
        public void WhenCredentialsSelected_ThenVerifyConfigurationAsUserStartsProcessWithTempFile()
        {
            var shellAdapter = new Mock<IShellAdapter>();
            var credentials = new NetworkCredential(
                WindowsIdentity.GetCurrent().Name,
                "password");
            shellAdapter
                .Setup(a => a.PromptForCredentials(
                    It.IsAny<IWin32Window>(),
                    out credentials))
                .Returns(DialogResult.OK);

            var vm = new EditConfigurationViewModel(
                NewSampleCredentialConfigurationFile(),
                shellAdapter.Object);

            Assert.AreEqual(
                DialogResult.OK,
                vm.VerifyConfigurationAsUser(null));

            shellAdapter.Verify(a => a.StartProcessAsUser(
                    It.Is<string>(s => s == Program.ExecutablePath),
                    It.Is<string>(s => s.Contains("/Verify") && s.Contains(".tmp")),
                    It.Is<NetworkCredential>(c => c == credentials)),
                Times.Once);
        }

        //---------------------------------------------------------------------
        // LaunchGcloud.
        //---------------------------------------------------------------------

        [Test]
        public void LaunchGcloudStartsConsoleCommandWithTempFile()
        {
            var shellAdapter = new Mock<IShellAdapter>();
            var vm = new EditConfigurationViewModel(
                NewSampleCredentialConfigurationFile(),
                shellAdapter.Object);

            vm.LaunchGcloud();

            shellAdapter.Verify(a => a.StartConsoleCommand(
                    It.Is<string>(s => s.Contains(".tmp"))),
                Times.Once);
        }

        //---------------------------------------------------------------------
        // LaunchCommandLineEnvironment.
        //---------------------------------------------------------------------

        [Test]
        public void LaunchCommandLineEnvironmentStartsConsoleCommandWithTempFile()
        {
            var shellAdapter = new Mock<IShellAdapter>();
            var vm = new EditConfigurationViewModel(
                NewSampleCredentialConfigurationFile(),
                shellAdapter.Object);

            vm.LaunchCommandLineEnvironment();

            shellAdapter.Verify(a => a.StartConsoleCommand(
                    It.Is<string>(s => s.Contains(".tmp"))),
                Times.Once);
        }

        //---------------------------------------------------------------------
        // LaunchExecutableCommand.
        //---------------------------------------------------------------------

        [Test]
        public void LaunchExecutableCommandStartsConsoleCommandAndPipesToMore()
        {
            var shellAdapter = new Mock<IShellAdapter>();
            var vm = new EditConfigurationViewModel(
                NewSampleCredentialConfigurationFile(),
                shellAdapter.Object);

            vm.LaunchExecutableCommand();

            shellAdapter.Verify(a => a.StartConsoleCommand(
                    It.Is<string>(s => s.Contains("| more"))),
                Times.Once);
        }
    }
}
