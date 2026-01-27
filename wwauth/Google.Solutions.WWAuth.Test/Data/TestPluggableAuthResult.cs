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
using Moq;
using NUnit.Framework;
using System;
using System.IO;

namespace Google.Solutions.WWAuth.Test.Data
{
    [TestFixture]
    public class TestPluggableAuthResult
    {
        [Test]
        public void WhenTokenIsJwt_ThenIdTokenIsSet()
        {
            var token = new Mock<ISubjectToken>();
            token.SetupGet(t => t.Type).Returns(SubjectTokenType.Jwt);
            token.SetupGet(t => t.Value).Returns("token value");
            token.SetupGet(t => t.Expiry).Returns(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));

            var result = new PluggableAuthResult(token.Object);
            Assert.That(result.Success, Is.True);
            Assert.That(result.IdToken, Is.EqualTo("token value"));
        }

        [Test]
        public void WhenTokenIsSaml2_ThenSamlResponseIsSet()
        {
            var token = new Mock<ISubjectToken>();
            token.SetupGet(t => t.Type).Returns(SubjectTokenType.Saml2);
            token.SetupGet(t => t.Value).Returns("token value");
            token.SetupGet(t => t.Expiry).Returns(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));

            var result = new PluggableAuthResult(token.Object);
            Assert.That(result.Success, Is.True);
            Assert.That(result.SamlResponse, Is.EqualTo("token value"));
        }

        [Test]
        public void WhenCreatedWithException_ThenMessageContainsInnerExceptionDetails()
        {
            var exception = new AggregateException(
                new InvalidOperationException("IOE",
                    new ArgumentException("AE")));

            var result = new PluggableAuthResult(exception);
            Assert.That(result.Code, Is.EqualTo("InvalidOperationException"));
            Assert.That(result.Message, Does.Contain("IOE"));
            Assert.That(result.Message, Does.Contain("AE"));
        }

        [Test]
        public void WhenResultContainsToken_ThenWriteToWritesValue()
        {
            var token = new Mock<ISubjectToken>();
            token.SetupGet(t => t.Type).Returns(SubjectTokenType.Jwt);
            token.SetupGet(t => t.Value).Returns("token value");
            token.SetupGet(t => t.Expiry).Returns(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));

            var result = new PluggableAuthResult(token.Object);
            using (var writer = new StringWriter())
            {
                result.WriteTo(writer);
                writer.Flush();

                Assert.That(writer.ToString(), Does.Contain("token value"));
            }
        }
    }
}
