//
// Copyright 2026 Google LLC
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

using Google.Solutions.AAAuth.Iam;
using NUnit.Framework;

namespace Google.Solutions.AAAuth.Test.Iam
{
    [TestFixture]
    public class TestWorkforceIdentityProviderName
    {
        //---------------------------------------------------------------------
        // ResourceName.
        //---------------------------------------------------------------------

        [Test]
        public void ResourceName()
        {
            var id = new WorkforceIdentityProviderName(
                "pool-id",
                "provider-id");

            Assert.That(
                id.ResourceName,
                Is.EqualTo(
                    "//iam.googleapis.com/locations/global" +
                    "/workforcePools/pool-id/providers/provider-id"));
        }

        //---------------------------------------------------------------------
        // ToString.
        //---------------------------------------------------------------------

        [Test]
        public void ToString_ReturnsResourceName()
        {
            var name = new WorkforceIdentityProviderName(
                "pool-id",
                "name-1");

            Assert.That(name.ToString(), Is.EqualTo(name.ResourceName));
        }

        //---------------------------------------------------------------------
        // Parse.
        //---------------------------------------------------------------------

        [Test]
        public void TryParse_WhenInvalid(
            [Values(
                null,
                "",
                "///////",
                "locations/global/workloadIdentityPools/pool-id/providers/provider-id",
                "locations/local/workforcePools/pool/providers/provider",
                "projects/global/locations//workforcePools//providers/",
                "projects/x/locations/global/workforcePools/x/providers/x")]
            string value)
        {
            Assert.That(
                WorkforceIdentityProviderName.TryParse(value, out var _),
                Is.False);
        }

        [Test]
        public void TryParse()
        {
            var name = new WorkforceIdentityProviderName(
                "pool-id",
                "name-1");

            Assert.That(
                WorkforceIdentityProviderName.TryParse(
                    name.ResourceName,
                    out var parsed),
                Is.True);

            Assert.That(parsed, Is.EqualTo(name));
        }
    }
}
