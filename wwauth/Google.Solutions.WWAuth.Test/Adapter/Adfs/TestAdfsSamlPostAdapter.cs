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
    public class TestAdfsSamlPostAdapter
    {
        [Test]
        public void WhenRelyingPartyIdNotWellFormed_ThenConstructorThrowsException()
        {
            Assert.Throws<ArgumentException>(
                () => new AdfsSamlPostAdapter(
                    new Uri("https://example.com/"),
                    "not-a-url",
                    "https://acs.example.com/",
                    null,
                    new NullLogger()));
        }

        [Test]
        public void WhenAcsNotWellFormed_ThenConstructorThrowsException()
        {
            Assert.Throws<ArgumentException>(
                () => new AdfsSamlPostAdapter(
                    new Uri("https://example.com/"),
                    "https://rp.example.com/",
                    "not-a-url",
                    null,
                    new NullLogger()));
        }

        [Test]
        public void WhenIssuerLacksTrailingSlash_ThenUrlIsNormalized()
        {
            var adapter = new AdfsSamlPostAdapter(
                new Uri("https://example.com/adfs"),
                "https://rp.example.com",
                "https://acs.example.com",
                null,
                new NullLogger());

            Assert.AreEqual("https://example.com/adfs/ls", adapter.Request.Destination);
        }

        //---------------------------------------------------------------------
        // HtmlResponse.
        //---------------------------------------------------------------------

        [Test]
        public void WhenHtmlHasNoForms_ThenIsLoginFormReturnsFalse()
        {
            var response = new AdfsSamlPostAdapter.HtmlResponse("<html/>");

            Assert.IsFalse(response.IsSamlLoginForm);
        }

        [Test]
        public void WhenHtmlHasFormWithSamlRequest_ThenIsLoginFormReturnsTrue()
        {
            var response = new AdfsSamlPostAdapter.HtmlResponse(@"<html>
                <body>
                    <form> </form>
                    <form action=''></form>
                    <form action='http://acs/?SAMLRequest=xx' method=POST> </form>
                </body>
                </html>");
            Assert.IsTrue(response.IsSamlLoginForm);
        }

        [Test]
        public void WhenHtmlHasFormWithSamlResponse_ThenIsPostbackFormReturnsTrue()
        {
            var response = new AdfsSamlPostAdapter.HtmlResponse(@"<html>
                <body>
                    <form>
                        <input value=""assertion"" name=""SAMLResponse"" id=a></input>
                </body>
                </html>");
            Assert.IsTrue(response.IsSamlPostbackForm);
        }

        [Test]
        public void WhenHtmlHasFormWithSamlResponse_ThenResponseIsSet()
        {
            var response = new AdfsSamlPostAdapter.HtmlResponse(@"<html>
                <body>
                    <form>
                        <input value = ""assertion""
                         name = ""SAMLResponse"" id=a></input>
                </body>
                </html>");
            Assert.AreEqual("assertion", response.SamlResponse);
        }

        [Test]
        public void WhenHtmlContainsError_ThenErrorReturnsDecodedText()
        {
            var response = new AdfsSamlPostAdapter.HtmlResponse(@"<html>
                <li>Error details: MSIS3200: No AssertionConsumerService is configured on the relying party trust &#39;https://...&#39; specified by the request.</li>
                </html>");

            Assert.AreEqual("MSIS3200: No AssertionConsumerService is configured on the relying party trust 'https://...' specified by the request.", response.Error);
        }
    }
}
