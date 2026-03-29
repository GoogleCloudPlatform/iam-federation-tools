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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Google.Solutions.AAAuth.Web
{
    /// <summary>
    /// Packs a URI and a string into a single, URL-safe parameter.
    /// </summary>
    internal class PackedParameter(Uri uri, string data)
    {
        /// <summary>
        /// State, as provided by client.
        /// </summary>
        public string Data { get; } = data;

        /// <summary>
        /// Redirect URI, as provided by client.
        /// </summary>
        public Uri Uri { get; } = uri;

        public override string ToString()
        {
            return WebUtility.UrlEncode($"{this.Uri}\n{this.Data}");
        }

        public static bool TryParse(
            string s,
            [NotNullWhen(true)] out PackedParameter? state)
        {
            if (WebUtility.UrlDecode(s) is not string decoded ||
                decoded.Split('\n') is not string[] parts ||
                parts.Length != 2 ||
                !Uri.TryCreate(parts[0], UriKind.Absolute, out var uri) ||
                parts[1] is not string data)
            {
                state = null;
                return false;
            }
            else
            {
                state = new PackedParameter(uri, data);
                return true;
            }
        }
    }
}
