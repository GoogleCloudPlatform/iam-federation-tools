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

using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace Google.Solutions.WWAuth.Test
{
    [TestFixture]
    public class TestUnattendedCommandLineOptions
    {
        [Test]
        public void WhenCommandLineHasNoArguments_ThenExecutableIsSet()
        {
            var options = UnattendedCommandLineOptions.Parse("test.exe");
            Assert.AreEqual("test.exe", options.Executable);
        }

        [Test]
        public void WhenCommandLineHasNoArguments_ThenValidateThrowsException()
        {
            var options = UnattendedCommandLineOptions.Parse("test.exe");
            Assert.Throws<ValidationException>(() => options.Validate());
        }
    }

    [TestFixture]
    public class TestAttendedCommandLineOptions
    {
        [Test]
        public void WhenCommandLineHasNoArguments_ThenExecutableIsSet()
        {
            var options = AttendedCommandLineOptions.Parse("test.exe");
            Assert.AreEqual("test.exe", options.Executable);
        }

        [Test]
        public void WhenEditPointsToNonexistingFile_ThenValidateThrowsException()
        {
            var options = AttendedCommandLineOptions.Parse("test.exe /edit doesnotexist.txt");
            Assert.Throws<ValidationException>(() => options.Validate());
        }
    }
}
