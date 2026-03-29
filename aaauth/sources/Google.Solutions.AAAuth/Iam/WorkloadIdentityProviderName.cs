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
using System.Text.RegularExpressions;

namespace Google.Solutions.AAAuth.Iam
{
    /// <summary>
    /// Uniquely identifies a workload identity provider.
    /// </summary>
    public record class WorkloadIdentityProviderName(
        long ProjectNumber,
        string PoolId,
        string ProviderId)
    {
        private const string ResourceNamePattern =
            @"^(//iam.googleapis.com/)?projects/(?<PROJECT>\d+)/locations/global/" +
            @"workloadIdentityPools/(?<POOL>[^/]+)/providers/(?<PROVIDER>[^/]+)$";

        private static readonly Regex ResourceNameRegex = new(ResourceNamePattern);

        /// <summary>
        /// Full resource name.
        /// </summary>
        public string ResourceName
        {
            get =>
                $"//iam.googleapis.com/projects/{this.ProjectNumber}" +
                $"/locations/global/workloadIdentityPools/{this.PoolId}" +
                $"/providers/{this.ProviderId}";
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.ResourceName;
        }

        /// <summary>
        /// Parse a full resource name in the format
        ///   projects/PROJECT/locations/global/
        ///   workloadIdentityPools/POOL/providers/PROVIDER
        /// </summary>
        public static WorkloadIdentityProviderName Parse(string resourceName)
        {
            var match = ResourceNameRegex.Match(resourceName);
            if (match.Success &&
                long.TryParse(
                    match.Groups["PROJECT"].Value,
                    out var projectNumber))
            {
                return new WorkloadIdentityProviderName(
                    projectNumber,
                    match.Groups["POOL"].Value,
                    match.Groups["PROVIDER"].Value);
            }
            else
            {
                throw new FormatException(
                    "The resource name of the workload identity provider is " +
                    "invalid, the name must match the pattern " +
                    $"'{ResourceNamePattern}'");
            }
        }
    }
}
