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
using NUnit.Framework;
using System;

namespace Google.Solutions.WWAuth.Test.Data
{
    [TestFixture]
    public class TestCredentialConfiguration
    {
        private const string SampleWorkloadAudience =
            "//iam.googleapis.com/projects/123/locations/global/workloadIdentityPools/" +
            "pool-1/providers/provider-1";

        private const string SampleWorkforceAudience =
            "//iam.googleapis.com/locations/global/workforcePools/" +
            "pool-1/providers/provider-1";

        private const string SampleServiceAccountImpersonationUrl =
            "https://iamcredentials.googleapis.com/v1/projects/-/serviceAccounts/" +
            "test@example.iam.gserviceaccount.com:generateAccessToken";

        private const string SampleServiceAccountEmail = "sa@example.iam.gserviceaccount.com";

        private static WorkloadIdentityPoolConfiguration SampleWorkloadPoolConfiguration
            => new WorkloadIdentityPoolConfiguration()
            {
                ProjectNumber = 1,
                Location = "local",
                PoolName = "pool-1",
                ProviderName = "provider-1"
            };

        private static WorkforceIdentityPoolConfiguration SampleWorkforcePoolConfiguration
            => new WorkforceIdentityPoolConfiguration()
            {
                UserProjectNumber = 1,
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
        // Validate.
        //---------------------------------------------------------------------

        [Test]
        public void WhenPoolConfigurationIncomplete_ThenValidateThrowsException(
            [Values(null, "")] string missingValue)
        {
            var poolConfiguration = SampleWorkloadPoolConfiguration;
            poolConfiguration.ProviderName = missingValue;

            var configuration = new CredentialConfiguration(
                poolConfiguration,
                SampleCommandLineOptions)
            {
                ServiceAccountEmail = SampleServiceAccountEmail
            };

            Assert.Throws<InvalidCredentialConfigurationException>(() => configuration.Validate());
        }

        [Test]
        public void WhenIssuerUrlMissing_ThenValidateThrowsException(
            [Values(null, "")] string missingValue)
        {
            var configuration = new CredentialConfiguration(
                SampleWorkloadPoolConfiguration,
                SampleCommandLineOptions)
            {
                ServiceAccountEmail = SampleServiceAccountEmail
            };

            configuration.Validate();

            configuration.Options.IssuerUrl = missingValue;
            Assert.Throws<InvalidCredentialConfigurationException>(() => configuration.Validate());
        }

        [Test]
        public void WhenAdfsRelyingPartyIdMissing_ThenValidateThrowsException(
            [Values(null, "")] string missingValue)
        {
            var configuration = new CredentialConfiguration(
                SampleWorkloadPoolConfiguration,
                SampleCommandLineOptions)
            {
                ServiceAccountEmail = SampleServiceAccountEmail
            };

            configuration.Validate();

            configuration.Options.RelyingPartyId = missingValue;
            Assert.Throws<InvalidCredentialConfigurationException>(() => configuration.Validate());
        }

        [Test]
        public void WhenServiceAccountEmailInvalid_ThenValidateThrowsException()
        {
            var configuration = new CredentialConfiguration(
                SampleWorkloadPoolConfiguration,
                SampleCommandLineOptions)
            {
                ServiceAccountEmail = "not-an-email"
            };

            Assert.Throws<InvalidCredentialConfigurationException>(() => configuration.Validate());
        }

        [Test]
        public void WhenServiceAccountEmailMissing_ThenValidateSucceeds(
            [Values(null, "")] string missingValue)
        {
            var configuration = new CredentialConfiguration(
                SampleWorkloadPoolConfiguration,
                SampleCommandLineOptions)
            {
                ServiceAccountEmail = SampleServiceAccountEmail
            };

            configuration.ServiceAccountEmail = missingValue;
            configuration.Validate();
        }

        //---------------------------------------------------------------------
        // ToJsonStructure - workload identity.
        //---------------------------------------------------------------------

        [Test]
        public void WhenWorkloadIdentityConfigurationIncomplete_ThenToJsonStructureThrowsException()
        {
            var configuration = CredentialConfiguration.NewWorkloadIdentityConfiguration();

            Assert.Throws<InvalidCredentialConfigurationException>(
                () => configuration.ToJsonStructure());
        }
        [Test]
        public void WhenWorkforceIdentityConfigurationIncomplete_ThenToJsonStructureThrowsException()
        {
            var configuration = CredentialConfiguration.NewWorkforceIdentityConfiguration();

            Assert.Throws<InvalidCredentialConfigurationException>(
                () => configuration.ToJsonStructure());
        }

        [Test]
        public void WhenWorkloadIdentityConfigurationValid_ThenToJsonStructureSucceeds()
        {
            var configuration = new CredentialConfiguration(
                SampleWorkloadPoolConfiguration,
                SampleCommandLineOptions)
            {
                ServiceAccountEmail = SampleServiceAccountEmail,
                Timeout = TimeSpan.FromSeconds(60)
            };

            var info = configuration.ToJsonStructure();
            Assert.AreEqual("external_account", info.Type);
            Assert.AreEqual(StsAdapter.DefaultTokenUrl, info.TokenUrl);
            Assert.AreEqual("//iam.googleapis.com/projects/1/locations/local/workloadIdentityPools/pool-1/providers/provider-1", info.Audience);
            Assert.AreEqual("https://iamcredentials.googleapis.com/v1/projects/-/serviceAccounts/sa@example.iam.gserviceaccount.com:generateAccessToken", info.ServiceAccountImpersonationUrl);
            Assert.AreEqual("urn:ietf:params:oauth:token-type:jwt", info.SubjectTokenType);
            Assert.AreEqual(60000, info.CredentialSource.Executable.TimeoutMillis);
            Assert.AreEqual("test.exe " +
                    "/IssuerUrl https://example.com/adfs/ " +
                    "/Protocol AdfsOidc " +
                    "/RelyingPartyId https://rp.example.com/ " +
                    "/OidcClientId client-1",
                info.CredentialSource.Executable.Command);
        }

        [Test]
        public void WhenWorkloadIdentitServiceAccountNotSet_ThenToJsonStructureSucceeds(
            [Values("", null)] string missingValue)
        {
            var configuration = new CredentialConfiguration(
                SampleWorkloadPoolConfiguration,
                SampleCommandLineOptions)
            {
                ServiceAccountEmail = missingValue,
                Timeout = TimeSpan.FromSeconds(60)
            };

            var info = configuration.ToJsonStructure();
            Assert.IsNull(info.ServiceAccountImpersonationUrl);
        }

        //---------------------------------------------------------------------
        // ToJsonStructure - workforce identity.
        //---------------------------------------------------------------------

        [Test]
        public void WhenUsingWorkforceIdentityPool_ThenToJsonStructureReturnsFullyPopulatedStructure()
        {
            var configuration = new CredentialConfiguration(
                SampleWorkforcePoolConfiguration,
                SampleCommandLineOptions)
            {
                ServiceAccountEmail = SampleServiceAccountEmail,
                Timeout = TimeSpan.FromSeconds(60)
            };

            var info = configuration.ToJsonStructure();
            Assert.AreEqual("external_account", info.Type);
            Assert.AreEqual(StsAdapter.DefaultTokenUrl, info.TokenUrl);
            Assert.AreEqual("//iam.googleapis.com/locations/local/workforcePools/pool-1/providers/provider-1", info.Audience);
            Assert.IsNull(info.ServiceAccountImpersonationUrl);
            Assert.AreEqual("urn:ietf:params:oauth:token-type:jwt", info.SubjectTokenType);
            Assert.AreEqual(60000, info.CredentialSource.Executable.TimeoutMillis);
            Assert.AreEqual("test.exe " +
                    "/IssuerUrl https://example.com/adfs/ " +
                    "/Protocol AdfsOidc " +
                    "/RelyingPartyId https://rp.example.com/ " +
                    "/OidcClientId client-1",
                info.CredentialSource.Executable.Command);
            Assert.AreEqual(1, info.WorkforcePoolUserProject);
        }

        //---------------------------------------------------------------------
        // FromJsonStructure - workload identity.
        //---------------------------------------------------------------------

        [Test]
        public void WhenTypeUnknown_ThenFromJsonStructureThrowsException()
        {
            var info = new CredentialConfiguration.CredentialConfigurationInfo(
                "junk-type",
                StsAdapter.DefaultTokenUrl,
                SampleWorkloadAudience,
                null,
                SampleServiceAccountImpersonationUrl,
                SubjectTokenType.Jwt.GetDescription(),
                new CredentialConfiguration.CredentialSourceInfo(
                    new CredentialConfiguration.ExecutableInfo(
                        "test.exe",
                        null)));

            Assert.Throws<UnknownCredentialConfigurationException>(
                () => CredentialConfiguration.FromJsonStructure(info));
        }

        [Test]
        public void WhenCredentialSourceMissing_ThenFromJsonStructureThrowsException()
        {
            var info = new CredentialConfiguration.CredentialConfigurationInfo(
                CredentialConfiguration.CredentialConfigurationInfo.ExternalAccount,
                StsAdapter.DefaultTokenUrl,
                SampleWorkloadAudience,
                null,
                SampleServiceAccountImpersonationUrl,
                SubjectTokenType.Jwt.GetDescription(),
                null);

            Assert.Throws<InvalidCredentialConfigurationException>(
                () => CredentialConfiguration.FromJsonStructure(info));
        }

        [Test]
        public void WhenExecutableMissing_ThenFromJsonStructureThrowsException()
        {
            var info = new CredentialConfiguration.CredentialConfigurationInfo(
                CredentialConfiguration.CredentialConfigurationInfo.ExternalAccount,
                StsAdapter.DefaultTokenUrl,
                SampleWorkloadAudience,
                null,
                SampleServiceAccountImpersonationUrl,
                SubjectTokenType.Jwt.GetDescription(),
                new CredentialConfiguration.CredentialSourceInfo(null));

            Assert.Throws<InvalidCredentialConfigurationException>(
                () => CredentialConfiguration.FromJsonStructure(info));
        }

        [Test]
        public void WhenAudienceMissing_ThenFromJsonStructureThrowsException()
        {
            var info = new CredentialConfiguration.CredentialConfigurationInfo(
                CredentialConfiguration.CredentialConfigurationInfo.ExternalAccount,
                StsAdapter.DefaultTokenUrl,
                "not-a-proper-audience",
                null,
                SampleServiceAccountImpersonationUrl,
                SubjectTokenType.Jwt.GetDescription(),
                new CredentialConfiguration.CredentialSourceInfo(
                    new CredentialConfiguration.ExecutableInfo(
                        "test.exe",
                        null)));

            Assert.Throws<InvalidCredentialConfigurationException>(
                () => CredentialConfiguration.FromJsonStructure(info));
        }

        [Test]
        public void WhenWorkloadIdentityServiceAccountMissing_ThenFromJsonStructureSucceeds()
        {
            var info = new CredentialConfiguration.CredentialConfigurationInfo(
                CredentialConfiguration.CredentialConfigurationInfo.ExternalAccount,
                StsAdapter.DefaultTokenUrl,
                SampleWorkloadAudience,
                null,
                null,
                SubjectTokenType.Jwt.GetDescription(),
                new CredentialConfiguration.CredentialSourceInfo(
                    new CredentialConfiguration.ExecutableInfo(
                        "test.exe",
                        null)));

            var configuration = CredentialConfiguration.FromJsonStructure(info);
            Assert.IsNull(configuration.ServiceAccountEmail);
        }

        [Test]
        public void WhenWorkloadIdentityConfigurationValid_ThenFromJsonStructureSucceeds()
        {
            var info = new CredentialConfiguration.CredentialConfigurationInfo(
                CredentialConfiguration.CredentialConfigurationInfo.ExternalAccount,
                StsAdapter.DefaultTokenUrl,
                SampleWorkloadAudience,
                null,
                SampleServiceAccountImpersonationUrl,
                SubjectTokenType.Jwt.GetDescription(),
                new CredentialConfiguration.CredentialSourceInfo(
                    new CredentialConfiguration.ExecutableInfo(
                        "test.exe",
                        null)));

            var configuration = CredentialConfiguration.FromJsonStructure(info);
            Assert.AreEqual("test@example.iam.gserviceaccount.com", configuration.ServiceAccountEmail);
            Assert.IsInstanceOf<WorkloadIdentityPoolConfiguration>(configuration.PoolConfiguration);

            var poolConfiguration = (WorkloadIdentityPoolConfiguration)configuration.PoolConfiguration;
            Assert.AreEqual(123, poolConfiguration.ProjectNumber);
            Assert.AreEqual("pool-1", poolConfiguration.PoolName);
            Assert.AreEqual("global", poolConfiguration.Location);
            Assert.AreEqual("provider-1", poolConfiguration.ProviderName);
            Assert.AreEqual("test.exe", configuration.Options.Executable);
        }

        //---------------------------------------------------------------------
        // FromJsonStructure - workforce identity.
        //---------------------------------------------------------------------

        [Test]
        public void WhenWorkforceIdentityProjectNumberMissing_ThenFromJsonStructureThrowsException()
        {
            var info = new CredentialConfiguration.CredentialConfigurationInfo(
                CredentialConfiguration.CredentialConfigurationInfo.ExternalAccount,
                StsAdapter.DefaultTokenUrl,
                SampleWorkforceAudience,
                null,
                null,
                SubjectTokenType.Jwt.GetDescription(),
                new CredentialConfiguration.CredentialSourceInfo(
                    new CredentialConfiguration.ExecutableInfo(
                        "test.exe",
                        null)));

            Assert.Throws<InvalidCredentialConfigurationException>(
                () => CredentialConfiguration.FromJsonStructure(info));
        }

        [Test]
        public void WhenWorkforceIdentityConfigurationValid_ThenFromJsonStructureSucceeds()
        {
            var info = new CredentialConfiguration.CredentialConfigurationInfo(
                CredentialConfiguration.CredentialConfigurationInfo.ExternalAccount,
                StsAdapter.DefaultTokenUrl,
                SampleWorkforceAudience,
                123,
                null,
                SubjectTokenType.Jwt.GetDescription(),
                new CredentialConfiguration.CredentialSourceInfo(
                    new CredentialConfiguration.ExecutableInfo(
                        "test.exe",
                        null)));

            var configuration = CredentialConfiguration.FromJsonStructure(info);
            Assert.IsNull(configuration.ServiceAccountEmail);
            Assert.IsInstanceOf<WorkforceIdentityPoolConfiguration>(configuration.PoolConfiguration);

            var poolConfiguration = (WorkforceIdentityPoolConfiguration)configuration.PoolConfiguration;
            Assert.AreEqual("pool-1", poolConfiguration.PoolName);
            Assert.AreEqual("global", poolConfiguration.Location);
            Assert.AreEqual("provider-1", poolConfiguration.ProviderName);
            Assert.AreEqual(123, poolConfiguration.UserProjectNumber);
            Assert.AreEqual("test.exe", configuration.Options.Executable);
        }

        //---------------------------------------------------------------------
        // CreateNew.
        //---------------------------------------------------------------------

        [Test]
        public void WhenNewFileCreated_ThenExecutableIsInitialized()
        {
            var configuration = CredentialConfiguration.NewWorkloadIdentityConfiguration();
            Assert.IsNotNull(configuration.Options.Executable);

            StringAssert.Contains(
                ".exe",
                configuration.Options.Executable);
        }
    }
}
