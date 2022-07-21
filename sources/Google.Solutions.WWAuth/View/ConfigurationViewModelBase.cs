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

using Google.Apis.Util;
using Google.Solutions.WWAuth.Adapters;
using Google.Solutions.WWAuth.Data;
using System.Diagnostics;
using System.Windows.Forms;

namespace Google.Solutions.WWAuth.View
{
    internal abstract class ConfigurationViewModelBase : ViewModelBase
    {
        private readonly IShellAdapter shellAdapter;

        protected CredentialConfigurationFile File { get; }

        protected ConfigurationViewModelBase(
            CredentialConfigurationFile file,
            IShellAdapter shellAdapter)
        {
            this.File = file.ThrowIfNull(nameof(file));
            this.shellAdapter = shellAdapter.ThrowIfNull(nameof(shellAdapter));
        }

        //---------------------------------------------------------------------
        // Observable properties.
        //---------------------------------------------------------------------

        public bool IsDirty
        {
            get => this.File.IsDirty;
            set
            {
                if (value)
                {
                    this.File.SetDirty();
                }

                RaisePropertyChange();
            }
        }

        //---------------------------------------------------------------------
        // Actions.
        //---------------------------------------------------------------------

        public virtual DialogResult ApplyChanges(IWin32Window owner)
        {
            Debug.Assert(this.IsDirty);

            DialogResult result;
            if (this.File.FilePath == null)
            {
                //
                // Select file to save to.
                //
                result = this.shellAdapter.ShowSaveFileDialog(
                    owner,
                    "Save as",
                    $"Credential configuration (*.{CredentialConfigurationFile.FileExtension})|" +
                        $"*.{CredentialConfigurationFile.FileExtension}|" +
                        "JSON (*.json)|*.json",
                    CredentialConfigurationFile.FileExtension,
                    out var filePath);

                if (result == DialogResult.OK)
                {
                    Debug.Assert(filePath != null);

                    this.File.SaveAs(filePath);
                    Debug.Assert(!this.File.IsDirty);
                }
            }
            else
            {
                this.File.Save();
                Debug.Assert(!this.File.IsDirty);

                result = DialogResult.OK;
            }

            //
            // Raise event so that the Apply button is disabled again.
            //
            RaisePropertyChange((WorkloadIdentityConfigurationViewModel m) => m.IsDirty);

            return result;
        }

        public void ValidateChanges()
        {
            this.File.Configuration.Validate();
        }
    }
}
