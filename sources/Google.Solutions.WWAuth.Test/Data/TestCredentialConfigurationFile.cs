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
using NUnit.Framework;
using System;
using System.IO;

namespace Google.Solutions.WWAuth.Test.Data
{
    [TestFixture]
    public class TestCredentialConfigurationFile
    {
        private const string SampleServiceAccountEmail = "sa@example.iam.gserviceaccount.com";

        private static WorkloadIdentityPoolConfiguration SampleWorkloadIdentityPoolConfiguration
            => new WorkloadIdentityPoolConfiguration()
            {
                ProjectNumber = 1,
                Location = "local",
                PoolName = "pool-1",
                ProviderName = "provider-1"
            };

        private static UnattendedCommandLineOptions SampleCommandLineOptions
            => new UnattendedCommandLineOptions()
            {
                Protocol = UnattendedCommandLineOptions.AuthenticationProtocol.AdfsOidc,
                Executable = "test.exe",
                IssuerUrl = "https://example.com/adfs/",
                RelyingPartyId = "https://rp.example.com/",
                OidcClientId = "client-1"
            };

        //---------------------------------------------------------------------
        // Save.
        //---------------------------------------------------------------------

        [Test]
        public void WhenFilePathNotSet_ThenSaveThrowsException()
        {
            var file = CredentialConfigurationFile.NewWorkloadIdentityConfigurationFile();
            Assert.Throws<InvalidOperationException>(
                () => file.Save());
        }

        [Test]
        public void WhenFilePathSet_ThenSaveClearsDirtyBit()
        {
            var file = new CredentialConfigurationFile(
                Path.GetTempFileName(),
                new CredentialConfiguration(
                    SampleWorkloadIdentityPoolConfiguration,
                    SampleCommandLineOptions)
                {
                    ServiceAccountEmail = SampleServiceAccountEmail
                });

            file.SetDirty();
            Assert.IsTrue(file.IsDirty);

            file.Save();
            Assert.IsFalse(file.IsDirty);
        }

        //---------------------------------------------------------------------
        // SaveAs.
        //---------------------------------------------------------------------

        [Test]
        public void WhenFilePathNotSet_ThenSaveAsSucceeds()
        {
            var file = new CredentialConfigurationFile(
                null,
                new CredentialConfiguration(
                    SampleWorkloadIdentityPoolConfiguration,
                    SampleCommandLineOptions)
                {
                    ServiceAccountEmail = SampleServiceAccountEmail
                });

            var filePath = Path.GetTempFileName();
            file.SaveAs(filePath);

            CredentialConfigurationFile.FromFile(filePath);
        }

        //---------------------------------------------------------------------
        // FromFile.
        //---------------------------------------------------------------------

        [Test]
        public void WhenFileDoesNotExist_ThenFromFileThrowsException()
        {
            Assert.Throws<FileNotFoundException>(
                () => CredentialConfigurationFile.FromFile("doesnotexist"));
        }

        [Test]
        public void WhenFileEmpty_ThenFromFileThrowsException()
        {
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, string.Empty);

            Assert.Throws<UnknownCredentialConfigurationException>(
                () => CredentialConfigurationFile.FromFile(filePath));
        }

        [Test]
        public void WhenFileContainsMalformedData_ThenFromFileThrowsException()
        {
            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, "not json");

            Assert.Throws<InvalidCredentialConfigurationFileException>(
                () => CredentialConfigurationFile.FromFile(filePath));
        }

        [Test]
        public void WhenFileContainsAwsConfiguration_ThenFromFileThrowsException()
        {
            var json = @"
            {
              'type': 'external_account',
              'audience': '//iam.googleapis.com/projects/1/locations/global/workloadIdentityPools/POOL_ID/providers/PROVIDER_ID',
              'subject_token_type': 'urn:ietf:params:aws:token-type:aws4_request',
              'service_account_impersonation_url': 'https://iamcredentials.googleapis.com/v1/projects/-/serviceAccounts/EMAIL:generateAccessToken',
              'token_url': 'https://sts.googleapis.com/v1/token',
              'credential_source': {
                'environment_id': 'aws1',
                'region_url': 'http://169.254.169.254/latest/meta-data/placement/availability-zone',
                'url': 'http://169.254.169.254/latest/meta-data/iam/security-credentials',
                'regional_cred_verification_url': 'https://sts.{region}.amazonaws.com?Action=GetCallerIdentity&Version=2011-06-15',
                'imdsv2_session_token_url': 'http://169.254.169.254/latest/api/token'
              }
            }";

            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, json);

            Assert.Throws<InvalidCredentialConfigurationFileException>(
                () => CredentialConfigurationFile.FromFile(filePath));
        }

        [Test]
        public void WhenFileContainsUrlSourcedCredentialConfiguration_ThenFromFileThrowsException()
        {
            var json = @"
            {
              'type': 'external_account',
              'audience': '//iam.googleapis.com/projects/2/locations/global/workloadIdentityPools/POOL_ID/providers/PROVIDER_ID',
              'subject_token_type': 'urn:ietf:params:oauth:token-type:jwt',
              'service_account_impersonation_url': 'https://iamcredentials.googleapis.com/v1/projects/-/serviceAccounts/EMAIL:generateAccessToken',
              'token_url': 'https://sts.googleapis.com/v1/token',
              'credential_source': {
                'headers': {
                    'Metadata': 'True'
                },
                'url': 'http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=https://iam.googleapis.com/projects/PROJECT_NUMBER/locations/global/workloadIdentityPools/POOL_ID/providers/PROVIDER_ID',
                'format': {
                  'type': 'json',
                  'subject_token_field_name': 'access_token'
                }
              }
            }";

            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, json);

            Assert.Throws<InvalidCredentialConfigurationFileException>(
                () => CredentialConfigurationFile.FromFile(filePath));
        }

        [Test]
        public void WhenFileContainsFileSourcedCredentialConfiguration_ThenFromFileThrowsException()
        {
            var json = @"
            {
              'type': 'external_account',
              'audience': '//iam.googleapis.com/projects/2/locations/global/workloadIdentityPools/POOL_ID/providers/PROVIDER_ID',
              'subject_token_type': 'urn:ietf:params:oauth:token-type:jwt',
              'service_account_impersonation_url': 'https://iamcredentials.googleapis.com/v1/projects/-/serviceAccounts/EMAIL:generateAccessToken',
              'token_url': 'https://sts.googleapis.com/v1/token',
              'credential_source': {
                'file': '/var/run/saml/assertion/token'
              }
            }";

            var filePath = Path.GetTempFileName();
            File.WriteAllText(filePath, json);

            Assert.Throws<InvalidCredentialConfigurationFileException>(
                () => CredentialConfigurationFile.FromFile(filePath));
        }

        //---------------------------------------------------------------------
        // Clone.
        //---------------------------------------------------------------------

        [Test]
        public void WhenCloned_ThenConfigurationIsNotSameAndFilePathIsNull()
        {
            var file = new CredentialConfigurationFile(
                Path.GetTempFileName(),
                new CredentialConfiguration(
                    SampleWorkloadIdentityPoolConfiguration,
                    SampleCommandLineOptions)
                {
                    ServiceAccountEmail = SampleServiceAccountEmail
                });

            var clone = file.Clone();
            Assert.AreNotSame(file, clone);
            Assert.AreNotSame(file.Configuration, clone.Configuration);

            Assert.IsNull(clone.FilePath);
        }

        [Test]
        public void WhenCloned_ThenChangesDontApplyToOriginal()
        {
            var file = new CredentialConfigurationFile(
                Path.GetTempFileName(),
                new CredentialConfiguration(
                    SampleWorkloadIdentityPoolConfiguration,
                    SampleCommandLineOptions)
                {
                    ServiceAccountEmail = SampleServiceAccountEmail
                });
            file.Configuration.ServiceAccountEmail = "sa-1@test.iam.gserviceaccount.com";
            file.Configuration.Options.Executable = "test.exe";

            var clone = file.Clone();
            clone.Configuration.ServiceAccountEmail = "sa-2@test.iam.gserviceaccount.com";
            clone.Configuration.Options.Executable = "foo.exe";

            Assert.AreEqual("sa-1@test.iam.gserviceaccount.com", file.Configuration.ServiceAccountEmail);
            Assert.AreEqual("test.exe", file.Configuration.Options.Executable);
        }
    }
}
