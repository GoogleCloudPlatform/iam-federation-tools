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

namespace Google.Solutions.WWAuth.Test.Data
{
    [TestFixture]
    public class TestWorkloadIdentityPoolConfiguration
    {
        //---------------------------------------------------------------------
        // Audience.
        //---------------------------------------------------------------------

        [Test]
        public void WhenNotInitialized_ThenAudienceReturnsProperUrl()
        {
            var configuration = new WorkloadIdentityPoolConfiguration();

            Assert.That(
                configuration.Audience,
                Is.EqualTo("//iam.googleapis.com/projects/-/locations/global/workloadIdentityPools/-/providers/-"));
        }

        [Test]
        public void WhenInitialized_ThenAudienceReturnsProperUrl()
        {
            var configuration = new WorkloadIdentityPoolConfiguration()
            {
                ProjectNumber = 123,
                Location = "global",
                PoolName = "pool-1",
                ProviderName = "provider-1"
            };

            Assert.That(
                configuration.Audience,
                Is.EqualTo("//iam.googleapis.com/projects/123/locations/global/workloadIdentityPools/pool-1/providers/provider-1"));
        }

        //---------------------------------------------------------------------
        // Validate.
        //---------------------------------------------------------------------

        [Test]
        public void WhenProjectNumberMissing_ThenValidateThrowsException(
            [Values(null, 0ul)] ulong? missingValue)
        {
            var configuration = new WorkloadIdentityPoolConfiguration()
            {
                ProjectNumber = missingValue,
                Location = "global",
                PoolName = "pool-1",
                ProviderName = "provider-1"
            };

            Assert.That(() => configuration.Validate(), Throws.InstanceOf<InvalidCredentialConfigurationException>());
        }

        [Test]
        public void WhenPoolNameMissing_ThenValidateThrowsException(
            [Values(null, "")] string missingValue)
        {
            var configuration = new WorkloadIdentityPoolConfiguration()
            {
                ProjectNumber = 123,
                Location = "global",
                PoolName = missingValue,
                ProviderName = "provider-1"
            };

            Assert.That(() => configuration.Validate(), Throws.InstanceOf<InvalidCredentialConfigurationException>());
        }

        [Test]
        public void WhenWorkloadIdentityPoolProviderNameMissing_ThenValidateThrowsException(
            [Values(null, "")] string missingValue)
        {
            var configuration = new WorkloadIdentityPoolConfiguration()
            {
                ProjectNumber = 123,
                Location = "global",
                PoolName = "pool-1",
                ProviderName = missingValue
            };

            Assert.That(() => configuration.Validate(), Throws.InstanceOf<InvalidCredentialConfigurationException>());
        }

        //---------------------------------------------------------------------
        // TryParse.
        //---------------------------------------------------------------------

        [Test]
        public void WhenAudienceMalformed_ThenTryParseReturnsFalse(
            [Values(null, "", "//iam.googleapis.com/")] string audience)
        {
            Assert.That(WorkloadIdentityPoolConfiguration.TryParse(
                audience,
                out var _), Is.False);
        }

        [Test]
        public void WhenAudienceValid_ThenTryParseReturnsTrue()
        {
            var parsed = WorkloadIdentityPoolConfiguration.TryParse(
                "//iam.googleapis.com/projects/123/locations/global/workloadIdentityPools/" +
                "pool-1/providers/provider-1",
                out var config);

            Assert.That(parsed, Is.True);
            Assert.That(config, Is.Not.Null);

            Assert.That(config.ProjectNumber, Is.EqualTo(123));
            Assert.That(config.PoolName, Is.EqualTo("pool-1"));
            Assert.That(config.Location, Is.EqualTo("global"));
            Assert.That(config.ProviderName, Is.EqualTo("provider-1"));
        }
    }
}
