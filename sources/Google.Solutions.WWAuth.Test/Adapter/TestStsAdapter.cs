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

using Google.Apis.Logging;
using Google.Solutions.WWAuth.Adapters;
using Google.Solutions.WWAuth.Data;
using Moq;
using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace Google.Solutions.WWAuth.Test.Adapter
{
    [TestFixture]
    public class TestStsAdapter
    {
        //---------------------------------------------------------------------
        // ExchangeTokenAsync.
        //---------------------------------------------------------------------

        [Test]
        public async Task WhenAudienceInvalid_ThenExchangeTokenThrowsExceptionWithProperMessage()
        {
            var adapter = new StsAdapter(
                "//iam.googleapis.com/projects/PROJECT_NUMBER/locations/LOCATION/workloadIdentityPools/WORKLOAD_POOL_ID/providers/PROVIDER_ID",
                new NullLogger());

            var token = new Mock<ISubjectToken>();
            token.SetupGet(t => t.Type).Returns(SubjectTokenType.Jwt);
            token.SetupGet(t => t.Value).Returns("token");
            try
            {
                await adapter.ExchangeTokenAsync(
                        token.Object,
                        CredentialConfiguration.DefaultScopes,
                        CancellationToken.None)
                    .ConfigureAwait(false);
                Assert.Fail("Expected exception");
            }
            catch (TokenExchangeException e)
            {
                StringAssert.Contains("Invalid value for \"audience\"", e.Message);
            }
        }
    }
}
