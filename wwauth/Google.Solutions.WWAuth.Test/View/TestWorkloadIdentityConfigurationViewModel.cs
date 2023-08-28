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
using System.IO;
using System.Windows.Forms;

namespace Google.Solutions.WWAuth.Test.View
{
    [TestFixture]
    public class TestWorkloadIdentityConfigurationViewModel : TestConfigurationViewModelBase
    {
        //---------------------------------------------------------------------
        // ProjectNumber.
        //---------------------------------------------------------------------

        [Test]
        public void WhenProjectNumberChanged_ThenEventIsRaisedAndDirtyIsSet()
        {
            var vm = new WorkloadIdentityConfigurationViewModel(
                CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile(),
                new Mock<IShellAdapter>().Object);

            Assert.IsFalse(vm.IsDirty);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.ProjectNumber,
                () => vm.ProjectNumber = "value");
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsDirty,
                () => vm.ProjectNumber = "value");
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.Audience,
                () => vm.ProjectNumber = "value");

            Assert.IsTrue(vm.IsDirty);
        }

        //---------------------------------------------------------------------
        // PoolId.
        //---------------------------------------------------------------------

        [Test]
        public void WhenPoolIdChanged_ThenEventIsRaisedAndDirtyIsSet()
        {
            var vm = new WorkloadIdentityConfigurationViewModel(
                CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile(),
                new Mock<IShellAdapter>().Object);

            Assert.IsFalse(vm.IsDirty);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.PoolId,
                () => vm.PoolId = "value");
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsDirty,
                () => vm.PoolId = "value");
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.Audience,
                () => vm.PoolId = "value");

            Assert.IsTrue(vm.IsDirty);
        }

        //---------------------------------------------------------------------
        // ProviderId.
        //---------------------------------------------------------------------

        [Test]
        public void WhenProviderIdChanged_ThenEventIsRaisedAndDirtyIsSet()
        {
            var vm = new WorkloadIdentityConfigurationViewModel(
                CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile(),
                new Mock<IShellAdapter>().Object);

            Assert.IsFalse(vm.IsDirty);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.ProviderId,
                () => vm.ProviderId = "value");
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsDirty,
                () => vm.ProviderId = "value");
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.Audience,
                () => vm.ProviderId = "value");

            Assert.IsTrue(vm.IsDirty);
        }

        //---------------------------------------------------------------------
        // IsImpersonateServiceAccountEnabled.
        //---------------------------------------------------------------------

        [Test]
        public void WhenConfigurationContainsServiceAccount_ThenIsImpersonateServiceAccountEnabledIsTrue()
        {
            var file = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            file.Configuration.ServiceAccountEmail = "sa@example.iam.gserviceaccount.com";

            var vm = new WorkloadIdentityConfigurationViewModel(
                file,
                new Mock<IShellAdapter>().Object);

            Assert.IsTrue(vm.IsImpersonateServiceAccountEnabled);
        }

        [Test]
        public void WhenConfigurationLacksServiceAccount_ThenIsImpersonateServiceAccountEnabledIsFalse(
            [Values(null, "")] string missingValue)
        {
            var file = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            file.Configuration.ServiceAccountEmail = missingValue;

            var vm = new WorkloadIdentityConfigurationViewModel(
                file,
                new Mock<IShellAdapter>().Object);

            Assert.IsFalse(vm.IsImpersonateServiceAccountEnabled);
        }

        [Test]
        public void WhenDisablingImpersonateServiceAccount_ThenServiceAccountIsCleared()
        {
            var file = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            file.Configuration.ServiceAccountEmail = "sa@example.iam.gserviceaccount.com";

            var vm = new WorkloadIdentityConfigurationViewModel(
                file,
                new Mock<IShellAdapter>().Object);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                v => v.ServiceAccountEmail,
                () => vm.IsImpersonateServiceAccountEnabled = false);

            Assert.AreEqual(string.Empty, file.Configuration.ServiceAccountEmail);
        }

        //---------------------------------------------------------------------
        // ServiceAccountEmail.
        //---------------------------------------------------------------------

        [Test]
        public void WhenServiceAccountEmailChanged_ThenEventIsRaisedAndDirtyIsSet()
        {
            var vm = new WorkloadIdentityConfigurationViewModel(
                CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile(),
                new Mock<IShellAdapter>().Object);

            Assert.IsFalse(vm.IsDirty);

            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.ServiceAccountEmail,
                () => vm.ServiceAccountEmail = "value");
            PropertyAssert.RaisesPropertyChangeNotification(
                vm,
                m => m.IsDirty,
                () => vm.ServiceAccountEmail = "value");

            Assert.IsTrue(vm.IsDirty);
        }

        //---------------------------------------------------------------------
        // ApplyChanges.
        //---------------------------------------------------------------------

        [Test]
        public void WhenFilePathSet_ThenApplyChangesClearsDirtyBit()
        {
            var file = NewSampleWorkloadIdentityPoolConfigurationFile();
            file.SaveAs(Path.GetTempFileName());

            var vm = new WorkloadIdentityConfigurationViewModel(
                file,
                new Mock<IShellAdapter>().Object)
            {
                ProjectNumber = "123"
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

            var vm = new WorkloadIdentityConfigurationViewModel(
                file,
                shellAdapter.Object)
            {
                ProjectNumber = "123"
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
            var vm = new WorkloadIdentityConfigurationViewModel(
                CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile(),
                new Mock<IShellAdapter>().Object);

            Assert.Throws<InvalidCredentialConfigurationException>(() => vm.ValidateChanges());
        }
    }
}
