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

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Google.Solutions.AAAuth.Iam
{
    /// <summary>
    /// Uniquely identifies a workforce identity provider.
    /// </summary>
    public record class WorkforceIdentityProviderName(
        string PoolId,
        string ProviderId)
    {
        private const string ResourceNamePattern =
            @"^(//iam.googleapis.com/)?locations/global/" +
            @"workforcePools/(?<POOL>[^/]+)/providers/(?<PROVIDER>[^/]+)$";

        private static readonly Regex ResourceNameRegex
            = new Regex(ResourceNamePattern);

        /// <summary>
        /// Full resource name.
        /// </summary>
        public string ResourceName
        {
            get =>
                $"//iam.googleapis.com" +
                $"/locations/global/workforcePools/{this.PoolId}" +
                $"/providers/{this.ProviderId}";
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.ResourceName;
        }

        /// <summary>
        /// Parse a full resource name in the format
        ///   locations/global/workforcePools/POOL/providers/PROVIDER
        /// </summary>
        public static bool TryParse(
            string? resourceName,
            [NotNullWhen(true)] out WorkforceIdentityProviderName? result)
        {
            result = null;

            if (resourceName == null)
            {
                return false;
            }

            var match = ResourceNameRegex.Match(resourceName);
            if (match.Success)
            {
                result = new WorkforceIdentityProviderName(
                    match.Groups["POOL"].Value,
                    match.Groups["PROVIDER"].Value);
            }

            return result != null;
        }
    }
}
