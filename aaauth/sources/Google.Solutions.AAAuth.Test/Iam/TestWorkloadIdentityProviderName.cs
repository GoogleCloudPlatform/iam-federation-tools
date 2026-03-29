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
using System;

namespace Google.Solutions.AAAuth.Test.Iam
{
    [TestFixture]
    public class TestWorkloadIdentityProviderName
    {
        //---------------------------------------------------------------------
        // ResourceName.
        //---------------------------------------------------------------------

        [Test]
        public void ResourceName()
        {
            var id = new WorkloadIdentityProviderName(
                12345,
                "pool-id",
                "provider-id");

            Assert.That(
                id.ResourceName,
                Is.EqualTo(
                    "//iam.googleapis.com/projects/12345/locations/global" +
                    "/workloadIdentityPools/pool-id/providers/provider-id"));
        }

        //---------------------------------------------------------------------
        // ToString.
        //---------------------------------------------------------------------

        [Test]
        public void ToString_ReturnsResourceName()
        {
            var name = new WorkloadIdentityProviderName(
                1234567890000,
                "pool-id",
                "name-1");

            Assert.That(name.ToString(), Is.EqualTo(name.ResourceName));
        }

        //---------------------------------------------------------------------
        // Parse.
        //---------------------------------------------------------------------

        [Test]
        public void Parse_WhenInvalid(
            [Values(
                "",
                "///////",
                "locations/global/workforcePools/pool/providers/provider",
                "projects//locations//workloadIdentityPools//providers/",
                "projects/x/locations/global/workloadIdentityPools/x/providers/x")]
            string value)
        {
            Assert.Throws<FormatException>(
                () => WorkloadIdentityProviderName.Parse(value));
        }

        [Test]
        public void Parse()
        {
            var name = new WorkloadIdentityProviderName(
                long.MaxValue,
                "pool-id",
                "name-1");
            var parsed = WorkloadIdentityProviderName.Parse(name.ResourceName);

            Assert.That(parsed, Is.EqualTo(name));
        }
    }
}
