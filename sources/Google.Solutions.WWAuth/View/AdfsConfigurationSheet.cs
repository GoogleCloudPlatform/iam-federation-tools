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
using Google.Solutions.WWAuth.Util;
using System.Windows.Forms;

namespace Google.Solutions.WWAuth.View
{
    [SkipCodeCoverage]
    internal partial class AdfsConfigurationSheet : UserControl, IPropertiesSheet
    {
        private readonly AdfsConfigurationViewModel viewModel;
        private readonly ICertificateStoreAdapter certificateStoreAdapter
            = new CertificateStoreAdapter();

        public AdfsConfigurationSheet(
            CredentialConfigurationFile file)
        {
            InitializeComponent();

            this.viewModel = new AdfsConfigurationViewModel(
                file,
                new ShellAdapter(),
                this.certificateStoreAdapter);

            this.adfsIssuerUriText.BindProperty(
                c => c.Text,
                this.viewModel,
                m => m.IssuerUrl,
                this.Container);
            this.protocolComboBox.Items.AddRange(this.viewModel.AvailableProtocols);
            this.protocolComboBox.BindProperty(
                c => c.SelectedIndex,
                this.viewModel,
                m => m.ProtocolIndex,
                this.Container);
            this.rpIdTextBox.BindProperty(
                c => c.Text,
                this.viewModel,
                m => m.RelyingPartyId,
                this.Container);
            this.rpIdTextBox.BindReadonlyProperty(
                c => c.ReadOnly,
                this.viewModel,
                m => m.IsRelyingPartyIdTextBoxReadonly,
                this.Container);

            this.clientIdLabel.BindReadonlyProperty(
                c => c.Visible,
                this.viewModel,
                m => m.IsClientIdTextBoxVisible,
                this.Container);
            this.clientIdTextBox.BindReadonlyProperty(
                c => c.Visible,
                this.viewModel,
                m => m.IsClientIdTextBoxVisible,
                this.Container);
            this.clientIdTextBox.BindProperty(
                c => c.Text,
                this.viewModel,
                m => m.ClientId,
                this.Container);

            this.acsUrlLabel.BindReadonlyProperty(
                c => c.Visible,
                this.viewModel,
                m => m.IsAcsUrlTextBoxVisible,
                this.Container);
            this.acsUrlTextBox.BindReadonlyProperty(
                c => c.Visible,
                this.viewModel,
                m => m.IsAcsUrlTextBoxVisible,
                this.Container);
            this.acsUrlTextBox.BindProperty(
                c => c.Text,
                this.viewModel,
                m => m.AcsUrl,
                this.Container);

            this.signRequestCheckBox.BindReadonlyProperty(
                c => c.Visible,
                this.viewModel,
                m => m.IsSignRequestControlVisible,
                this.Container);
            this.signRequestCheckBox.BindProperty(
                c => c.Checked,
                this.viewModel,
                m => m.IsSignRequestControlEnabled,
                this.Container);

            this.signingCertificateTextBox.BindReadonlyProperty(
                c => c.Visible,
                this.viewModel,
                m => m.IsSignRequestControlVisible,
                this.Container);
            this.signingCertificateTextBox.BindReadonlyProperty(
                c => c.Enabled,
                this.viewModel,
                m => m.IsSignRequestControlEnabled,
                this.Container);
            this.signingCertificateTextBox.BindReadonlyProperty(
                c => c.Text,
                this.viewModel,
                m => m.SigningCertificateSubject,
                this.Container);

            this.browseCertificateButton.BindReadonlyProperty(
                c => c.Visible,
                this.viewModel,
                m => m.IsSignRequestControlVisible,
                this.Container);
            this.browseCertificateButton.BindReadonlyProperty(
                c => c.Enabled,
                this.viewModel,
                m => m.IsSignRequestControlEnabled,
                this.Container);

            this.viewCertificateMenuItem.BindReadonlyProperty(
                c => c.Enabled,
                this.viewModel,
                m => m.IsViewCertificateMenuItemEnabled,
                this.Container);
        }

        public IPropertiesSheetViewModel ViewModel => this.viewModel;

        public void OnActivated()
        {
            this.viewModel.ReapplyProtocolDefaults();
        }

        private void browseCertificateButton_Click(object sender, System.EventArgs e)
        {
            this.viewModel.BrowseForRequestSigningCertificate(this);
        }

        private void viewCertificateMenuItem_Click(object sender, System.EventArgs e)
        {
            this.viewModel.ViewRequestSigningCertificate(this);
        }
    }
}
