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
using System;
using System.ComponentModel;

namespace Google.Solutions.WWAuth.View
{
    [SkipCodeCoverage]
    internal partial class EditConfigurationDialog : PropertiesDialog
    {
        private readonly EditConfigurationViewModel viewModel;

        public EditConfigurationDialog(CredentialConfigurationFile file)
        {
            InitializeComponent();

            viewModel = new EditConfigurationViewModel(
                file,
                new ShellAdapter());

            this.BindReadonlyProperty(
                c => c.Text,
                viewModel,
                m => m.WindowTitle,
                this.Container);

            AddSheet(new AdfsConfigurationSheet(file));

            if (file.Configuration.PoolConfiguration is WorkloadIdentityPoolConfiguration)
            {
                AddSheet(new WorkloadIdentityConfigurationSheet(file));
            }
            else
            {
                throw new NotImplementedException(
                    "Unrecognized pool configuration: This tool only supports " +
                    "workload identity federation.");
            }
        }

        private void verifyButton_Click(object sender, EventArgs _)
            => InvokeAction(
                "Verifying configuration",
                () =>
                {
                    Validate();
                    this.viewModel.VerifyConfiguration(this);
                });

        private void verifyAsUserMenuItem_Click(object sender, EventArgs e)
            => InvokeAction(
                "Verifying configuration",
                () =>
                {
                    Validate();
                    this.viewModel.VerifyConfigurationAsUser(this);
                });

        private void gcloudMenuItem_Click(object sender, EventArgs e)
            => InvokeAction(
                "Launching gcloud",
                () => this.viewModel.LaunchGcloud());

        private void adcMenuItem_Click(object sender, EventArgs e)
            => InvokeAction(
                "Launching command line environment",
                () => this.viewModel.LaunchCommandLineEnvironment());

        private void showOutputMenuItem_Click(object sender, EventArgs e)
            => InvokeAction(
                "Launching executable command",
                () => this.viewModel.LaunchExecutableCommand());

        private void EditConfigurationDialog_HelpButtonClicked(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            using (var dialog = new AboutDialog())
            {
                dialog.ShowDialog(this);
            }
        }
    }
}
