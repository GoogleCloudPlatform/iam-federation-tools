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

using Google.Solutions.WWAuth.Data;
using System.IO;

namespace Google.Solutions.WWAuth.Test.View
{
    public abstract class TestConfigurationViewModelBase
    {
        internal virtual CredentialConfiguration NewSampleConfigurartion()
            => new CredentialConfiguration(
                    new WorkloadIdentityPoolConfiguration()
                    {
                        ProjectNumber = 123,
                        PoolName = "pool-1",
                        ProviderName = "provider-1"
                    },
                    new UnattendedCommandLineOptions()
                    {
                        Executable = "test.exe",
                        Protocol = UnattendedCommandLineOptions.AuthenticationProtocol.AdfsOidc,
                        IssuerUrl = "https://example.com/",
                        RelyingPartyId = "https://example.com/"
                    })
            {
                ServiceAccountEmail = "sa@ex.iam.gserviceaccount.com"
            };

        internal virtual CredentialConfigurationFile NewSampleWorkloadIdentityPoolConfigurationFile()
            => new CredentialConfigurationFile(
                Path.GetTempFileName(),
                NewSampleConfigurartion());
    }
}
