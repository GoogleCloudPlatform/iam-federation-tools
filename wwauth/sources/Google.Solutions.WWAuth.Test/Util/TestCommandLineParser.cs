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
using System.IO;

namespace Google.Solutions.WWAuth.Test.Util
{
    [TestFixture]
    public class TestCommandLineParser
    {
        public enum Colors
        {
            Blue,
            White
        }

        public class Options : ICommandLineOptions
        {
            [CommandLineArgument]
            public string StringOne { get; set; }

            [CommandLineArgument]
            public string StringTwo { get; set; }

            private string IgnoreMe { get; set; } = "ignore";

            [CommandLineArgument]
            public int IntOne { get; set; }

            [CommandLineArgument]
            public Colors EnumOne { get; set; }

            public string Executable { get; set; }
        }

        //---------------------------------------------------------------------
        // Parse.
        //---------------------------------------------------------------------

        [Test]
        public void WhenCommandLineEmpty_ThenParseSucceeds()
        {
            var options = CommandLineParser.Parse<Options>("foo.exe");
            Assert.IsNotNull(options);
        }

        [Test]
        public void WhenCommandLineContainsExtraArguments_ThenParseThrowsException()
        {
            Assert.Throws<ArgumentException>(
                () => CommandLineParser.Parse<Options>("foo.exe /stringone"));
        }

        [Test]
        public void WhenCommandLineContainsArgumentWitoutSlash_ThenParseThrowsException()
        {
            Assert.Throws<ArgumentException>(
                () => CommandLineParser.Parse<Options>("foo.exe stringone value"));
        }

        [Test]
        public void WhenCommandLineReferencesInvalidProperty_ThenParseThrowsException()
        {
            Assert.Throws<ArgumentException>(
                () => CommandLineParser.Parse<Options>("foo.exe intone value"));
            Assert.Throws<ArgumentException>(
                () => CommandLineParser.Parse<Options>("foo.exe /Executable value"));
            Assert.Throws<ArgumentException>(
                () => CommandLineParser.Parse<Options>("foo.exe /nonexistingproperty value"));
            Assert.Throws<ArgumentException>(
                () => CommandLineParser.Parse<Options>("foo.exe /enumone value"));
        }

        [Test]
        public void WhenCommandLineValid_ThenParseSuccceeds()
        {
            var options = CommandLineParser.Parse<Options>(
                "'c:\\path to\\foo.exe' /stringONE 'some value' /stringtwo \" some more\" /enumone White");
            Assert.IsNotNull(options);
            Assert.AreEqual("some value", options.StringOne);
            Assert.AreEqual("some more", options.StringTwo);
            Assert.AreEqual(Colors.White, options.EnumOne);
        }

        //---------------------------------------------------------------------
        // ToString.
        //---------------------------------------------------------------------

        [Test]
        public void WhenCommandLineCreatedByToString_ThenParseReturnsEquivalentOptions()
        {
            var options = new Options()
            {
                Executable = "foo.exe",
                StringOne = "first value",
                StringTwo = "second value",
                EnumOne = Colors.White
            };

            var cmd = CommandLineParser.ToString(options);
            var parsed = CommandLineParser.Parse<Options>(cmd);
            Assert.AreEqual(options.StringOne, parsed.StringOne);
            Assert.AreEqual(options.StringTwo, parsed.StringTwo);
            Assert.AreEqual(Colors.White, parsed.EnumOne);
        }

        [Test]
        public void WhenPropertyIsNullOrEmpty_TnenToStringIgnoresProperty()
        {
            var options = new Options()
            {
                Executable = "foo.exe",
                StringOne = null,
                StringTwo = string.Empty
            };

            Assert.AreEqual("\"foo.exe\" /EnumOne \"Blue\"", CommandLineParser.ToString(options));
        }

        [Test]
        public void WhenQuotesDisabled_TnenToStringReturnsUnquotedString()
        {
            var options = new Options()
            {
                Executable = "c:\\pathto\\foo.exe",
                StringOne = null,
                StringTwo = string.Empty
            };

            Assert.AreEqual("c:\\pathto\\foo.exe /EnumOne Blue", CommandLineParser.ToString(options, false));
        }

        [Test]
        public void WhenQuotesDisabledAndExecutableContainsSpaces_TnenExecutablePathIsConvertedTo8dot3()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("KOKORO_BUILD_ID")))
            {
                Assert.Inconclusive("8.3 filenames are disabled in Kokoro");
                return;
            }

            var tempFolderPath = Path.Combine(Path.GetTempPath(), "long folder name with spaces");
            Directory.CreateDirectory(tempFolderPath);

            var exePath = Path.Combine(tempFolderPath, "foo.exe");
            File.WriteAllText(exePath, string.Empty);

            var options = new Options()
            {
                Executable = exePath,
                StringOne = null,
                StringTwo = string.Empty
            };

            var commandLine = CommandLineParser.ToString(options, false);

            StringAssert.DoesNotContain(
                " ",
                commandLine.Substring(0, commandLine.IndexOf("foo.exe")));
        }
    }
}
