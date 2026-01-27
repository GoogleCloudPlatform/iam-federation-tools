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
using Google.Solutions.WWAuth.Adapters.Adfs;
using NUnit.Framework;
using System;

namespace Google.Solutions.WWAuth.Test.Adapter.Adfs
{
    [TestFixture]
    public class TestAdfsWsTrustAdapter
    {
        [Test]
        public void WhenRelyingPartyIdNotWellFormed_ThenConstructorThrowsException()
        {
            Assert.That(
                () => new AdfsWsTrustAdapter(
                    new Uri("https://example.com/"),
                    "not-a-url",
                    new NullLogger()),
                Throws.ArgumentException);
        }
    }
}
