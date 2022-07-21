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

using Google.Apis.Logging;
using Google.Solutions.WWAuth.Adapters;
using Google.Solutions.WWAuth.Data;
using Google.Solutions.WWAuth.Util;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Google.Solutions.WWAuth.View
{
    [SkipCodeCoverage]
    internal partial class VerifyConfigurationDialog : Form
    {
        private readonly IShellAdapter shellAdapter = new ShellAdapter();

        public VerifyConfigurationDialog(
            CredentialConfiguration configuration)
        {
            InitializeComponent();

            var logger = new MemoryLogger(LogLevel.All);
            var viewModel = new VerifyConfigurationViewModel(
                AdapterFactory.CreateTokenAdapter(
                    configuration.Options,
                    logger),
                new StsAdapter(configuration.PoolConfiguration.Audience, logger),
                new ServiceAccountAdapter(configuration.ServiceAccountEmail, logger),
                logger);

            this.BindReadonlyProperty(
                c => c.Text,
                viewModel,
                m => m.WindowTitle,
                this.Container);

            this.acuireTokenPictureBox.BindReadonlyProperty(
                c => c.Image,
                viewModel,
                m => m.AcquireTokenStatusImage,
                this.Container);
            this.exchangeTokenPictureBox.BindReadonlyProperty(
                c => c.Image,
                viewModel,
                m => m.ExchangeTokenStatusImage,
                this.Container);
            this.impersonatePictureBox.BindReadonlyProperty(
                c => c.Image,
                viewModel,
                m => m.ImpersonateStatusImage,
                this.Container);

            this.impersonateLabel.BindReadonlyProperty(
                c => c.Visible,
                viewModel,
                m => m.IsImpersonationStatusVisible,
                this.Container);
            this.impersonatePictureBox.BindReadonlyProperty(
                c => c.Visible,
                viewModel,
                m => m.IsImpersonationStatusVisible,
                this.Container);

            this.resultPanel.BindReadonlyProperty(
                c => c.Visible,
                viewModel,
                m => m.IsResultPanelVisible,
                this.Container);
            this.resultPictureBox.BindReadonlyProperty(
                c => c.Image,
                viewModel,
                m => m.ResultImage,
                this.Container);
            this.resultLabel.BindReadonlyProperty(
                c => c.Text,
                viewModel,
                m => m.ResultText,
                this.Container);

            this.cancelButton.BindReadonlyProperty(
                c => c.Enabled,
                viewModel,
                m => m.IsCancelButtonEnabled,
                this.Container);
            this.okButton.BindReadonlyProperty(
                c => c.Enabled,
                viewModel,
                m => m.IsOkButtonEnabled,
                this.Container);
            this.logsButton.BindReadonlyProperty(
                c => c.Enabled,
                viewModel,
                m => m.IsLogsButtonEnabled,
                this.Container);

            this.showExternalTokenDetailsLink.BindReadonlyProperty(
                c => c.Visible,
                viewModel,
                m => m.IsShowExternalTokenDetailsLinkEnabled,
                this.Container);
            this.showServiceAccountTokenDetailsLink.BindReadonlyProperty(
                c => c.Visible,
                viewModel,
                m => m.IsShowServiceAccountTokenDetailsLinkEnabled,
                this.Container);

            var cancellationSource = new CancellationTokenSource();

            this.Shown += async (sender, args) =>
            {
                await Task.Yield();
                await viewModel
                    .PerformTestAsync(
                        cancellationSource.Token)
                    .ConfigureAwait(true);
            };

            this.cancelButton.Click += (sender, args) =>
            {
                cancellationSource.Cancel();
                this.DialogResult = DialogResult.Cancel;
            };

            this.showExternalTokenDetailsLink.LinkClicked += (sender, args) =>
            {
                using (var prop = new PropertiesDialog())
                {
                    prop.Text = "External token";
                    prop.FormBorderStyle = FormBorderStyle.Sizable;
                    prop.AddSheet(new ViewTokenSheet(viewModel.ExternalToken));
                    prop.ShowDialog(this);
                }
            };

            this.showServiceAccountTokenDetailsLink.LinkClicked += (sender, args) =>
            {
                using (var prop = new PropertiesDialog())
                {
                    prop.Text = "Service account token";
                    prop.FormBorderStyle = FormBorderStyle.Sizable;
                    prop.AddSheet(new ViewTokenSheet(viewModel.ServiceAccountToken));
                    prop.ShowDialog(this);
                }
            };

            this.logsButton.Click += (sender, args) =>
            {
                var logFile = Path.GetTempFileName() + ".txt";
                File.WriteAllText(logFile, viewModel.Logs);

                this.shellAdapter.OpenFile(logFile);
            };
        }

        public new DialogResult ShowDialog(
            IWin32Window owner)
        {
            return base.ShowDialog(owner);
        }
    }
}
