//
// Copyright 2026 Google LLC
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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Google.Solutions.AAAuth.Web
{
    /// <summary>
    /// Formatter that emits structured logs in the format expected
    /// by Cloud Run.
    /// </summary>
    public class CloudRunLogFormatter : ConsoleFormatter
    {
        public const string FormatterName = nameof(CloudRunLogFormatter);

        public CloudRunLogFormatter() : base(FormatterName)
        {
        }

        public override void Write<TState>(
            in LogEntry<TState> logEntry,
            IExternalScopeProvider? scopeProvider,
            TextWriter textWriter)
        {
            var customLog = new Dictionary<string, object>()
            {
                //
                // Use severities as defined in
                // https://cloud.google.com/logging/docs/reference/v2/rest/v2/LogEntry
                //
                {
                    "severity",
                    logEntry.LogLevel switch
                    {
                        LogLevel.Information => "INFO",
                        LogLevel.Warning => "WARNING",
                        LogLevel.Error => "ERROR",
                        LogLevel.Critical => "CRITICAL",
                        _ => "DEBUG",
                    }
                },
                { "message", logEntry.Formatter(logEntry.State, logEntry.Exception) },
                {
                    "logging.googleapis.com/labels",
                    new {
                        category = logEntry.Category,
                        event_id = logEntry.EventId,
                        exception = logEntry.Exception?.ToString()
                    }
                }
            };

            textWriter.WriteLine(JsonSerializer.Serialize(customLog));
        }
    }
}
