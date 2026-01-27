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
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Google.Solutions.WWAuth.Test.Util
{
    [TestFixture]
    public class TestExceptionExtensions
    {
        //---------------------------------------------------------------------
        // Unwrap.
        //---------------------------------------------------------------------

        [Test]
        public void WhenRegularException_UnwrapDoesNothing()
        {
            var ex = new ApplicationException();

            var unwrapped = ex.Unwrap();

            Assert.That(unwrapped, Is.SameAs(ex));
        }

        [Test]
        public void WhenAggregateException_UnwrapReturnsFirstInnerException()
        {
            var inner1 = new ApplicationException();
            var inner2 = new ApplicationException();
            var aggregate = new AggregateException(inner1, inner2);

            var unwrapped = aggregate.Unwrap();

            Assert.That(unwrapped, Is.SameAs(inner1));
        }

        [Test]
        public void WhenTargetInvocationException_UnwrapReturnsInnerException()
        {
            var inner = new ApplicationException();
            var target = new TargetInvocationException("", inner);

            var unwrapped = target.Unwrap();

            Assert.That(unwrapped, Is.SameAs(inner));
        }

        //---------------------------------------------------------------------
        // FullMessage.
        //---------------------------------------------------------------------

        [Test]
        public void WhenExceptionHasNoInnerException_ThenFullMessageIsSameAsMessage()
        {
            var ex = new ArgumentException("something went wrong!");
            Assert.That(ex.FullMessage(), Is.EqualTo(ex.Message));
        }

        [Test]
        public void WhenExceptionHasInnerException_ThenFullMessageContainsAllMessages()
        {
            var ex = new ArgumentException("One",
                new InvalidOperationException("two",
                    new Exception("three")));
            Assert.That(ex.FullMessage(), Is.EqualTo("One: two: three"));
        }
    }
}
