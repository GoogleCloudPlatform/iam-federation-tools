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
using System.IO;

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

            Assert.That(
                () => configuration.Validate(),
                Throws.InstanceOf<InvalidCredentialConfigurationException>());
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
            Assert.That(
                () => configuration.Validate(),
                Throws.InstanceOf<InvalidCredentialConfigurationException>());
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
            Assert.That(
                () => configuration.Validate(),
                Throws.InstanceOf<InvalidCredentialConfigurationException>());
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

            Assert.That(
                () => configuration.Validate(),
                Throws.InstanceOf<InvalidCredentialConfigurationException>());
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

            Assert.That(
                () => configuration.ToJsonStructure(),
                Throws.InstanceOf<InvalidCredentialConfigurationException>());
        }
        [Test]
        public void WhenWorkforceIdentityConfigurationIncomplete_ThenToJsonStructureThrowsException()
        {
            var configuration = CredentialConfiguration.NewWorkforceIdentityConfiguration();

            Assert.That(
                () => configuration.ToJsonStructure(),
                Throws.InstanceOf<InvalidCredentialConfigurationException>());
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
            Assert.That(info.Type, Is.EqualTo("external_account"));
            Assert.That(info.TokenUrl, Is.EqualTo(StsAdapter.DefaultTokenUrl));
            Assert.That(info.Audience, Is.EqualTo("//iam.googleapis.com/projects/1/locations/local/workloadIdentityPools/pool-1/providers/provider-1"));
            Assert.That(info.ServiceAccountImpersonationUrl, Is.EqualTo("https://iamcredentials.googleapis.com/v1/projects/-/serviceAccounts/sa@example.iam.gserviceaccount.com:generateAccessToken"));
            Assert.That(info.SubjectTokenType, Is.EqualTo("urn:ietf:params:oauth:token-type:jwt"));
            Assert.That(info.CredentialSource.Executable.TimeoutMillis, Is.EqualTo(60000));
            Assert.That(info.CredentialSource.Executable.Command, Is.EqualTo("test.exe " +
                    "/IssuerUrl https://example.com/adfs/ " +
                    "/Protocol AdfsOidc " +
                    "/RelyingPartyId https://rp.example.com/ " +
                    "/OidcClientId client-1"));
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
            Assert.That(info.ServiceAccountImpersonationUrl, Is.Null);
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
            Assert.That(info.Type, Is.EqualTo("external_account"));
            Assert.That(info.TokenUrl, Is.EqualTo(StsAdapter.DefaultTokenUrl));
            Assert.That(info.Audience, Is.EqualTo("//iam.googleapis.com/locations/local/workforcePools/pool-1/providers/provider-1"));
            Assert.That(info.ServiceAccountImpersonationUrl, Is.Null);
            Assert.That(info.SubjectTokenType, Is.EqualTo("urn:ietf:params:oauth:token-type:jwt"));
            Assert.That(info.CredentialSource.Executable.TimeoutMillis, Is.EqualTo(60000));
            Assert.That(info.CredentialSource.Executable.Command, Is.EqualTo("test.exe " +
                    "/IssuerUrl https://example.com/adfs/ " +
                    "/Protocol AdfsOidc " +
                    "/RelyingPartyId https://rp.example.com/ " +
                    "/OidcClientId client-1"));
            Assert.That(info.WorkforcePoolUserProject, Is.EqualTo(1));
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

            Assert.That(
                () => CredentialConfiguration.FromJsonStructure(info),
                Throws.InstanceOf<UnknownCredentialConfigurationException>());
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

            Assert.That(
                () => CredentialConfiguration.FromJsonStructure(info),
                Throws.InstanceOf<InvalidCredentialConfigurationException>());
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

            Assert.That(
                () => CredentialConfiguration.FromJsonStructure(info),
                Throws.InstanceOf<InvalidCredentialConfigurationException>());
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

            Assert.That(
                () => CredentialConfiguration.FromJsonStructure(info),
                Throws.InstanceOf<InvalidCredentialConfigurationException>());
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
            Assert.That(configuration.ServiceAccountEmail, Is.Null);
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
            Assert.That(configuration.ServiceAccountEmail, Is.EqualTo("test@example.iam.gserviceaccount.com"));
            Assert.That(configuration.PoolConfiguration, Is.InstanceOf<WorkloadIdentityPoolConfiguration>());

            var poolConfiguration = (WorkloadIdentityPoolConfiguration)configuration.PoolConfiguration;
            Assert.That(poolConfiguration.ProjectNumber, Is.EqualTo(123));
            Assert.That(poolConfiguration.PoolName, Is.EqualTo("pool-1"));
            Assert.That(poolConfiguration.Location, Is.EqualTo("global"));
            Assert.That(poolConfiguration.ProviderName, Is.EqualTo("provider-1"));
            Assert.That(configuration.Options.Executable, Is.EqualTo("test.exe"));
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

            Assert.That(
                () => CredentialConfiguration.FromJsonStructure(info),
                Throws.InstanceOf<InvalidCredentialConfigurationException>());
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
            Assert.That(configuration.ServiceAccountEmail, Is.Null);
            Assert.That(configuration.PoolConfiguration, Is.InstanceOf<WorkforceIdentityPoolConfiguration>());

            var poolConfiguration = (WorkforceIdentityPoolConfiguration)configuration.PoolConfiguration;
            Assert.That(poolConfiguration.PoolName, Is.EqualTo("pool-1"));
            Assert.That(poolConfiguration.Location, Is.EqualTo("global"));
            Assert.That(poolConfiguration.ProviderName, Is.EqualTo("provider-1"));
            Assert.That(poolConfiguration.UserProjectNumber, Is.EqualTo(123));
            Assert.That(configuration.Options.Executable, Is.EqualTo("test.exe"));
        }

        //---------------------------------------------------------------------
        // CreateNew.
        //---------------------------------------------------------------------

        [Test]
        public void WhenNewFileCreated_ThenExecutableIsInitialized()
        {
            var configuration = CredentialConfiguration.NewWorkloadIdentityConfiguration();
            Assert.That(configuration.Options.Executable, Is.Not.Null);
            Assert.That(configuration.Options.Executable, Does.Contain(".exe"));
        }

        //---------------------------------------------------------------------
        // ResetExecutable.
        //---------------------------------------------------------------------

        [Test]
        public void WhenExecutableInvalid_ThenResetExecutableUpdatesPath()
        {
            var configuration = CredentialConfiguration.NewWorkloadIdentityConfiguration();
            configuration.Options.Executable = "doesnotexist.exe";

            configuration.ResetExecutable();

            Assert.That(File.Exists(configuration.Options.Executable), Is.True);
        }
    }
}
