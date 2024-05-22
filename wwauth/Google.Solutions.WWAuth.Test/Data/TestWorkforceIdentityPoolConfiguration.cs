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
    public class TestWorkforceIdentityPoolConfiguration
    {
        //---------------------------------------------------------------------
        // Audience.
        //---------------------------------------------------------------------

        [Test]
        public void WhenNotInitialized_ThenAudienceReturnsProperUrl()
        {
            var configuration = new WorkforceIdentityPoolConfiguration();

            Assert.AreEqual(
                "//iam.googleapis.com/locations/global/workforcePools/-/providers/-",
                configuration.Audience);
        }

        [Test]
        public void WhenInitialized_ThenAudienceReturnsProperUrl()
        {
            var configuration = new WorkforceIdentityPoolConfiguration()
            {
                UserProjectNumber = 1,
                Location = "global",
                PoolName = "pool-1",
                ProviderName = "provider-1"
            };

            Assert.AreEqual(
                "//iam.googleapis.com/locations/global/workforcePools/pool-1/providers/provider-1",
                configuration.Audience);
        }

        //---------------------------------------------------------------------
        // Validate.
        //---------------------------------------------------------------------

        [Test]
        public void WhenUserProjectNumberMissing_ThenValidateThrowsException(
            [Values(null, 0ul)] ulong? missingValue)
        {
            var configuration = new WorkforceIdentityPoolConfiguration()
            {
                UserProjectNumber = missingValue,
                Location = "global",
                PoolName = "pool-1",
                ProviderName = "provider-1"
            };

            Assert.Throws<InvalidCredentialConfigurationException>(() => configuration.Validate());
        }

        [Test]
        public void WhenPoolNameMissing_ThenValidateThrowsException(
            [Values(null, "")] string missingValue)
        {
            var configuration = new WorkforceIdentityPoolConfiguration()
            {
                UserProjectNumber = 1,
                Location = "global",
                PoolName = missingValue,
                ProviderName = "provider-1"
            };

            Assert.Throws<InvalidCredentialConfigurationException>(() => configuration.Validate());
        }

        [Test]
        public void WhenWorkforceIdentityPoolProviderNameMissing_ThenValidateThrowsException(
            [Values(null, "")] string missingValue)
        {
            var configuration = new WorkforceIdentityPoolConfiguration()
            {
                UserProjectNumber = 1,
                Location = "global",
                PoolName = "pool-1",
                ProviderName = missingValue
            };

            Assert.Throws<InvalidCredentialConfigurationException>(() => configuration.Validate());
        }

        //---------------------------------------------------------------------
        // TryParse.
        //---------------------------------------------------------------------

        [Test]
        public void WhenAudienceMalformed_ThenTryParseReturnsFalse(
            [Values(null, "", "//iam.googleapis.com/")] string audience)
        {
            Assert.IsFalse(WorkforceIdentityPoolConfiguration.TryParse(
                audience,
                out var _));
        }

        [Test]
        public void WhenAudienceValid_ThenTryParseReturnsTrue()
        {
            var parsed = WorkforceIdentityPoolConfiguration.TryParse(
                "//iam.googleapis.com/locations/global/workforcePools/" +
                "pool-1/providers/provider-1",
                out var config);

            Assert.IsTrue(parsed);
            Assert.IsNotNull(config);

            Assert.AreEqual("pool-1", config.PoolName);
            Assert.AreEqual("global", config.Location);
            Assert.AreEqual("provider-1", config.ProviderName);
        }
    }
}
