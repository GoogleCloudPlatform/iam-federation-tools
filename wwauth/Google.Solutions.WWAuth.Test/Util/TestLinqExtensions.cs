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

using Google.Solutions.WWAuth.Util;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Google.Solutions.WWAuth.Test.Util
{
    [TestFixture]
    public class TestLinqExtensions
    {
        //---------------------------------------------------------------------
        // EnsureNotNull.
        //---------------------------------------------------------------------

        [Test]
        public void WhenEnumIsNull_EnsureNotNullReturnsEmpty()
        {
            IEnumerable<string> e = null;
            Assert.That(e.EnsureNotNull(), Is.Not.Null);
            Assert.That(e.EnsureNotNull().Count(), Is.EqualTo(0));
        }

        //---------------------------------------------------------------------
        // TryGet.
        //---------------------------------------------------------------------

        [Test]
        public void WhenKeyFound_ThenTryGetReturnsValue()
        {
            var dict = new Dictionary<string, string>()
            {
                { "key", "value" },
            };
            Assert.That(dict.TryGet("key"), Is.EqualTo("value"));
        }

        [Test]
        public void WhenKeyNotFound_ThenTryGetReturnsNull()
        {
            var dict = new Dictionary<string, string>();
            Assert.That(dict.TryGet("key"), Is.Null);
        }

        //---------------------------------------------------------------------
        // SplitQuotedString.
        //---------------------------------------------------------------------

        [Test]
        public void WhenStringHasNoQuotes_ThenSplitQuotedStringSplits()
        {
            var s = "this is a string";
            Assert.That(
                s.SplitQuotedString(' '),
                Is.EquivalentTo(s.Split()));
        }

        [Test]
        public void WhenStringHasQuotes_ThenQuotedPartIsPreserved()
        {
            var s = "'this' is 'a string'";
            Assert.That(
                s.SplitQuotedString(' '),
                Is.EquivalentTo(new[] { "this", "is", "a string" }));
        }

        [Test]
        public void WhenStringHasMultipleQuotes_ThenQuotedPartIsPreserved()
        {
            var s = "this is 'a string' and \"here is\" another one";
            Assert.That(
                s.SplitQuotedString(' '),
                Is.EquivalentTo(new[] { "this", "is", "a string", "and", "here is", "another", "one" }));
        }
    }
}
