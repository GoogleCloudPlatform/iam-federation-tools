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

using Google.Solutions.WWAuth.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Google.Solutions.WWAuth.Util
{
    public interface ICommandLineOptions
    {
        string Executable { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CommandLineArgumentAttribute : Attribute
    { }

    /// <summary>
    /// Generate and parse command line arguments based on 
    /// properties of a class.
    /// </summary>
    internal static class CommandLineParser
    {
        private static IEnumerable<PropertyInfo> GetSupportedProperties<TOptions>()
            => typeof(TOptions)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CustomAttributes
                    .EnsureNotNull()
                    .Any(a => a.AttributeType == typeof(CommandLineArgumentAttribute)))
                .Where(p => p.PropertyType.IsEnum || p.PropertyType == typeof(string));

        public static TOptions Parse<TOptions>(string commandLine)
            where TOptions : class, ICommandLineOptions, new()
        {
            var commandLineParts = commandLine
                .SplitQuotedString(' ')
                .Select(arg => arg.Trim())
                .Where(arg => !string.IsNullOrWhiteSpace(arg));

            var options = new TOptions()
            {
                Executable = commandLineParts.FirstOrDefault()
            };

            var arguments = commandLineParts
                .Skip(1) // Ignore the executable path
                .ToList();

            if ((arguments.Count % 2) != 0)
            {
                throw new ArgumentException(
                    "Command line contains too many options");
            }

            var properties = GetSupportedProperties<TOptions>();

            for (int i = 0; i < arguments.Count / 2; i++)
            {
                var key = arguments[i * 2];
                var value = arguments[i * 2 + 1];
                if (!key.StartsWith("/"))
                {
                    throw new ArgumentException(
                        "Unrecognized command line option: " + key);
                }

                var property = properties.FirstOrDefault(
                    p => p.Name.Equals(key.Substring(1), StringComparison.OrdinalIgnoreCase));

                if (property == null)
                {
                    throw new ArgumentException(
                        "Unrecognized command line option: " + key);
                }

                if (property.PropertyType.IsEnum)
                {
                    property.SetValue(options, Enum.Parse(property.PropertyType, value));
                }
                else
                {
                    property.SetValue(options, value);
                }
            }

            return options;
        }

        public static string ToString<TOptions>(TOptions options, bool useQuotes = true)
            where TOptions : ICommandLineOptions
        {
            var quote = useQuotes ? "\"" : string.Empty;

            string executablePath = options.Executable;
            if (executablePath.Contains(" ") && !useQuotes)
            {
                //
                // Some client libraries don't support quoted command
                // lines (b/238143555, b/237606033). That's a problem if
                // the executable path itself contains spaces, which is
                // reasonably likely on Windows.
                //
                // As a workaround, convert the executable path to
                // 8.3 notation, which is guaranteed not to include
                // spaces. Then it's safe to drop the quotes.
                //
                var shortNameBuffer = new StringBuilder(260);
                if (NativeMethods.GetShortPathName(
                    executablePath,
                    shortNameBuffer,
                    shortNameBuffer.Capacity) == 0)
                {
                    throw new Win32Exception("Failed to create short-path name for file");
                }

                executablePath = shortNameBuffer.ToString();
                Debug.Assert(!executablePath.Contains(" "));
            }

            return $"{quote}{executablePath}{quote} " + string.Join(
                " ",
                GetSupportedProperties<TOptions>()
                    .Where(p => p.PropertyType.IsEnum ||
                                p.GetValue(options) is string s && !string.IsNullOrEmpty(s))
                    .Select(p => $"/{p.Name} {quote}{p.GetValue(options)}{quote}"));
        }
    }
}
