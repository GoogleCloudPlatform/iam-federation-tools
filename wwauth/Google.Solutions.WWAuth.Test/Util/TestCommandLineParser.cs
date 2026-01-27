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
            Assert.That(options, Is.Not.Null);
        }

        [Test]
        public void WhenCommandLineContainsExtraArguments_ThenParseThrowsException()
        {
            Assert.That(
                () => CommandLineParser.Parse<Options>("foo.exe /stringone"),
                Throws.ArgumentException);
        }

        [Test]
        public void WhenCommandLineContainsArgumentWitoutSlash_ThenParseThrowsException()
        {
            Assert.That(
                () => CommandLineParser.Parse<Options>("foo.exe stringone value"),
                Throws.ArgumentException);
        }

        [Test]
        public void WhenCommandLineReferencesInvalidProperty_ThenParseThrowsException()
        {
            Assert.That(() => CommandLineParser.Parse<Options>("foo.exe intone value"), Throws.ArgumentException);
            Assert.That(() => CommandLineParser.Parse<Options>("foo.exe /Executable value"), Throws.ArgumentException);
            Assert.That(() => CommandLineParser.Parse<Options>("foo.exe /nonexistingproperty value"), Throws.ArgumentException);
            Assert.That(() => CommandLineParser.Parse<Options>("foo.exe /enumone value"), Throws.ArgumentException);
        }

        [Test]
        public void WhenCommandLineValid_ThenParseSuccceeds()
        {
            var options = CommandLineParser.Parse<Options>(
                "'c:\\path to\\foo.exe' /stringONE 'some value' /stringtwo \" some more\" /enumone White");
            Assert.That(options, Is.Not.Null);
            Assert.That(options.StringOne, Is.EqualTo("some value"));
            Assert.That(options.StringTwo, Is.EqualTo("some more"));
            Assert.That(options.EnumOne, Is.EqualTo(Colors.White));
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
            Assert.That(parsed.StringOne, Is.EqualTo(options.StringOne));
            Assert.That(parsed.StringTwo, Is.EqualTo(options.StringTwo));
            Assert.That(parsed.EnumOne, Is.EqualTo(Colors.White));
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

            Assert.That(CommandLineParser.ToString(options), Is.EqualTo("\"foo.exe\" /EnumOne \"Blue\""));
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

            Assert.That(CommandLineParser.ToString(options, false), Is.EqualTo("c:\\pathto\\foo.exe /EnumOne Blue"));
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

            Assert.That(commandLine.Substring(0, commandLine.IndexOf("foo.exe")), Does.Not.Contain(" "));
        }
    }
}
