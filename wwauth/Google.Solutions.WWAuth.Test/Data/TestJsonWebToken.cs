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

using Google.Apis.Json;
using Google.Solutions.WWAuth.Data;
using Google.Solutions.WWAuth.Util;
using NUnit.Framework;
using System;
using System.Text;

namespace Google.Solutions.WWAuth.Test.Data
{
    [TestFixture]
    public class TestJsonWebToken
    {
        private static JsonWebToken CreateJwt(object body)
        {
            var encodedBody = UrlSafeBase64.Encode(
                Encoding.UTF8.GetBytes(
                    NewtonsoftJsonSerializer.Instance.Serialize(body)));

            return new JsonWebToken(
                $"HEADER.{encodedBody}.FOOTER",
                DateTimeOffset.UtcNow.AddMinutes(1));
        }

        //---------------------------------------------------------------------
        // Issuer.
        //---------------------------------------------------------------------

        [Test]
        public void WhenJwtLacksIssuerClaim_ThenIssuerIsNull()
        {
            var jwt = CreateJwt(new
            {
                sub = "subject"
            });

            Assert.IsNull(jwt.Issuer);
        }

        [Test]
        public void WhenJwtHasIssuerClaim_ThenIssuerIsNotNull()
        {
            var jwt = CreateJwt(new
            {
                sub = "subject",
                iss = "issuer"
            });

            Assert.AreEqual("issuer", jwt.Issuer);
        }

        //---------------------------------------------------------------------
        // Audience.
        //---------------------------------------------------------------------

        [Test]
        public void WhenJwtLacksAudienceClaim_ThenAudienceIsNull()
        {
            var jwt = CreateJwt(new
            {
                sub = "subject"
            });

            Assert.IsNull(jwt.Audience);
        }

        [Test]
        public void WhenJwtHasAudienceClaim_ThenAudienceIsNotNull()
        {
            var jwt = CreateJwt(new
            {
                sub = "subject",
                aud = "audience"
            });

            Assert.AreEqual("audience", jwt.Audience);
        }

        //---------------------------------------------------------------------
        // Attributes.
        //---------------------------------------------------------------------

        [Test]
        public void WhenJwtContainsNestedArraysAndObjects_ThenAttributesReturnsFlattedList()
        {
            var jwt = CreateJwt(new
            {
                scalarInt = 1,
                scalarString = "test",
                arrayInt = new[] { 1, 2, 3 },
                arrayString = new[] { "one", "two", "three" },
                nested = new
                {
                    scalarInt = 1
                }
            });

            var attributes = jwt.Attributes;

            Assert.AreEqual(9, attributes.Count);
            Assert.AreEqual(1, attributes["assertion.scalarInt"]);
            Assert.AreEqual("test", attributes["assertion.scalarString"]);
            Assert.AreEqual(1, attributes["assertion.arrayInt[0]"]);
            Assert.AreEqual(2, attributes["assertion.arrayInt[1]"]);
            Assert.AreEqual(3, attributes["assertion.arrayInt[2]"]);
            Assert.AreEqual("one", attributes["assertion.arrayString[0]"]);
            Assert.AreEqual("two", attributes["assertion.arrayString[1]"]);
            Assert.AreEqual("three", attributes["assertion.arrayString[2]"]);
            Assert.AreEqual(1, attributes["assertion.nested.scalarInt"]);
        }
    }
}
