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
using System;

namespace Google.Solutions.WWAuth.View
{
    internal class WorkloadIdentityConfigurationViewModel
        : ConfigurationViewModelBase, IPropertiesSheetViewModel
    {
        private readonly WorkloadIdentityPoolConfiguration poolConfiguration;

        private bool isImpersonateServiceAccountEnabled;

        public WorkloadIdentityConfigurationViewModel(
            CredentialConfigurationFile file,
            IShellAdapter shellAdapter)
            : base(file, shellAdapter)
        {
            this.poolConfiguration = (WorkloadIdentityPoolConfiguration)
                file.Configuration.PoolConfiguration;

            this.isImpersonateServiceAccountEnabled =
                !string.IsNullOrEmpty(file.Configuration.ServiceAccountEmail);
        }

        public string Title => "Workload identity";

        //---------------------------------------------------------------------
        // Observable properties.
        //---------------------------------------------------------------------

        public string ProjectNumber
        {
            get => this.poolConfiguration.ProjectNumber.ToString();
            set
            {
                if (ulong.TryParse(value.Trim(), out var number))
                {
                    this.poolConfiguration.ProjectNumber = number;
                }
                else
                {
                    this.poolConfiguration.ProjectNumber = null;
                }

                this.IsDirty = true;
                RaisePropertyChange();
                RaisePropertyChange((WorkloadIdentityConfigurationViewModel m) => m.Audience);
            }
        }

        public string PoolId
        {
            get => this.poolConfiguration.PoolName;
            set
            {
                if (this.poolConfiguration == null)
                {
                    throw new InvalidOperationException("Invalid pool type");
                }

                this.poolConfiguration.PoolName = value.Trim();
                this.IsDirty = true;
                RaisePropertyChange();
                RaisePropertyChange((WorkloadIdentityConfigurationViewModel m) => m.Audience);
            }
        }

        public string ProviderId
        {
            get => this.poolConfiguration.ProviderName;
            set
            {
                if (this.poolConfiguration == null)
                {
                    throw new InvalidOperationException("Invalid pool type");
                }

                this.poolConfiguration.ProviderName = value.Trim();
                this.IsDirty = true;
                RaisePropertyChange();
                RaisePropertyChange((WorkloadIdentityConfigurationViewModel m) => m.Audience);
            }
        }

        public string ServiceAccountEmail
        {
            get => this.File.Configuration.ServiceAccountEmail;
            set
            {
                this.File.Configuration.ServiceAccountEmail = value.Trim();
                this.IsDirty = true;
                RaisePropertyChange();
            }
        }

        public bool IsImpersonateServiceAccountEnabled
        {
            get => this.isImpersonateServiceAccountEnabled;
            set
            {
                this.isImpersonateServiceAccountEnabled = value;

                if (!value)
                {
                    this.ServiceAccountEmail = string.Empty;
                }

                RaisePropertyChange();
            }
        }

        public string Audience => this.File.Configuration.PoolConfiguration.Audience;
    }
}
